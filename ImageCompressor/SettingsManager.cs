using Newtonsoft.Json;
using System;
using System.IO;

namespace ImageCompressor
{
    public class SettingsManager<T> where T : class, new()
    {
        private readonly string _filePath;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, fileName);
        }

        public T LoadSettings()
        {
            if (File.Exists(_filePath))
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(_filePath)) ?? new T();
            }
            else
            {
                SaveSettings(default(T));
                return LoadSettings();
            }
        }

        public void SaveSettings(T settings)
        {
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}
