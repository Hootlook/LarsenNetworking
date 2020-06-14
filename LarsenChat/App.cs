using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LarsenChat
{
    static class App
    {
        public static Networker networker;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Command.Register(new IMessage[] { 
                new Client.ConnectionMessage(),
                new PrintMessage() 
            });

            Application.Run(new MainForm());
        }

        public class PrintMessage : IMessage
        {
            public string message;
            public void Execute()
            {
                HomeForm.messageBox.Items.Add(message);
            }
        }
    }
}
