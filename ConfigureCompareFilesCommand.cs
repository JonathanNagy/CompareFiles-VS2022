using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CompareFilesVS2019
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConfigureCompareFilesCommand
    {
        private string compareToolPath = @"%PROGRAMFILES(X86)%\Beyond Compare 4\BCompare.exe";
        private const string settingsFilePath = @"%USERPROFILE%\AppData\Local\CompareFilesAddIn\CompareFiles.conf";

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b39b66a5-d715-428d-8172-469dafdd0da3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureCompareFilesCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ConfigureCompareFilesCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConfigureCompareFilesCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ConfigureCompareFilesCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ConfigureCompareFilesCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ShowCompareFilesConfigurationWindow(sender, e);
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowCompareFilesConfigurationWindow(object sender, EventArgs e)
        {
            LoadCompareToolPath();

            using (var dialog = new ConfigurationDialog(compareToolPath))
            {
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    StoreCompareToolPath(dialog.ToolPath);
                }
            }
        }

        private void LoadCompareToolPath()
        {
            FileInfo settingsFile = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (settingsFile.Exists)
            {
                using (FileStream fileStream = settingsFile.OpenRead())
                {
                    TextReader reader = new StreamReader(fileStream);
                    var storedCompareToolPath = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(storedCompareToolPath))
                    {
                        compareToolPath = storedCompareToolPath;
                    }
                }
            }
        }

        private void StoreCompareToolPath(string newToolPath)
        {
            FileInfo file = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (!file.Directory.Exists)
                file.Directory.Create();

            FileStream fileStream = null;
            try
            {
                fileStream = file.Create();
                using (TextWriter writer = new StreamWriter(fileStream))
                {
                    fileStream = null;
                    writer.WriteLine(newToolPath);
                }
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();
            }
        }


    }
}
