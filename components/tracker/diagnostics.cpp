/****************************************************************************
*                                                                           *
****************************************************************************/

#include <stdio.h>
#include <stdarg.h>
#include <XnOpenNI.h>

#include "diagnostics.h"

namespace ImolaRetinaTracker
{
	void Diagnostics::Trace(const char *format, ...)
	{
		va_list args;

		va_start(args, format );
		printf(format, args );
		va_end(args );
	}
	
	void Diagnostics::Trace(xn::EnumerationErrors errors)
	{
		XnChar strError[1024];
		errors.ToString(strError, 1024);
			
		Diagnostics::Trace("%s\n", strError);
	}
}