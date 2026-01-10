using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.Utils
{
    public class TextFileIO
    {
        //
        public static bool WriteTextFile(string filePath, string content)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(content);
                    writer.Flush();
                }
                return true;
            }
            catch (IOException ex)
            {
                Debug.LogWarning($"[FileIO] Writing failed.: {ex.Message}");
                return false;
            }
        }

        //
        public static string ReadTextFile(string filePath)
        {
            try
            {
                string defaultContent = string.Empty;
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    if (fs.Length == 0 && !string.IsNullOrEmpty(defaultContent))
                    {
                        using (StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8, 1024, true)) // true: 스트림 유지
                        {
                            writer.Write(defaultContent);
                            writer.Flush();
                        }
                        return defaultContent;
                    }

                    fs.Position = 0; 
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.LogWarning($"[FileIO] Reading has been failed! : {ex.Message}");
                return string.Empty;
            }
        }
    }
}
