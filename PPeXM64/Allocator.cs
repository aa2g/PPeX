using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PPeXM64
{
    public sealed class PoolPointer : IDisposable
    {
        public bool Fixed { get; set; } = false;

        public long Offset { get; internal set; }
        public long Length { get; internal set; }

        private Allocator Allocator { get; }

        public PoolPointer(Allocator allocator, long offset, long length, bool isFixed)
        {
            Allocator = allocator;
            Offset = offset;
            Length = length;
            Fixed = isFixed;
        }

        public IMemoryOwner<byte> GetReference()
        {
            return Allocator.GetMemory(this);
        }

        public void Release()
        {
            Allocator.Release(this);
        }

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        ~PoolPointer()
        {
            Release();
        }
    }

    public class Allocator
    {
        protected List<PoolPointer> PoolPointers { get; } = new List<PoolPointer>();
        protected List<PoolPointer> PinnedPointers { get; } = new List<PoolPointer>();

        public bool DefaultFixed { get; set; } = false;

        protected Memory<byte> UnderlyingMemory { get; }
        protected IntPtr HeapPointer { get; }

        public long PoolSize { get; }

        public Allocator(long poolSize)
        {
	        PoolSize = poolSize;

	        HeapPointer = Marshal.AllocHGlobal((IntPtr)poolSize);
            UnderlyingMemory = new UnmanagedMemory(HeapPointer, poolSize).Memory;
        }

        public long GetTotalAllocatedSize()
        {
	        lock (PoolPointers)
	        {
		        return PoolPointers.Sum(x => x.Length);
	        }
        }

        public PoolPointer Allocate(long size)
        {
            if (size > PoolSize)
                return null;

            long currentScanOffset = 0;

            lock (PoolPointers)
            {
                foreach (var pointer in PoolPointers.OrderBy(x => x.Offset).ToArray())
                {
                    if (pointer.Offset - currentScanOffset >= size)
                    {
                        var newPointer = new PoolPointer(this, currentScanOffset, size, DefaultFixed);
                        PoolPointers.Add(newPointer);
                        return newPointer;
                    }

                    currentScanOffset = pointer.Offset + pointer.Length;
                }

                if (PoolSize - currentScanOffset >= size)
                {
	                var newPointer = new PoolPointer(this, currentScanOffset, size, DefaultFixed);
	                PoolPointers.Add(newPointer);
	                return newPointer;
                }
            }

            return null;
        }

        public void Release(PoolPointer poolPointer)
        {
            lock (PoolPointers)
            {
                if (PinnedPointers.Contains(poolPointer))
                    throw new InvalidOperationException("Cannot release a pool pointer that is currently pinned");

                PoolPointers.Remove(poolPointer);
            }
        }

        public IMemoryOwner<byte> GetMemory(PoolPointer poolPointer)
        {
	        lock (PoolPointers)
	        {
                PinnedPointers.Add(poolPointer);
                return new AllocatorMemoryOwner(this, poolPointer, UnderlyingMemory.Slice((int)poolPointer.Offset, (int)poolPointer.Length));
	        }
        }

        protected void UnpinPointer(PoolPointer pointer)
        {
	        lock (PoolPointers)
	        {
		        PinnedPointers.Remove(pointer);
            }
        }

        public void Compact()
        {
            lock (PoolPointers)
            {
                long currentScanOffset = 0;

                int currentPinnedPointerIndex = 0;

                var pinnedPointers = PinnedPointers
	                .Concat(PoolPointers.Where(x => x.Fixed))
	                .OrderBy(x => x.Offset)
	                .ToArray();

                var movablePointers = PoolPointers
	                .Except(pinnedPointers)
	                .OrderBy(x => x.Offset);

                foreach (var pointer in movablePointers)
                {
                    if (currentScanOffset == pointer.Offset)
                    {
                        // Block is already in optimal position
                        currentScanOffset += pointer.Length;
                        continue;
                    }

                    while (pinnedPointers[currentPinnedPointerIndex].Offset - currentScanOffset < pointer.Length)
                    {
                        // There is a pinned pointer in the way. Skip over it
                        currentScanOffset = pinnedPointers[currentPinnedPointerIndex].Offset +
                                            pinnedPointers[currentPinnedPointerIndex].Length;
                        currentPinnedPointerIndex++;
                    }

                    var originalPosition = UnderlyingMemory.Slice((int)pointer.Offset, (int)pointer.Length);
                    var newPosition = UnderlyingMemory.Slice((int)currentScanOffset, (int)pointer.Length);

                    originalPosition.CopyTo(newPosition);

                    pointer.Offset = currentScanOffset;

                    currentScanOffset += pointer.Length;
                }
            }
        }

        private class AllocatorMemoryOwner : IMemoryOwner<byte>
        {
            private bool IsDisposed = false;

	        public Memory<byte> Memory { get; protected set; }

            public PoolPointer PoolPointer { get; }

            public Allocator Allocator { get; }

	        public AllocatorMemoryOwner(Allocator allocator, PoolPointer pointer, Memory<byte> memory)
	        {
		        Allocator = allocator;
		        Memory = memory;
		        PoolPointer = pointer;
	        }

	        public void Dispose()
	        {
		        if (!IsDisposed)
		        {
                    Allocator.UnpinPointer(PoolPointer);
			        Memory = null;
                    IsDisposed = true;
                }
	        }
        }
    }


    public unsafe class UnmanagedMemory : MemoryManager<byte>
    {
	    public long Size { get; }

	    protected IntPtr Pointer = IntPtr.Zero;
	    protected bool DisposeHandle = false;

	    public UnmanagedMemory(IntPtr pointer, long size)
	    {
		    Pointer = pointer;
		    Size = size;
	    }

	    public UnmanagedMemory(long size) : this(Marshal.AllocHGlobal(new IntPtr(size)), size)
	    {
		    DisposeHandle = true;
	    }

	    protected override void Dispose(bool disposing)
	    {
		    if (DisposeHandle && Pointer != IntPtr.Zero)
		    {
			    Marshal.FreeHGlobal(Pointer);
			    Pointer = IntPtr.Zero;
		    }
	    }

	    public override Span<byte> GetSpan()
	    {
		    return new Span<byte>(Pointer.ToPointer(), (int)Size);
	    }

	    public override MemoryHandle Pin(int elementIndex = 0)
	    {
		    return new MemoryHandle(Pointer.ToPointer());
	    }

	    public override void Unpin() { }

	    ~UnmanagedMemory()
	    {
		    Dispose(false);
	    }
    }
}