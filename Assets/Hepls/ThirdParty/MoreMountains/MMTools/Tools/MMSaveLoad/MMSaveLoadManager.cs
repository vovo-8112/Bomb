using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    public static class MMSaveLoadManager
    {
        public static IMMSaveLoadManagerMethod saveLoadMethod = new MMSaveLoadManagerMethodBinary();
        private const string _baseFolderName = "/MMData/";
		private const string _defaultFolderName = "MMSaveLoadManager";
		static string DetermineSavePath(string folderName = _defaultFolderName)
		{
			string savePath;
			if (Application.platform == RuntimePlatform.IPhonePlayer) 
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			} 
			else 
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
			#if UNITY_EDITOR
			    savePath = Application.dataPath + _baseFolderName;
			#endif

			savePath = savePath + folderName + "/";
			return savePath;
		}
		static string DetermineSaveFileName(string fileName)
		{
			return fileName;
		}
		public static void Save(object saveObject, string fileName, string foldername = _defaultFolderName)
		{
			string savePath = DetermineSavePath(foldername);
			string saveFileName = DetermineSaveFileName(fileName);
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}

            FileStream saveFile = File.Create(savePath + saveFileName);

            saveLoadMethod.Save(saveObject, saveFile);
            saveFile.Close();
        }
		public static object Load(System.Type objectType, string fileName, string foldername = _defaultFolderName)
		{
			string savePath = DetermineSavePath(foldername);
			string saveFileName = savePath + DetermineSaveFileName(fileName);

			object returnObject;
			if (!Directory.Exists(savePath) || !File.Exists(saveFileName))
			{
				return null;
			}

            FileStream saveFile = File.Open(saveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            returnObject = saveLoadMethod.Load(objectType, saveFile);
            saveFile.Close();

            return returnObject;
		}
		public static void DeleteSave(string fileName, string folderName = _defaultFolderName)
		{
			string savePath = DetermineSavePath(folderName);
			string saveFileName = DetermineSaveFileName(fileName);
            if (File.Exists(savePath + saveFileName))
            {
                File.Delete(savePath + saveFileName);
            }			
		}
		public static void DeleteSaveFolder(string folderName = _defaultFolderName)
		{
            string savePath = DetermineSavePath(folderName);
            if (Directory.Exists(savePath))
            {
                DeleteDirectory(savePath);
            }
        }
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}