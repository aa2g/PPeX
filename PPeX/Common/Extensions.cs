using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPeX.Common
{
    public static class Extensions
    {
	    public static async Task ForEachAsync<T>(this IEnumerable<T> items, int maxParallel, Func<T, Task> action)
	    {
		    var semaphore = new SemaphoreSlim(maxParallel);

		    await Task.WhenAll(items.Select(async item =>
		    {
			    await semaphore.WaitAsync();

			    try
			    {
				    await action(item);
			    }
			    finally
			    {
				    semaphore.Release();
			    }
		    }));
	    }

		public static void WriteString(this BinaryWriter writer, string str)
        {
            writer.Write(Encoding.ASCII.GetBytes(str));
        }

        public static string ReadString(this BinaryReader reader, int length)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(length));
        }
    }
}
