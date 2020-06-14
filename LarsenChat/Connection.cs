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
    public partial class ConnectionForm : Form
    {
        public ConnectionForm()
        {
            InitializeComponent();
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void backgroundWorker1_DoWork_1(object sender, DoWorkEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            App.networker = new Client();
            if (!((Client)App.networker).Connect(textServerIP.Text))
                MessageBox.Show("Connection Retry limit reached",
                    "Host couldn't be reached",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            else
            {
                MainForm.OpenForm(new HomeForm());
            }
        }

        private void HostButton_Click(object sender, EventArgs e)
        {
            App.networker = new Server((uint)maxClients.Value);
            ((Server)App.networker).Run();

            MainForm.OpenForm(new HomeForm());
        }
    }
}
