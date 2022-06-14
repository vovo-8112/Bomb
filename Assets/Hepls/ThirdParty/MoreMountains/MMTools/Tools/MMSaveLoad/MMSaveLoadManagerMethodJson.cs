using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    public class MMSaveLoadManagerMethodJson : IMMSaveLoadManagerMethod
    {
        public void Save(object objectToSave, FileStream saveFile)
        {
            string json = JsonUtility.ToJson(objectToSave);
            StreamWriter streamWriter = new StreamWriter(saveFile);
            streamWriter.Write(json);
            streamWriter.Close();
            saveFile.Close();
            Debug.Log(objectToSave.GetType());
        }
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject;
            StreamReader streamReader = new StreamReader(saveFile, Encoding.UTF8);
            string json = streamReader.ReadToEnd();
            Debug.Log(json);
            savedObject = JsonUtility.FromJson(json, objectType);
            streamReader.Close();
            saveFile.Close();
            return savedObject;
        }
    }
}
