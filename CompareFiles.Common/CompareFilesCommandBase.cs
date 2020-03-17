using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CompareFiles.Common
{
    public abstract class CompareFilesCommandBase
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected readonly AsyncPackage package;

        private readonly DTE applicationObject;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="appObject">DTE event object</param>
        protected CompareFilesCommandBase(AsyncPackage package, DTE appObject)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.applicationObject = appObject ?? throw new ArgumentNullException(nameof(appObject));
        }
        
        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
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

            CompareToolConfiguration.LoadCompareToolConfiguration();

            var items = applicationObject.SelectedItems;

            var compareToolPathExpanded = Environment.ExpandEnvironmentVariables(CompareToolConfiguration.ExecutablePath);

            string arguments;
            switch (items.Count)
            {
                case 1:
                    SelectedItem item = items.Item(1);
                    for (short i = 1; i <= item.ProjectItem.FileCount; i++)
                    {
                        arguments = (CompareToolConfiguration.ExtraArugments + " \"" + item.ProjectItem.FileNames[i] + "\"").Trim();
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
                                arguments = (CompareToolConfiguration.ExtraArugments + " \"" + subItem.FileNames[j] + "\"").Trim();
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
                        arguments = (CompareToolConfiguration.ExtraArugments + " \"" + item1.ProjectItem.FileNames[i] + "\" \"" + item2.ProjectItem.FileNames[i] + "\"").Trim();
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
                                arguments = (CompareToolConfiguration.ExtraArugments + " \"" + subItem1.FileNames[i] + "\" \"" + subItem2.FileNames[i] + "\"").Trim();
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

        
    }
}
