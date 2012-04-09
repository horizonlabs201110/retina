using System;
using System.Runtime.InteropServices;

namespace Com.Imola.Retina.Utility
{
    class Utilities
    {
        public unsafe static void SendKey(ushort vkey)
        {
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = Win32Consts.INPUT_KEYBOARD;

            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = uint.Parse(Win32.GetMessageExtraInfo().ToString());

            //Key down
            structInput.ki.wVk = vkey;
            Win32.SendInput(1, ref structInput, (uint)sizeof(INPUT));

            //Key up
            structInput.ki.dwFlags = Win32Consts.KEYEVENTF_KEYUP;
            Win32.SendInput(1, ref structInput, (uint)sizeof(INPUT));
        }

        public class Win32Consts
        {
            public const int INPUT_MOUSE = 0;
            public const int INPUT_KEYBOARD = 1;
            public const int INPUT_HARDWARE = 2;
            public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
            public const uint KEYEVENTF_KEYUP = 0x0002;
            public const uint KEYEVENTF_UNICODE = 0x0004;
            public const uint KEYEVENTF_SCANCODE = 0x0008;
            public const uint XBUTTON1 = 0x0001;
            public const uint XBUTTON2 = 0x0002;
            public const uint MOUSEEVENTF_MOVE = 0x0001;
            public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
            public const uint MOUSEEVENTF_LEFTUP = 0x0004;
            public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
            public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
            public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
            public const uint MOUSEEVENTF_XDOWN = 0x0080;
            public const uint MOUSEEVENTF_XUP = 0x0100;
            public const uint MOUSEEVENTF_WHEEL = 0x0800;
            public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
            public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public long time;
            public uint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public KEYBDINPUT ki;
        }

        class Win32
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint SendInput(uint nInputs, [In,Out] ref INPUT pInputs, uint cbSize);

            [DllImport("user32.dll")]
            public static extern IntPtr GetMessageExtraInfo();
        }
    }
}