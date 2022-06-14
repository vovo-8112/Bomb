using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/CharacterSwapManager")]
    public class CharacterSwapManager : MMSingleton<CharacterSwapManager>
    {
        [Tooltip("the name of the axis to use to catch input and trigger a swap on press")]
        public string SwapButtonName = "Player1_SwapCharacter";
        [Tooltip("the PlayerID set on the Characters you want to swap between")]
        public string PlayerID = "Player1";

        protected CharacterSwap[] _characterSwapArray;
        protected List<CharacterSwap> _characterSwapList;
        protected TopDownEngineEvent _swapEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwap, null);
        protected virtual void Start()
        {
            UpdateList();
        }
        public virtual void UpdateList()
        {
            _characterSwapArray = FindObjectsOfType<CharacterSwap>();
            _characterSwapList = new List<CharacterSwap>();
            for (int i = 0; i < _characterSwapArray.Length; i++)
            {
                if (_characterSwapArray[i].PlayerID == PlayerID)
                {
                    _characterSwapList.Add(_characterSwapArray[i]);
                }
            }

            if (_characterSwapList.Count == 0)
            {
                return;
            }
            _characterSwapList.Sort(SortSwapsByOrder);
        }
        static int SortSwapsByOrder(CharacterSwap a, CharacterSwap b)
        {
            return a.Order.CompareTo(b.Order);
        }
        protected virtual void Update()
        {
            HandleInput();
        }
        protected virtual void HandleInput()
        {
            if (Input.GetButtonDown(SwapButtonName))
            {
                SwapCharacter();
            }
        }
        public virtual void SwapCharacter()
        {
            if (_characterSwapList.Count < 2)
            {
                return;
            }

            int newIndex = -1;

            for (int i = 0; i < _characterSwapList.Count; i++)
            {
                if (_characterSwapList[i].Current())
                {
                    _characterSwapList[i].ResetCharacterSwap();
                    newIndex = i + 1;
                }
            }

            if (newIndex >= _characterSwapList.Count)
            {
                newIndex = 0;
            }
            _characterSwapList[newIndex].SwapToThisCharacter();

            LevelManager.Instance.Players[0] = _characterSwapList[newIndex].gameObject.GetComponentInParent<Character>();
            MMEventManager.TriggerEvent(_swapEvent);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
        }
    }
}