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
	PPeX::Manager::Initializer::BindDependencies();
}

bool PPeXIL::ArchiveDecompress(wchar_t** paramArchive, wchar_t** paramFile, DWORD* readBytes, BYTE** outBuffer, void *(__stdcall*alloc)(size_t))
{
	System::String^ archive = gcnew System::String(*paramArchive);
	System::String^ file = gcnew System::String(*paramFile);
	
	size_t sz = PPeX::Manager::Manager::PreAlloc(archive, file);
	
	if (sz > 0) {
		*outBuffer = (BYTE*)alloc(sz);
		*readBytes = sz;

		PPeX::Manager::Manager::Decompress(archive, file, *outBuffer);
		
		delete archive;
		delete file;
		return true;
	}
	
	delete archive;
	delete file;

	return false;
}


PPeXIL::~PPeXIL()
{
}
