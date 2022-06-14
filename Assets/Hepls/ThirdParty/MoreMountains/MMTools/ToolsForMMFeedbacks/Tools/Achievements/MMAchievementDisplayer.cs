using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Achievements/MMAchievementDisplayer")]
    public class MMAchievementDisplayer : MonoBehaviour, MMEventListener<MMAchievementUnlockedEvent>
	{
		[Header("Achievements")]
		public MMAchievementDisplayItem AchievementDisplayPrefab;
		public float AchievementDisplayDuration = 5f;
		public float AchievementFadeDuration = 0.2f;

		protected WaitForSeconds _achievementFadeOutWFS;
		public virtual IEnumerator DisplayAchievement(MMAchievement achievement)
		{
			if ((this.transform == null) || (AchievementDisplayPrefab == null))
			{
				yield break;
			}
			GameObject instance = (GameObject)Instantiate(AchievementDisplayPrefab.gameObject);
			instance.transform.SetParent(this.transform,false);
			MMAchievementDisplayItem achievementDisplay = instance.GetComponent<MMAchievementDisplayItem> ();
			if (achievementDisplay == null)
			{
				yield break;
			}
			achievementDisplay.Title.text = achievement.Title;
			achievementDisplay.Description.text = achievement.Description;
			achievementDisplay.Icon.sprite = achievement.UnlockedImage;
			if (achievement.AchievementType == AchievementTypes.Progress)
			{
				achievementDisplay.ProgressBarDisplay.gameObject.SetActive(true);
			}
			else
			{
				achievementDisplay.ProgressBarDisplay.gameObject.SetActive(false);
			}
			if (achievement.UnlockedSound != null)
			{
				MMSfxEvent.Trigger (achievement.UnlockedSound);
			}
			CanvasGroup achievementCanvasGroup = instance.GetComponent<CanvasGroup> ();
			if (achievementCanvasGroup != null)
			{
				achievementCanvasGroup.alpha = 0;
				StartCoroutine(MMFade.FadeCanvasGroup(achievementCanvasGroup, AchievementFadeDuration, 1));
				yield return _achievementFadeOutWFS;
				StartCoroutine(MMFade.FadeCanvasGroup(achievementCanvasGroup, AchievementFadeDuration, 0));
			}
		}
		public virtual void OnMMEvent(MMAchievementUnlockedEvent achievementUnlockedEvent)
		{
			StartCoroutine(DisplayAchievement (achievementUnlockedEvent.Achievement));
		}
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMAchievementUnlockedEvent>();
			_achievementFadeOutWFS = new WaitForSeconds (AchievementFadeDuration + AchievementDisplayDuration);
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMAchievementUnlockedEvent>();
		}
	}
}