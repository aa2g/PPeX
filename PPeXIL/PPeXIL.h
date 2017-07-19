#pragma once
#include "stdafx.h"

class PPeXIL
{
public:
	PPeXIL();
	static bool ArchiveDecompress(wchar_t** paramArchive, wchar_t** paramFile, DWORD* readBytes, BYTE** outBuffer, void *(__stdcall*alloc)(size_t));
	~PPeXIL();
private:
	static void *(__stdcall* IllusionAlloc)(size_t);
};

