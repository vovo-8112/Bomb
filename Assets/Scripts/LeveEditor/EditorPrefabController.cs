using System;
using TMPro;
using UnityEngine;

namespace LeveEditor
{
    public class EditorPrefabController : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _inputField;

        private int Num;

        public int GetNum()
        {
            ReadText();
            return Num;
        }

        private void ReadText()
        {
            int int32 = Int32.Parse(_inputField.text);
            Num = int32;
        }
    }
}