
#include <stdio.h>
#include <stdarg.h>
#include "diagnostics.h"

void Trace(const char *format, ...)
{
	va_list args;

	va_start(args, format );
	printf(format, args );
	va_end(args );
}