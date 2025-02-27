using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Focus
{
    public class UserMacros
    {
        public List<string> commands = new();
        public List<Key> keys = new();
        public List<string> context = new();
    }

    [System.Serializable]
    public class UserData
    {
        [SerializeField]
        private List<UserMacros> macros = new();

        public List<UserMacros> Macros()
        {
            return macros;
        }

        public void AddCommand(UserMacros macro)
        {
            if (macros.Contains(macro))
            {
                return;
            }

            macros.Add(macro);
        }
    }

    public class FileDataHandler
    {
        private string DirPath = "";

        private string fileName = "";

        public FileDataHandler(string path, string file)
        {
            DirPath = path;
            fileName = file;
        }

        public UserData Load()
        {
            string path = Path.Combine(DirPath, fileName);

            UserData data = null;

            if (!File.Exists(path))
            {
                // Debug.LogError($"File {fileName} does not exist at ${path}");
                return null;
            }

            try
            {
                string loaded = "";

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    {
                        loaded = reader.ReadToEnd();
                    }
                }

                data = JsonConvert.DeserializeObject<UserData>(loaded);
            }
            catch (Exception e)
            {
                Debug.Log($"an error occurred {e}");
            }

            return data;
        }

        public void Save(UserData data)
        {
            string path = Path.Combine(DirPath, fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                var wrapper = data;

                string storingData = JsonConvert.SerializeObject(wrapper);

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(storingData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"an error occurred {e}");
            }
        }
    }
}
