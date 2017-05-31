#include "stdafx.h"
#include "PPeXIL.h"
#using <PPeX.dll>

PPeXIL::PPeXIL()
{
}

bool PPeXIL::ArchiveDecompress(wchar_t** paramArchive, wchar_t** paramFile, DWORD* readBytes, BYTE** outBuffer, void *(__stdcall*alloc)(size_t))
{
	System::String^ archive = gcnew System::String(*paramArchive);
	System::String^ file = gcnew System::String(*paramFile);
	
	size_t sz = PPeX::Manager::PreAlloc(archive, file);
	
	if (sz > 0) {
		*outBuffer = (BYTE*)alloc(sz);
		*readBytes = sz;

		//PPeX::Manager::Decompress(archive, file, System::IntPtr(*outBuffer));
		PPeX::Manager::Decompress(archive, file, *outBuffer);
		
		delete archive;
		delete file;
		return true;
	}
	
	
	//void* fileBuffer = Shared::IllusionMemAlloc(match->GetFileSize());
	
	delete archive;
	delete file;

	return false;
}


PPeXIL::~PPeXIL()
{
}
