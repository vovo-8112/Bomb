using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
    public class MMSaveLoadManagerMethodBinaryEncrypted : MMSaveLoadManagerEncrypter, IMMSaveLoadManagerMethod
    {
        public void Save(object objectToSave, FileStream saveFile)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, objectToSave);
            memoryStream.Position = 0;
            Encrypt(memoryStream, saveFile, Key);
            saveFile.Flush();
            memoryStream.Close();
            saveFile.Close();
        }
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
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
            savedObject = formatter.Deserialize(memoryStream);
            memoryStream.Close();
            saveFile.Close();
            return savedObject;
        }
    }
}
