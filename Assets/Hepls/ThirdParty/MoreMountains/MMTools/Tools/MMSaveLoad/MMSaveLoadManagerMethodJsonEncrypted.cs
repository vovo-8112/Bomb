using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    public class MMSaveLoadManagerMethodJsonEncrypted : MMSaveLoadManagerEncrypter, IMMSaveLoadManagerMethod
    {
        public void Save(object objectToSave, FileStream saveFile)
        {
            string json = JsonUtility.ToJson(objectToSave);
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                memoryStream.Position = 0;
                Encrypt(memoryStream, saveFile, Key);
            }
            saveFile.Close();
        }
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject = null;
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader streamReader = new StreamReader(memoryStream))
            {
                try
                {
                    Decrypt(saveFile, memoryStream, Key);
                }
                catch (CryptographicException ce)
                {
                    Debug.LogError("[MMSaveLoadManager] Encryption key error: " + ce.Message);
                    return null;
                }
                memoryStream.Position = 0;
                savedObject = JsonUtility.FromJson(streamReader.ReadToEnd(), objectType);
            }
            saveFile.Close();
            return savedObject;
        }

    }
}
