using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace Clipoff
{
    class HotKeyManager
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        static int hHook = 0;
        private const int WM_KEYUP = 0x101;
        private const int WM_KEYDOWN = 0x100;
        public const int WH_KEYBOARD_LL = 13;
        static HookProc KeyBoardHookProcedure;
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public static bool StartHook()
        {
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                            KeyBoardHookProcedure,
                            //IntPtr.Zero, 
                            GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName),
                            0);
            }
            return (hHook != 0);
        }

        public static bool ReleaseHook()
        {
            bool retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
            return retKeyboard;
        }
        public static int KeyBoardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyBoardHookStruct kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
                
                if (wParam == (IntPtr)WM_KEYDOWN
                   && (int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Alt)
                {
                    char hotKey = Config.HotKey;//either digit or upppercase letter
                    //char hotKeys = 'A';
                    if (((char.IsLetter(hotKey)) && kbh.vkCode == (int)Keys.A + (int)(hotKey - 'A'))
                        || kbh.vkCode == (int)Keys.D0 + (int)(hotKey - '0'))
                    {
                        App.Instance.Clip();
                    }
                }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}
