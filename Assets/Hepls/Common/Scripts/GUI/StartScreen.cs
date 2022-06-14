using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/GUI/StartScreen")]
    public class StartScreen : MonoBehaviour
	{
		[Tooltip("the level to load after the start screen")]
		public string NextLevel;
		[Tooltip("the delay after which the level should auto skip (if less than 1s, won't autoskip)")]
		public float AutoSkipDelay = 0f;

		[Header("Fades")]
		[Tooltip("the duration of the fade from black at the start of the level")]
		public float FadeInDuration = 1f;
		[Tooltip("the duration of the fade to black at the end of the level")]
		public float FadeOutDuration = 1f;
        [Tooltip("the tween type to use to fade the startscreen in and out ")]
		public MMTweenType Tween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);

		[Header("Sound Settings Bindings")]
		[Tooltip("the switch used to turn the music on or off")]
		public MMSwitch MusicSwitch;
		[Tooltip("the switch used to turn the SFX on or off")]
		public MMSwitch SfxSwitch;
		protected virtual void Awake()
		{	
			GUIManager.Instance.SetHUDActive (false);
			MMFadeOutEvent.Trigger(FadeInDuration, Tween);
            Cursor.visible = true;
			if (AutoSkipDelay > 1f)
			{
				FadeOutDuration = AutoSkipDelay;
				StartCoroutine (LoadFirstLevel ());
			}
		}
		protected async void Start()
		{
			await Task.Delay(1);
			
			if (MusicSwitch != null)
			{
				MusicSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.MusicOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				MusicSwitch.InitializeState ();
			}

			if (SfxSwitch != null)
			{
				SfxSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.SfxOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				SfxSwitch.InitializeState ();
			}
		}
		protected virtual void Update()
		{
			if (!Input.GetButtonDown ("Player1_Jump"))
				return;
			
			ButtonPressed ();
		}
		public virtual void ButtonPressed()
		{
			MMFadeInEvent.Trigger(FadeOutDuration, Tween);
			StartCoroutine (LoadFirstLevel ());
		}
		protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds (FadeOutDuration);
			MMSceneLoadingManager.LoadScene (NextLevel);
		}
	}
}