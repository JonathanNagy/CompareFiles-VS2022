using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareFilesVS2019
{
    public partial class ConfigurationDialog : Form
    {
        private string toolPath;

        public ConfigurationDialog()
        {
            InitializeComponent();
        }

        public ConfigurationDialog(string currentPath)
        {
            InitializeComponent();

            toolPath = currentPath;
            txtFilePath.Text = currentPath;
        }

        public string ToolPath
        {
            get
            {
                return toolPath;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Executables (.exe)|*.exe|All Files (*.*)|*.*";
                fileDialog.FilterIndex = 1;
                fileDialog.CheckFileExists = true;

                DialogResult dialogResults = fileDialog.ShowDialog();
                if (dialogResults == System.Windows.Forms.DialogResult.OK)
                {
                    txtFilePath.Text = fileDialog.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            toolPath = txtFilePath.Text;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
