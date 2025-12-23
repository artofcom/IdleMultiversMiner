using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            } 
            return true;
        }

        //
        public static string ReadTextFile(string filePath)
        {
            try
            {
                if(!File.Exists(filePath))
                {
                    var stream = File.Create(filePath);
                    if(stream != null) 
                        return string.Empty;

                    stream.Close();
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            } 
            return string.Empty;
        }
    }
}
