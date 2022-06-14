using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    public interface IMMSaveLoadManagerMethod
    {
        void Save(object objectToSave, FileStream saveFile);
        object Load(System.Type objectType, FileStream saveFile);
    }
    public enum MMSaveLoadManagerMethods { Json, JsonEncrypted, Binary, BinaryEncrypted };
    public abstract class MMSaveLoadManagerEncrypter
    {
        public string Key { get; set; } = "yourDefaultKey";

        protected string _saltText = "SaltTextGoesHere";
        protected virtual void Encrypt(Stream inputStream, Stream outputStream, string sKey)
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(inputStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read);
            cryptostream.CopyTo(outputStream);
        }
        protected virtual void Decrypt(Stream inputStream, Stream outputStream, string sKey)
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(inputStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read);
            cryptostream.CopyTo(outputStream);
        }
    }
}
