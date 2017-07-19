#include "stdafx.h"
#include "PPeXIL.h"
#ifdef _DEBUG
#using <..\PPeX.Manager\bin\Debug\PPeX.Manager.dll>
#else
//#using <..\PPeX.Manager\bin\Release\PPeX.Manager.dll>
#using <..\PPeX.Manager\bin\Debug\PPeX.Manager.dll>
#endif


PPeXIL::PPeXIL()
{
}

void *(__stdcall* PPeXIL::IllusionAlloc)(size_t);

bool PPeXIL::ArchiveDecompress(wchar_t** paramArchive, wchar_t** paramFile, DWORD* readBytes, BYTE** outBuffer, void *(__stdcall* alloc)(size_t))
{
	System::String^ archive = gcnew System::String(*paramArchive);
	System::String^ file = gcnew System::String(*paramFile);
	
	IllusionAlloc = alloc;

	//Lambdas don't work with delegates so this is the best we can manage
	//Can't put the method in the header since it'll complain about AAUnlimited needing CLI
	//so it has to be a local function
	class Dummy {
		public :
			static System::IntPtr ManagedAlloc(size_t size)
			{
				return System::IntPtr(IllusionAlloc(size));
			}
	};

	PPeX::Manager::Manager::AllocateDelegate^ delgatedAlloc = gcnew PPeX::Manager::Manager::AllocateDelegate(Dummy::ManagedAlloc);

	unsigned int uReadBytes = 0;

	bool result =  PPeX::Manager::Manager::Decompress(archive, file, delgatedAlloc, outBuffer, &uReadBytes);

	*readBytes = uReadBytes;
	
	delete archive;
	delete file;

	return result;
}


PPeXIL::~PPeXIL()
{
}
