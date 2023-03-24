using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Utils
{
    internal class SettingsLoader<T> where T : new()
    {
        protected string FileName = "";

        public SettingsLoader(string fileName)
        {
            FileName = fileName;
        }
        public T Load()
        {
            //load from file or give defaults and fail
            var currentDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            var filePath = Path.Combine(currentDirectory, FileName);
            T instance = new T();
            try
            {
                instance = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                File.WriteAllText(filePath, JsonConvert.SerializeObject(instance));
            }
            catch
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(instance));
            }
            return instance;
        }
    }
}

