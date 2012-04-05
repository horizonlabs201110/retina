
#include <windows.h>
#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>
#include <stdarg.h>
#include "utilities.h"
#include "diagnostics.h"

void SendKey(WORD key)
{
	INPUT input[2]; 

    input[0].type = INPUT_KEYBOARD; 
    input[0].ki.wVk = 0; 
    input[0].ki.wScan = key;
    input[0].ki.dwFlags = KEYEVENTF_UNICODE ; 
    input[0].ki.time = 0; 
    input[0].ki.dwExtraInfo = 0; 

    input[1].type = INPUT_KEYBOARD; 
    input[1].ki.wVk = 0; 
    input[1].ki.wScan = key;
    input[1].ki.dwFlags = KEYEVENTF_KEYUP  | KEYEVENTF_UNICODE ; 
    input[1].ki.time = 0; 
    input[1].ki.dwExtraInfo = 0; 

    if (0 < SendInput(2, input, sizeof(INPUT)))
    { 
		Trace("Key %s sent \n", key);
    }
}

BOOL AnsiToUnicode16(const CHAR *in_Src, WCHAR *out_Dst, INT in_MaxLen)
{
    /* locals */
    INT lv_Len;

  // do NOT decrease maxlen for the eos
  if (in_MaxLen <= 0)
    return FALSE;

  // let windows find out the meaning of ansi
  // - the SrcLen=-1 triggers MBTWC to add a eos to Dst and fails if MaxLen is too small.
  // - if SrcLen is specified then no eos is added
  // - if (SrcLen+1) is specified then the eos IS added
  lv_Len = MultiByteToWideChar(CP_ACP, 0, in_Src, -1, out_Dst, in_MaxLen);

  // validate
  if (lv_Len < 0)
    lv_Len = 0;

  // ensure eos, watch out for a full buffersize
  // - if the buffer is full without an eos then clear the output like MBTWC does
  //   in case of too small outputbuffer
  // - unfortunately there is no way to let MBTWC return shortened strings,
  //   if the outputbuffer is too small then it fails completely
  if (lv_Len < in_MaxLen)
    out_Dst[lv_Len] = 0;
  else if (out_Dst[in_MaxLen-1])
    out_Dst[0] = 0;

  // done
  return TRUE;
}


BOOL AnsiToUnicode16L(const CHAR *in_Src, INT in_SrcLen, WCHAR *out_Dst, INT in_MaxLen)
{
    /* locals */
    INT lv_Len;


  // do NOT decrease maxlen for the eos
  if (in_MaxLen <= 0)
    return FALSE;

  // let windows find out the meaning of ansi
  // - the SrcLen=-1 triggers MBTWC to add a eos to Dst and fails if MaxLen is too small.
  // - if SrcLen is specified then no eos is added
  // - if (SrcLen+1) is specified then the eos IS added
  lv_Len = MultiByteToWideChar(CP_ACP, 0, in_Src, in_SrcLen, out_Dst, in_MaxLen);

  // validate
  if (lv_Len < 0)
    lv_Len = 0;

  // ensure eos, watch out for a full buffersize
  // - if the buffer is full without an eos then clear the output like MBTWC does
  //   in case of too small outputbuffer
  // - unfortunately there is no way to let MBTWC return shortened strings,
  //   if the outputbuffer is too small then it fails completely
  if (lv_Len < in_MaxLen)
    out_Dst[lv_Len] = 0;
  else if (out_Dst[in_MaxLen-1])
    out_Dst[0] = 0;

  // done
  return TRUE;
}