using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(RectTransform))]
	public class InventorySelectionMarker : MonoBehaviour 
	{
		[MMInformation("The selection marker will highlight the current selection. Here you can define its transition speed and minimal distance threshold (it's usually ok to leave it to default).",MMInformationAttribute.InformationType.Info,false)]
		public float TransitionSpeed=5f;
		public float MinimalTransitionDistance=0.01f;

		protected RectTransform _rectTransform;
		protected GameObject _currentSelection;
		protected Vector3 _originPosition;
		protected Vector3 _originLocalScale;
		protected Vector3 _originSizeDelta;
		protected float _originTime;
		protected bool _originIsNull=true;
		protected float _deltaTime;
		void Start () 
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		void Update () 
		{			
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}

            if (_currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>() == null)
            {
                return;
            }

            if (Vector3.Distance(transform.position,_currentSelection.transform.position) > MinimalTransitionDistance)
			{
				if (_originIsNull)
				{
					_originIsNull=false;
					_originPosition = transform.position;
					_originLocalScale = _rectTransform.localScale;
					_originSizeDelta = _rectTransform.sizeDelta;
					_originTime = Time.unscaledTime;
				} 
				_deltaTime =  (Time.unscaledTime - _originTime)*TransitionSpeed;
				transform.position= Vector3.Lerp(_originPosition,_currentSelection.transform.position,_deltaTime);
				_rectTransform.localScale = Vector3.Lerp(_originLocalScale, _currentSelection.GetComponent<RectTransform>().localScale,_deltaTime);
				_rectTransform.sizeDelta = Vector3.Lerp(_originSizeDelta, _currentSelection.GetComponent<RectTransform>().sizeDelta, _deltaTime);
			}
			else
			{
				_originIsNull=true;
			}
		}
	}
}