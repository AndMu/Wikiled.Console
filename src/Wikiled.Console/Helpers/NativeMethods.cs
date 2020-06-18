using System;
using System.Runtime.InteropServices;

namespace Wikiled.Console.Helpers
{
    public static class NativeMethods
    {
        public const uint ES_CONTINUOUS = 0x80000000;

        public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringConsoleToFront()
        {
            SetForegroundWindow(NativeMethods.GetConsoleWindow());
        }
    }
}
