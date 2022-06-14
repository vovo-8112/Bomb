using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class ProgressEvent : UnityEvent<float>{}
	[Serializable]
	public class MMAdditiveSceneLoadingManagerSettings
	{
		[Tooltip("the name of the MMSceneLoadingManager scene you want to use when in additive mode")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		[Tooltip("when in additive loading mode, the thread priority to apply to the loading")]
		public ThreadPriority ThreadPriority = ThreadPriority.High;
		[Tooltip("whether or not to make additional sanity checks (better leave this to true)")]
		public bool SecureLoad = true;
		[Tooltip("when in additive loading mode, whether or not to interpolate the progress bar's progress")]
		public bool InterpolateProgress = true;
		[Tooltip("when in additive loading mode, when in additive loading mode, the duration (in seconds) of the delay before the entry fade")]
		public float BeforeEntryFadeDelay = 0f;
		[Tooltip("when in additive loading mode, the duration (in seconds) of the entry fade")]
		public float EntryFadeDuration = 0.25f;
		[Tooltip("when in additive loading mode, the duration (in seconds) of the delay before the entry fade")]
		public float AfterEntryFadeDelay = 0.1f;
		[Tooltip("when in additive loading mode, the duration (in seconds) of the delay before the exit fade")]
		public float BeforeExitFadeDelay = 0.25f;
		[Tooltip("when in additive loading mode, the duration (in seconds) of the exit fade")]
		public float ExitFadeDuration = 0.2f;
		[Tooltip("when in additive loading mode, when in additive loading mode, the tween to use to fade on entry")]
		public MMTweenType EntryFadeTween = null;
		[Tooltip("when in additive loading mode, the tween to use to fade on exit")]
		public MMTweenType ExitFadeTween = null;
		[Tooltip("when in additive loading mode, the speed at which the loader's progress bar should move")]
		public float ProgressBarSpeed = 5f;
		[Tooltip("when in additive loading mode, the selective additive fade mode")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
	}
	public class MMAdditiveSceneLoadingManager : MonoBehaviour 
	{
		public enum FadeModes { FadeInThenOut, FadeOutThenIn }

		[Header("Audio Listener")] 
		public AudioListener LoadingAudioListener;
		
        [Header("Settings")]
        [Tooltip("the ID on which to trigger a fade, has to match the ID on the fader in your scene")]
        public int FaderID = 500;
        [Tooltip("whether or not to output debug messages to the console")]
        public bool DebugMode = false;

        [Header("Progress Events")]
        [Tooltip("an event used to update progress")]
        public ProgressEvent SetRealtimeProgressValue;
        [Tooltip("an event used to update progress with interpolation")]
        public ProgressEvent SetInterpolatedProgressValue;

        [Header("State Events")]
        [Tooltip("an event that will be invoked when the load starts")]
        public UnityEvent OnLoadStarted;
        [Tooltip("an event that will be invoked when the delay before the entry fade starts")]
        public UnityEvent OnBeforeEntryFade;
        [Tooltip("an event that will be invoked when the entry fade starts")]
        public UnityEvent OnEntryFade;
        [Tooltip("an event that will be invoked when the delay after the entry fade starts")]
        public UnityEvent OnAfterEntryFade;
        [Tooltip("an event that will be invoked when the origin scene gets unloaded")]
        public UnityEvent OnUnloadOriginScene;
        [Tooltip("an event that will be invoked when the destination scene starts loading")]
        public UnityEvent OnLoadDestinationScene;
        [Tooltip("an event that will be invoked when the load of the destination scene is complete")]
        public UnityEvent OnLoadProgressComplete;
        [Tooltip("an event that will be invoked when the interpolated load of the destination scene is complete")]
        public UnityEvent OnInterpolatedLoadProgressComplete;
        [Tooltip("an event that will be invoked when the delay before the exit fade starts")]
        public UnityEvent OnBeforeExitFade;
        [Tooltip("an event that will be invoked when the exit fade starts")]
        public UnityEvent OnExitFade;
        [Tooltip("an event that will be invoked when the destination scene gets activated")]
        public UnityEvent OnDestinationSceneActivation;
        [Tooltip("an event that will be invoked when the scene loader gets unloaded")]
        public UnityEvent OnUnloadSceneLoader;
        [Tooltip("an event that will be invoked when the whole transition is complete")]
        public UnityEvent OnLoadTransitionComplete;

        protected static bool _interpolateProgress;
        protected static float _progressInterpolationSpeed;
        protected static float _beforeEntryFadeDelay;
        protected static MMTweenType _entryFadeTween;
        protected static float _entryFadeDuration;
        protected static float _afterEntryFadeDelay;
        protected static float _beforeExitFadeDelay;
        protected static MMTweenType _exitFadeTween;
        protected static float _exitFadeDuration;
        protected static FadeModes _fadeMode;
        protected static string _sceneToLoadName = "";
        protected static string _loadingScreenSceneName;
        protected static List<string> _scenesInBuild;
        protected static Scene[] _initialScenes;
        protected float _loadProgress = 0f;
        protected float _interpolatedLoadProgress;
        protected static bool _loadingInProgress = false;
        protected AsyncOperation _unloadOriginAsyncOperation;
        protected AsyncOperation _loadDestinationAsyncOperation;
        protected AsyncOperation _unloadLoadingAsyncOperation;
        protected bool _setRealtimeProgressValueIsNull;
        protected bool _setInterpolatedProgressValueIsNull;
        protected const float _asyncProgressLimit = 0.9f;
        public static void LoadScene(string sceneToLoadName, MMAdditiveSceneLoadingManagerSettings settings)
        {
	        LoadScene(sceneToLoadName, settings.LoadingSceneName, settings.ThreadPriority, settings.SecureLoad, settings.InterpolateProgress,
		        settings.BeforeEntryFadeDelay, settings.EntryFadeDuration, settings.AfterEntryFadeDelay, settings.BeforeExitFadeDelay,
		        settings.ExitFadeDuration, settings.EntryFadeTween, settings.ExitFadeTween, settings.ProgressBarSpeed, settings.FadeMode);
        }
        public static void LoadScene(string sceneToLoadName, string loadingSceneName = "MMAdditiveLoadingScreen", 
                                        ThreadPriority threadPriority = ThreadPriority.High, bool secureLoad = true,
                                        bool interpolateProgress = true,
                                        float beforeEntryFadeDelay = 0f,
                                        float entryFadeDuration = 0.25f,
                                        float afterEntryFadeDelay = 0.1f,
                                        float beforeExitFadeDelay = 0.25f,
                                        float exitFadeDuration = 0.2f, 
                                        MMTweenType entryFadeTween = null, MMTweenType exitFadeTween = null,
                                        float progressBarSpeed = 5f, 
                                        FadeModes fadeMode = FadeModes.FadeInThenOut)
        {
	        if (_loadingInProgress)
            {
	            Debug.LogError("MMLoadingSceneManagerAdditive : a request to load a new scene was emitted while a scene load was already in progress");  
                return;
            }

	        if (entryFadeTween == null)
	        {
		        entryFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
	        }

	        if (exitFadeTween == null)
	        {
		        exitFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
	        }

            if (secureLoad)
            {
	            _scenesInBuild = MMScene.GetScenesInBuild();
	            
	            if (!_scenesInBuild.Contains(sceneToLoadName))
	            {
		            Debug.LogError("MMLoadingSceneManagerAdditive : impossible to load the '"+sceneToLoadName+"' scene, " +
		                           "there is no such scene in the project's build settings.");
		            return;
	            }
	            if (!_scenesInBuild.Contains(loadingSceneName))
	            {
		            Debug.LogError("MMLoadingSceneManagerAdditive : impossible to load the '"+loadingSceneName+"' scene, " +
		                           "there is no such scene in the project's build settings.");
		            return;
	            }
            }

            _loadingInProgress = true;
            _initialScenes = MMScene.GetLoadedScenes();

            Application.backgroundLoadingPriority = threadPriority;
            _sceneToLoadName = sceneToLoadName;					
			_loadingScreenSceneName = loadingSceneName;
			_beforeEntryFadeDelay = beforeEntryFadeDelay;
			_entryFadeDuration = entryFadeDuration;
            _entryFadeTween = entryFadeTween;
            _afterEntryFadeDelay = afterEntryFadeDelay;
            _progressInterpolationSpeed = progressBarSpeed;
            _beforeExitFadeDelay = beforeExitFadeDelay;
            _exitFadeDuration = exitFadeDuration;
            _exitFadeTween = exitFadeTween;
            _fadeMode = fadeMode;
            _interpolateProgress = interpolateProgress;

            SceneManager.LoadScene(_loadingScreenSceneName, LoadSceneMode.Additive);
		}
		protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
	        MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initialization");

	        if (DebugMode)
	        {
		        foreach (Scene scene in _initialScenes)
		        {
			        MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initial scene : " + scene.name);
		        }    
	        }

	        _setRealtimeProgressValueIsNull = SetRealtimeProgressValue == null;
            _setInterpolatedProgressValueIsNull = SetInterpolatedProgressValue == null;
            Time.timeScale = 1f;

            if ((_sceneToLoadName == "") || (_loadingScreenSceneName == ""))
            {
	            return;
            }
            
            StartCoroutine(LoadSequence());
        }
		protected virtual void Update()
		{
			UpdateProgress();
		}
		protected virtual void UpdateProgress()
		{
			if (!_setRealtimeProgressValueIsNull)
			{
				SetRealtimeProgressValue.Invoke(_loadProgress);
			}

			if (_interpolateProgress)
			{
				_interpolatedLoadProgress = MMMaths.Approach(_interpolatedLoadProgress, _loadProgress, Time.deltaTime * _progressInterpolationSpeed);
				if (!_setInterpolatedProgressValueIsNull)
				{
					SetInterpolatedProgressValue.Invoke(_interpolatedLoadProgress);	
				}
			}
			else
			{
				SetInterpolatedProgressValue.Invoke(_loadProgress);	
			}
		}
		protected virtual IEnumerator LoadSequence()
		{
			InitiateLoad();
			yield return ProcessDelayBeforeEntryFade();
			yield return EntryFade();
			yield return ProcessDelayAfterEntryFade();
			yield return UnloadOriginScenes();
			yield return LoadDestinationScene();
			yield return ProcessDelayBeforeExitFade();
            yield return DestinationSceneActivation();
			yield return ExitFade();
			yield return UnloadSceneLoader();
			LoadTransitionComplete();
		}
		protected virtual void InitiateLoad()
		{
			_loadProgress = 0f;
			_interpolatedLoadProgress = 0f;
			Time.timeScale = 1f;
			SetAudioListener(false);
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initiate Load");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadStarted);
			OnLoadStarted?.Invoke();
		}
		protected virtual IEnumerator ProcessDelayBeforeEntryFade()
		{
			if (_beforeEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before entry fade, duration : " + _beforeEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeEntryFade);
				OnBeforeEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitFor(_beforeEntryFadeDelay);
			}
		}
		protected virtual IEnumerator EntryFade()
		{
			if (_entryFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : entry fade, duration : " + _entryFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.EntryFade);
				OnEntryFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					yield return null;
					MMFadeOutEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}
				else
				{
					yield return null;
					MMFadeInEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}           

				yield return MMCoroutine.WaitFor(_entryFadeDuration);
			}
		}
		protected virtual IEnumerator ProcessDelayAfterEntryFade()
		{
			if (_afterEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay after entry fade, duration : " + _afterEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.AfterEntryFade);
				OnAfterEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitFor(_afterEntryFadeDelay);
			}
		}
		protected virtual IEnumerator UnloadOriginScenes()
		{
			foreach (Scene scene in _initialScenes)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unload scene " + scene.name);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadOriginScene);
				OnUnloadOriginScene?.Invoke();
				
				
				_unloadOriginAsyncOperation = SceneManager.UnloadSceneAsync(scene);
				SetAudioListener(true);
				while (_unloadOriginAsyncOperation.progress < _asyncProgressLimit)
				{
					yield return null;
				}
			}
		}
		protected virtual IEnumerator LoadDestinationScene()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadDestinationScene);
			OnLoadDestinationScene?.Invoke();

			_loadDestinationAsyncOperation = SceneManager.LoadSceneAsync(_sceneToLoadName, LoadSceneMode.Additive );
            _loadDestinationAsyncOperation.completed += OnLoadOperationComplete;

            _loadDestinationAsyncOperation.allowSceneActivation = false;
            
			while (_loadDestinationAsyncOperation.progress < _asyncProgressLimit)
			{
				_loadProgress = _loadDestinationAsyncOperation.progress;
				yield return null;
			}
            
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load progress complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadProgressComplete);
			OnLoadProgressComplete?.Invoke();
			_loadProgress = 1f;
			if (_interpolateProgress)
			{
				while (_interpolatedLoadProgress < 1f)
				{
					yield return null;
				}
			}			

			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : interpolated load complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.InterpolatedLoadProgressComplete);
			OnInterpolatedLoadProgressComplete?.Invoke();
		}
        protected virtual IEnumerator ProcessDelayBeforeExitFade()
		{
			if (_beforeExitFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before exit fade, duration : " + _beforeExitFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeExitFade);
				OnBeforeExitFade?.Invoke();
				
				yield return MMCoroutine.WaitFor(_beforeExitFadeDelay);
			}
		}
		protected virtual IEnumerator ExitFade()
		{
			SetAudioListener(false);
			if (_exitFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : exit fade, duration : " + _exitFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.ExitFade);
				OnExitFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					MMFadeInEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				else
				{
					MMFadeOutEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				yield return MMCoroutine.WaitFor(_exitFadeDuration);
			}
		}
		protected virtual IEnumerator DestinationSceneActivation()
        {
            yield return MMCoroutine.WaitForFrames(1);
            MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : activating destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.DestinationSceneActivation);
			OnDestinationSceneActivation?.Invoke();
            _loadDestinationAsyncOperation.allowSceneActivation = true;
            while (_loadDestinationAsyncOperation.progress < 1.0f)
            {
                yield return null;
            }
        }
        protected virtual void OnLoadOperationComplete(AsyncOperation obj)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneToLoadName));
            MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : set active scene to " + _sceneToLoadName);

        }
        protected virtual IEnumerator UnloadSceneLoader()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unloading scene loader");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadSceneLoader);
			OnUnloadSceneLoader?.Invoke();
			
			yield return null;
			_unloadLoadingAsyncOperation = SceneManager.UnloadSceneAsync(_loadingScreenSceneName);
			while (_unloadLoadingAsyncOperation.progress < _asyncProgressLimit)
			{
				yield return null;
			}	
		}
		protected virtual void LoadTransitionComplete()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load transition complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadTransitionComplete);
			OnLoadTransitionComplete?.Invoke();
			
			_loadingInProgress = false;
		}
		protected virtual void SetAudioListener(bool state)
		{
			if (LoadingAudioListener != null)
			{
			}
		}
		protected virtual void OnDestroy()
		{
			_loadingInProgress = false;
		}
		protected virtual void MMLoadingSceneDebug(string message)
		{
			if (!DebugMode)
			{
				return;
			}
			
			string output = "";
			output += "<color=#82d3f9>[" + Time.frameCount + "]</color> ";
			output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
			output +=  message;
			Debug.Log(output);
		}
	}
}