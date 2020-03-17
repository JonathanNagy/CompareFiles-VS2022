using System;
using System.Windows.Forms;

namespace CompareFiles.Common
{
    public partial class ConfigurationDialog : Form
    {
        private CompareToolConfiguration configuration;
        
        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        public static void ShowCompareFilesConfigurationWindow(object sender, EventArgs e)
        {
            CompareToolConfiguration.LoadCompareToolConfiguration();

            using (var dialog = new ConfigurationDialog())
            {
                var manifest = VsixManifest.GetManifest();
                dialog.SetProductInfo(manifest.DisplayName, manifest.Version);
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    CompareToolConfiguration.StoreCompareToolConfiguration(dialog.Configuration);
                }
            }
        }

        public ConfigurationDialog()
        {
            InitializeComponent();

            txtFilePath.Text = CompareToolConfiguration.ExecutablePath;
            txtExtraArguments.Text = CompareToolConfiguration.ExtraArugments;
        }
        
        public CompareToolConfiguration Configuration
        {
            get
            {
                return configuration;
            }
        }

        private void SetProductInfo(string productName, string versionNumber)
        {
            lblProductInfo.Text = String.Format("{0} v{1}", productName, versionNumber);
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
            configuration = new CompareToolConfiguration()
            {
                CompareToolExecutablePath = txtFilePath.Text,
                CompareToolExtraArguments = txtExtraArguments.Text
            };
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
