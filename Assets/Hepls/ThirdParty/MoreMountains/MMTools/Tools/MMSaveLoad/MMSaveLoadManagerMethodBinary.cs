using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
    public class MMSaveLoadManagerMethodBinary : IMMSaveLoadManagerMethod
    {
        public void Save(object objectToSave, FileStream saveFile)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(saveFile, objectToSave);
            saveFile.Close();
        }
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject;
            BinaryFormatter formatter = new BinaryFormatter();
            savedObject = formatter.Deserialize(saveFile);
            saveFile.Close();
            return savedObject;
        }
    }
}
