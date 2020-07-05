using LarsenNetworking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LarsenChat
{
    public partial class HomeForm : Form
    {
        public static ListBox messageBox;

        public HomeForm()
        {
            InitializeComponent();
            messageBox = chatBox;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            Packet packet = Packet.Empty;
            packet.WriteCommand(new Command(new App.PrintMessage() { message = inputBox.Text }));

            messageBox.Items.Add(inputBox.Text);

            if (App.networker is Server)
            {
                foreach (NetPlayer player in App.networker.Players.Values)
                {
                    player.OutPackets.Enqueue(packet);
                }

            }
            else
            {
                ((Client)App.networker).server.OutPackets.Enqueue(packet);
            }
        }

        private void chatBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
