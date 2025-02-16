using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nexus_Injector.Menu
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Capture = false;
                    Message m = Message.Create(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
                    WndProc(ref m);
                }

            };
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/cmwa124P",
                UseShellExecute = true
            });
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/B4tSWuBQbQ",
                UseShellExecute = true
            });
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/124Px",
                UseShellExecute = true
            });
        }

        private void bunifuButton5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bunifuButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
