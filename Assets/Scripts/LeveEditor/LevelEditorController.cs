using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModestTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LeveEditor
{
    public class LevelEditorController : MonoBehaviour
    {
        [SerializeField]
        private Button _saveButton;

        [SerializeField]
        private Button _refresh;

        [SerializeField]
        private EditorPrefabController _editorPrefabController;

        private List<EditorPrefabController> _listPrefabs = new List<EditorPrefabController>();

        [SerializeField]
        private int _countBlocks;

        [SerializeField]
        private Transform _spawnTransform;

        public string nameFile = "test.txt";

        private void Start()
        {
            _saveButton.onClick.AddListener(SaveLevel);
            _refresh.onClick.AddListener(RefreshScene);
            CreateBlock();
        }

        private void CreateBlock()
        {
            for (int i = 0; i < _countBlocks; i++)
            {
                var obj = Instantiate(_editorPrefabController, _spawnTransform);
                _listPrefabs.Add(_editorPrefabController);
            }
        }

        private void RefreshScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void SaveLevel()
        {
            var nums = _listPrefabs.Select(prefabController => prefabController.GetNum()).ToList();
            StreamWriter sw = new StreamWriter(Application.dataPath + "/" + nameFile);
 

            sw.WriteLine();
            sw.Close();
        }
    }
}