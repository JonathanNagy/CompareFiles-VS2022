using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CompareFilesVS2019
{
    internal abstract class CompareFilesCommandBase 
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected readonly AsyncPackage package;
        
        private string compareToolPath = @"%PROGRAMFILES(X86)%\Beyond Compare 4\BCompare.exe";
        private const string settingsFilePath = @"%USERPROFILE%\AppData\Local\CompareFilesAddIn\CompareFiles.conf";

        protected CompareFilesCommandBase(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected void Execute(object sender, EventArgs e)
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Execute() of: {0}", this.ToString()));

            ThreadHelper.ThrowIfNotOnUIThread();

            LoadCompareToolPath();

            var _applicationObject = (DTE)package.GetServiceAsync(typeof(DTE)).Result;

            var items = _applicationObject.SelectedItems;

            var compareToolPathExpanded = Environment.ExpandEnvironmentVariables(compareToolPath);

            string arguments;
            switch (items.Count)
            {
                case 1:
                    SelectedItem item = items.Item(1);
                    for (short i = 1; i <= item.ProjectItem.FileCount; i++)
                    {
                        arguments = "\"" + item.ProjectItem.FileNames[i] + "\"";
                        System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                    }

                    var subProjectItems = item.ProjectItem.ProjectItems;
                    if (subProjectItems != null)
                    {
                        for (short i = 1; i <= subProjectItems.Count; i++)
                        {
                            ProjectItem subItem = subProjectItems.Item(i);
                            for (short j = 1; j <= subItem.FileCount; j++)
                            {
                                arguments = "\"" + subItem.FileNames[j] + "\"";
                                System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                            }
                        }
                    }
                    break;
                case 2:
                    SelectedItem item1 = items.Item(1);
                    SelectedItem item2 = items.Item(2);
                    for (short i = 1; i <= Math.Min(item1.ProjectItem.FileCount, item2.ProjectItem.FileCount); i++)
                    {
                        arguments = "\"" + item1.ProjectItem.FileNames[i] + "\" \"" + item2.ProjectItem.FileNames[i] + "\"";
                        System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                    }

                    var subProjectItems1 = item1.ProjectItem.ProjectItems;
                    var subProjectItems2 = item2.ProjectItem.ProjectItems;
                    if (subProjectItems1 != null && subProjectItems2 != null)
                    {
                        for (short i = 1; i <= Math.Min(subProjectItems1.Count, subProjectItems2.Count); i++)
                        {
                            ProjectItem subItem1 = subProjectItems1.Item(i);
                            ProjectItem subItem2 = subProjectItems2.Item(i);
                            for (short j = 1; j <= Math.Min(subItem1.FileCount, subItem1.FileCount); j++)
                            {
                                arguments = "\"" + subItem1.FileNames[i] + "\" \"" + subItem2.FileNames[i] + "\"";
                                System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                            }
                        }
                    }
                    break;
                default:
                    MessageBox.Show("Select 1 or 2 files.", "Compare Files");
                    return;
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
