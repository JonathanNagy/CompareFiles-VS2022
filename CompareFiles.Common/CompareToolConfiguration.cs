using System;
using System.IO;
using Newtonsoft.Json;

namespace CompareFiles.Common
{
    public class CompareToolConfiguration
    {
        private const string defaultExecutablePath = @"%PROGRAMFILES(X86)%\Beyond Compare 4\BCompare.exe";
        private const string defaultExtraArguments = "";

        internal static string ExecutablePath = defaultExecutablePath;
        internal static string ExtraArugments = defaultExtraArguments;

        private const string settingsFilePath = @"%USERPROFILE%\AppData\Local\CompareFilesAddIn\CompareFilesConfig.json";

        public string CompareToolExecutablePath { get; set; }
        public string CompareToolExtraArguments { get; set; }
        
        public static void LoadCompareToolConfiguration()
        {
            var settingsFile = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (settingsFile.Exists)
            {
                try
                {
                    LoadConfigurationFromFile(settingsFile);
                } 
                catch(Exception)
                {
                    StoreDefaultConfiguration();
                    LoadConfigurationFromFile(settingsFile);
                }
            }
        }
        
        private static void LoadConfigurationFromFile(FileInfo settingsFile)
        {
            using (FileStream fileStream = settingsFile.OpenRead())
            {
                using(TextReader reader = new StreamReader(fileStream))
                {
                    var serializer = new JsonSerializer();
                    var configuration = (CompareToolConfiguration)serializer.Deserialize(reader, typeof(CompareToolConfiguration));
                    ExecutablePath = configuration.CompareToolExecutablePath;
                    ExtraArugments = configuration.CompareToolExtraArguments;
                }
            }
        }
        
        public static void StoreCompareToolConfiguration(CompareToolConfiguration configuration)
        {
            FileInfo file = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (!file.Directory.Exists)
                file.Directory.Create();

            using(var fileStream = file.Create()) 
            {
                using (TextWriter writer = new StreamWriter(fileStream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, configuration);
                }
            }
        }

        private static void StoreDefaultConfiguration()
        {
            var configuration = new CompareToolConfiguration()
            {
                CompareToolExecutablePath = defaultExecutablePath,
                CompareToolExtraArguments = defaultExtraArguments
            };
            StoreCompareToolConfiguration(configuration);
        }

    }
}
