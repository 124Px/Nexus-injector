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


    public partial class ProcessSelectorForm : Form
    {
        public string SelectedProcess { get; private set; } = "";
        public int SelectedPID { get; private set; } = 0;

        public ProcessSelectorForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            LoadProcessList();
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

        private void LoadProcessList()
        {
            ProcessList.Items.Clear();
            var processes = Process.GetProcesses()
                .OrderBy(p => p.ProcessName)
                .ToList();

            foreach (var process in processes)
            {
                ProcessList.Items.Add($"{process.ProcessName} ({process.Id})");
            }
        }

        private void btnWindowList_Click(object sender, EventArgs e)
        {

            ProcessList.Items.Clear();
            var processes = Process.GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero) // Filtre les fenêtres visibles
                .OrderBy(p => p.ProcessName)
                .ToList();

            foreach (var process in processes)
            {
                ProcessList.Items.Add($"{process.ProcessName} ({process.Id})");
            }

        }

        private void btnProcessList_Click(object sender, EventArgs e)
        {
            LoadProcessList();
        }

        private void btnSelectProcess_Click(object sender, EventArgs e)
        {

            if (ProcessList.SelectedItem != null)
            {
                string selectedItem = ProcessList.SelectedItem.ToString();
                string processName = selectedItem.Split('(')[0].Trim();
                int processId = int.Parse(selectedItem.Split('(')[1].Replace(")", "").Trim());

                SelectedProcess = processName;
                SelectedPID = processId;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a process.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
