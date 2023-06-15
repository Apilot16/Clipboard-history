using System;
using System.Windows.Forms;

namespace Try_to_hook
{
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }


        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>







    }
}
