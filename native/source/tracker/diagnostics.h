/****************************************************************************
*                                                                           *
****************************************************************************/

#ifndef __DIAGNOSTICS_H__
#define __DIAGNOSTICS_H__

#include <XnCppWrapper.h>

namespace ImolaRetinaTracker
{
	class Diagnostics
	{
	public:
		static void Trace(const char *format, ...);
		static void Trace(xn::EnumerationErrors errors);
	};
}

#endif