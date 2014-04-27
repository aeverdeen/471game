using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace StepDX
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Game game = new Game();
            game.Show();
            do
            {
                game.Advance();
                game.Render();
                Application.DoEvents();
                if (game.Restart == true)
                {
                    game = new Game();
                    game.Show();
                }
            } while (game.Created);
        }
    }
}
