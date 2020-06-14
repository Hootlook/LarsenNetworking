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
    public partial class MainForm : Form
    {
        private static Form currentForm;
        private static Panel mainPanel;

        public MainForm()
        {
            InitializeComponent();
            mainPanel = MainPanel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            OpenForm(new ConnectionForm());
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        public static void OpenForm(Form form)
        {
            if (currentForm != null)
                currentForm.Close();

            currentForm = form;

            currentForm.TopLevel = false;
            currentForm.FormBorderStyle = FormBorderStyle.None;
            currentForm.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(form);
            mainPanel.Tag = form;
            currentForm.BringToFront();
            currentForm.Show();
        }
    }
}
