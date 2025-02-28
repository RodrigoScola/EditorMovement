using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Focus.Persistance
{
    public class FileDataHandler<T>
        where T : class
    {
        private string DirPath = "";

        private string fileName = "";

        public FileDataHandler(string path, string file)
        {
            DirPath = path;
            fileName = file;
        }

        public T Load()
        {
            string path = Path.Combine(DirPath, fileName);

            T data = default(T);

            if (!File.Exists(path))
            {
                // Debug.LogError($"File {fileName} does not exist at ${path}");
                return data;
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

                data = JsonConvert.DeserializeObject<T>(loaded);
            }
            catch (Exception e)
            {
                Debug.Log($"an error occurred {e}");
            }

            return data;
        }

        public void Save(T data)
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
