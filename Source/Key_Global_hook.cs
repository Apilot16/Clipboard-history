using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Try_to_hook
{
    internal class Key_Global_hook
    {
        private const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc _proc = HookCallback;
        public static IntPtr _hookID = IntPtr.Zero;
        //public static StreamWriter SW = new StreamWriter(@"D:\textZ.txt");

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// Этот код работает, но мне абсолютно не понятно:
        /// как работает HookCallback; и зачем в нем столько аргументов при вызове
        /// почему используется тип IntPtr, вместо стандартных int и др.;
        /// 
        /// </summary>
        public static void unhook_on_clozed()
        {
            UnhookWindowsHookEx(_hookID);
        }
        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }


        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //тут собственно и происходит выполнение кода хука

            //if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)//это проверка на именно нажатие, смотри keydown,keypress,keyup
            //{
            //    int vkCode = Marshal.ReadInt32(lParam);//тут чтение данных из ячейки памяти по адресу lParam(адресс определяется автоматически при запуске приложений)
            //    if ((Keys)vkCode == Keys.C)
            //    {
            //      ваш код сдесь   
            //      ИЛИ переназначте на любую другую функцию в  
            //    }
            //}
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

    }
}
