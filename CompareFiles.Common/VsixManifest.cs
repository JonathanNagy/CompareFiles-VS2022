using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CompareFiles.Common
{
    public class VsixManifest
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string DisplayName { get; set; }

        public VsixManifest()
        {
        }

        public VsixManifest(string manifestPath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestPath);

            if (doc.DocumentElement == null || doc.DocumentElement.Name != "PackageManifest") return;

            var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == "Metadata");
            if(metaData == null) return;
            var identity = metaData.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == "Identity");
            var displayName = metaData.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == "DisplayName");

            Id = identity?.GetAttribute("Id");
            Version = identity?.GetAttribute("Version");
            DisplayName = displayName?.InnerText;
        }

        public static VsixManifest GetManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyUri = new UriBuilder(assembly.CodeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var vsixManifestPath = Path.Combine(assemblyDirectory, "extension.vsixmanifest");

            return new VsixManifest(vsixManifestPath);
        }
    }
}
