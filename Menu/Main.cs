using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DLLInjector.Injectors;
using Nexus_Injector.Menu;

namespace Nexus_Injector
{
    public partial class Main : Form
    {
        private Dictionary<Button, string> injectionMethods;

        public Main()
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

                pnlDropZone.AllowDrop = true;
                pnlDropZone.DragEnter += PnlDropZone_DragEnter;
                pnlDropZone.DragDrop += PnlDropZone_DragDrop;
            };

        }

        bool isExpanded = false;

        private void PnlDropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && Path.GetExtension(files[0]).ToLower() == ".dll")
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void PnlDropZone_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                txtDllPath.Text = files[0];
            }
        }

        private void btnShowMenu_Click(object sender, EventArgs e)
        {
            if (isExpanded)
            {
                bunifuTransition1.HideSync(pnlInjectMethods);
                pnlInjectMethods.Height = 0;
                isExpanded = false;
                btnShowMenu.Text = "▲ Injection Methods";
            }
            else
            {
                pnlInjectMethods.Height = 382;
                bunifuTransition1.ShowSync(pnlInjectMethods);
                isExpanded = true;
                btnShowMenu.Text = "▼ Injection Methods";
            }
        }


        private void btnBrowseDll_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL Files (*.dll)|*.dll",
                Title = "NEXUS - Select your DLL",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtDllPath.Text = openFileDialog.FileName;
            }
        }

        private void btnSelectProcess_Click(object sender, EventArgs e)
        {
            using (ProcessSelectorForm processSelector = new ProcessSelectorForm())
            {
                if (processSelector.ShowDialog() == DialogResult.OK)
                {
                    int pid = processSelector.SelectedPID;
                    string processName = processSelector.SelectedProcess;

                    lblProcessName.Text = $" {processName}";
                    lblProcessID.Text = $" {pid}";

                    try
                    {
                        Icon processIcon = Icon.ExtractAssociatedIcon(Process.GetProcessById(pid).MainModule.FileName);
                        pbProcessIcon.Image = processIcon.ToBitmap();
                    }
                    catch
                    {
                        pbProcessIcon.Image = SystemIcons.Application.ToBitmap();
                    }
                }
            }
        }

        private void btnClearProcess_Click(object sender, EventArgs e)
        {
            lblProcessName.Text = "none";
            lblProcessID.Text = "none";
            pbProcessIcon.Image = null;
        }

        private void btnClearDll_Click(object sender, EventArgs e)
        {
            txtDllPath.Text = "";
        }

        private void About_Click(object sender, EventArgs e)
        {
            About aboutForm = new About();
            aboutForm.ShowDialog();
        }

        private void bunifuButton4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void bunifuButton5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void InjectionMethod_Click(object sender, EventArgs e)
        {

        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            // Vérifie si une méthode d'injection est sélectionnée
            if (string.IsNullOrEmpty(lblSelectedMethod.Text))
            {
                MessageBox.Show("Veuillez sélectionner une méthode d'injection.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifie si une DLL est choisie et existe
            string dllPath = txtDllPath.Text;
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                MessageBox.Show("Veuillez sélectionner un fichier DLL valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifie si un processus a été sélectionné
            if (!int.TryParse(lblProcessID.Text.Trim(), out int processID) || processID <= 0)
            {
                MessageBox.Show("Veuillez sélectionner un processus valide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = false; // Variable pour stocker le résultat de l'injection

            // Sélectionne la bonne méthode d'injection
            switch (lblSelectedMethod.Text)
            {
                case "Load Library Injection":
                    success = LoadLibraryInjector.Inject(processID, dllPath);
                    break;

                case "Write Process memory Injection":
                    success = WriteProcessMemoryInjector.Inject(processID, dllPath);
                    break;

                case "Set Windows Hook Ex Injection":
                    success = SetWindowsHookExInjector.Inject(processID, dllPath);
                    break;

                case "Mannual map Injection":
                    success = ManualMapInjector.Inject(processID, dllPath);
                    break;

                case "Process hollowing Injection":
                    success = ProcessHollowingInjector.Inject(processID, dllPath);
                    break;

                default:
                    MessageBox.Show("Méthode d'injection non reconnue.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            // Affichage du résultat
            if (success)
            {
                MessageBox.Show($"Injection réussie avec la méthode : {lblSelectedMethod.Text} !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"L'injection a échoué avec la méthode : {lblSelectedMethod.Text}.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadLibrary_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Load Library Injection";
        }

        private void lblSelectedMethod_Click(object sender, EventArgs e)
        {

        }

        private void btnWriteProcessMemory_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Write Process memory Injection";
        }

        private void btnSetWindowsHookEx_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Set Windows Hook Ex Injection";
        }

        private void btnManualMapping_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Mannual map Injection";
        }

        private void btnProcessHollowing_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Process hollowing Injection";
        }

        private void btnProcessDoppelganging_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Process Doppelganging Injection";
        }

        private void btnDLLReflective_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "DLL Reflective Injection";
        }

        private void btnSideLoading_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Slide Loading Injection";
        }

        private void btnAPCInjection_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "APC Injection";
        }

        private void btnThreadHijacking_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Thread  Hijacking Injection";
        }

        private void btnHellsGate_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Hells Gate Injection";
        }

        private void btnEarlyBird_Click(object sender, EventArgs e)
        {
            lblSelectedMethod.Text = "Early Bird Injection";
        }
    }
}
