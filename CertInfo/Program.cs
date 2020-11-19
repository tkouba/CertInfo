using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CertInfo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain(args.Length > 0 ? args[0] : null, args.Length > 1 ? args[1] : null));
        }
    }
}
