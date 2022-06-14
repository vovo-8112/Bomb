using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Utilities/MMSceneRestarter")]
    public class MMSceneRestarter : MonoBehaviour
    {
        public enum RestartModes { ActiveScene, SpecificScene }

        [Header("Settings")]
        public RestartModes RestartMode = RestartModes.ActiveScene;
        [MMEnumCondition("RestartMode", (int)RestartModes.SpecificScene)]
        public string SceneName;
        public LoadSceneMode LoadMode = LoadSceneMode.Single;

        [Header("Input")]
        public KeyCode RestarterKeyCode = KeyCode.Backspace;

        protected string _newSceneName;
        protected virtual void Update()
        {
            HandleInput();
        }
        protected virtual void HandleInput()
        {
            if (Input.GetKeyDown(RestarterKeyCode))
            {
                RestartScene();
            }
        }
        public virtual void RestartScene()
        {
            Debug.Log("Scene restarted by MMSceneRestarter");
            switch (RestartMode)
            {
                case RestartModes.ActiveScene:
                    Scene scene = SceneManager.GetActiveScene();
                    _newSceneName = scene.name;
                    break;

                case RestartModes.SpecificScene:
                    _newSceneName = SceneName;
                    break;
            }
            SceneManager.LoadScene(_newSceneName, LoadMode);
        }
    }
}
