using System;
using System.IO;
using System.Reflection;

namespace Glade2d.Services
{
    public class FileService
    {
        private static FileService instance;
        public static FileService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileService();
                }
                return instance;
            }
        }

        private FileService() { }

        public string AssemblyDirectory => Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        public void ListFilesRecursively(string path)
        {
            try
            {
                LogService.Log.Info($"Opening {path}");
                var files = Directory.GetFiles(path);
                var dirs = Directory.GetDirectories(path);

                foreach (var file in files)
                {
                    LogService.Log.Info($"  - {file}");
                }
                foreach (var dir in dirs)
                {
                    ListFilesRecursively(dir);
                }
            }
            catch (Exception ex)
            {
                LogService.Log.Error($"Failed to open {path} - {ex.Message}");
                return;
            }
        }

        public byte[] LoadResource(string name)
        {
            var resourcePath = $"Glade2d.Resources.{name}";
            Console.WriteLine($"Attempting to load resource: {resourcePath}");
            var assembly = Assembly.GetExecutingAssembly();
            byte[] resBytes;
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    resBytes = ms.ToArray();
                }
            }
            return resBytes;
        }
    }
}
