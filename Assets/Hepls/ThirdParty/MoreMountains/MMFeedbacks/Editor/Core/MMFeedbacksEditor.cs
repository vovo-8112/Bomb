using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks {
  [CustomEditor(typeof(MMFeedbacks))]
  public class MMFeedbacksEditor : Editor {
    public class FeedbackTypePair {
      public System.Type FeedbackType;
      public string FeedbackName;
    }
    static class FeedbackCopy {

      static public System.Type Type { get; private set; }
      static List<SerializedProperty> Properties = new List<SerializedProperty>();

      static string[] IgnoreList = new string[] {
        "m_ObjectHideFlags",
        "m_CorrespondingSourceObject",
        "m_PrefabInstance",
        "m_PrefabAsset",
        "m_GameObject",
        "m_Enabled",
        "m_EditorHideFlags",
        "m_Script",
        "m_Name",
        "m_EditorClassIdentifier"
      };

      static public void Copy(SerializedObject serializedObject) {
        Type = serializedObject.targetObject.GetType();
        Properties.Clear();

        SerializedProperty property = serializedObject.GetIterator();
        property.Next(true);
        do {
          if (!IgnoreList.Contains(property.name)) {
            Properties.Add(property.Copy());
          }
        } while (property.Next(false));
      }

      static public void Paste(SerializedObject target) {
        if (target.targetObject.GetType() == Type) {
          for (int i = 0; i < Properties.Count; i++) {
            target.CopyFromSerializedProperty(Properties[i]);
          }
        }
      }

      static public bool HasCopy() {
        return Properties != null && Properties.Count > 0;
      }

      static public void CopyAll(MMFeedbacks sourceFeedbacks) {
        MMFeedbacksConfiguration.Instance._mmFeedbacks = sourceFeedbacks;
      }

      static public bool HasMultipleCopies() {
        return (MMFeedbacksConfiguration.Instance._mmFeedbacks != null);
      }

      static public void PasteAll(MMFeedbacksEditor targetEditor) {
        var sourceFeedbacks = new SerializedObject(MMFeedbacksConfiguration.Instance._mmFeedbacks);
        SerializedProperty feedbacks = sourceFeedbacks.FindProperty("Feedbacks");

        for (int i = 0; i < feedbacks.arraySize; i++) {
          MMFeedback arrayFeedback = (feedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);

          FeedbackCopy.Copy(new SerializedObject(arrayFeedback));
          MMFeedback newFeedback = targetEditor.AddFeedback(arrayFeedback.GetType());
          SerializedObject serialized = new SerializedObject(newFeedback);
          serialized.Update();
          FeedbackCopy.Paste(serialized);
          serialized.ApplyModifiedProperties();
        }

        MMFeedbacksConfiguration.Instance._mmFeedbacks = null;
      }
    }

    protected MMFeedbacks _targetMMFeedbacks;
    protected SerializedProperty _mmfeedbacks;
    protected SerializedProperty _mmfeedbacksInitializationMode;
    protected SerializedProperty _mmfeedbacksSafeMode;
    protected SerializedProperty _mmfeedbacksAutoPlayOnStart;
    protected SerializedProperty _mmfeedbacksAutoPlayOnEnable;
    protected SerializedProperty _mmfeedbacksDirection;
    protected SerializedProperty _mmfeedbacksFeedbacksIntensity;
    protected SerializedProperty _mmfeedbacksAutoChangeDirectionOnEnd;
    protected SerializedProperty _mmfeedbacksDurationMultiplier;
    protected SerializedProperty _mmfeedbacksDisplayFullDurationDetails;
    protected SerializedProperty _mmfeedbacksCooldownDuration;
    protected SerializedProperty _mmfeedbacksInitialDelay;
    protected SerializedProperty _mmfeedbacksCanPlayWhileAlreadyPlaying;
    protected SerializedProperty _mmfeedbacksEvents;

    protected Dictionary<MMFeedback, Editor> _editors;
    protected List<FeedbackTypePair> _typesAndNames = new List<FeedbackTypePair>();
    protected string[] _typeDisplays;
    protected int _draggedStartID = -1;
    protected int _draggedEndID = -1;
    private static bool _debugView = false;
    protected Color _originalBackgroundColor;
    protected Color _scriptDrivenBoxColor;
    protected Texture2D _scriptDrivenBoxBackgroundTexture;
    protected Color _scriptDrivenBoxColorFrom = new Color(1f, 0f, 0f, 1f);
    protected Color _scriptDrivenBoxColorTo = new Color(0.7f, 0.1f, 0.1f, 1f);
    protected Color _playButtonColor = new Color32(193, 255, 2, 255);
    private static bool _settingsMenuDropdown;
    private static bool _eventsMenuDropdown;
    protected GUIStyle _directionButtonStyle;
    protected GUIStyle _playingStyle;
    void OnEnable() {
      _targetMMFeedbacks = target as MMFeedbacks;
      _mmfeedbacks = serializedObject.FindProperty("Feedbacks");
      _mmfeedbacksInitializationMode = serializedObject.FindProperty("InitializationMode");
      _mmfeedbacksSafeMode = serializedObject.FindProperty("SafeMode");
      _mmfeedbacksAutoPlayOnStart = serializedObject.FindProperty("AutoPlayOnStart");
      _mmfeedbacksAutoPlayOnEnable = serializedObject.FindProperty("AutoPlayOnEnable");
      _mmfeedbacksDirection = serializedObject.FindProperty("Direction");
      _mmfeedbacksAutoChangeDirectionOnEnd = serializedObject.FindProperty("AutoChangeDirectionOnEnd");
      _mmfeedbacksDurationMultiplier = serializedObject.FindProperty("DurationMultiplier");
      _mmfeedbacksDisplayFullDurationDetails = serializedObject.FindProperty("DisplayFullDurationDetails");
      _mmfeedbacksCooldownDuration = serializedObject.FindProperty("CooldownDuration");
      _mmfeedbacksInitialDelay = serializedObject.FindProperty("InitialDelay");
      _mmfeedbacksCanPlayWhileAlreadyPlaying = serializedObject.FindProperty("CanPlayWhileAlreadyPlaying");
      _mmfeedbacksFeedbacksIntensity = serializedObject.FindProperty("FeedbacksIntensity");

      _mmfeedbacksEvents = serializedObject.FindProperty("Events");
      _originalBackgroundColor = GUI.backgroundColor;
      RepairRoutine();
      _editors = new Dictionary<MMFeedback, Editor>();
      for (int i = 0; i < _mmfeedbacks.arraySize; i++) {
        AddEditor(_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);
      }
      List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
        from assemblyType in domainAssembly.GetTypes()
        where assemblyType.IsSubclassOf(typeof(MMFeedback))
        select assemblyType).ToList();
      List<string> typeNames = new List<string>();
      for (int i = 0; i < types.Count; i++) {
        FeedbackTypePair newType = new FeedbackTypePair();
        newType.FeedbackType = types[i];
        newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
        if (newType.FeedbackName == "MMFeedbackBase") {
          continue;
        }

        _typesAndNames.Add(newType);
      }

      _typesAndNames = _typesAndNames.OrderBy(t => t.FeedbackName).ToList();

      typeNames.Add("Add new feedback...");
      for (int i = 0; i < _typesAndNames.Count; i++) {
        typeNames.Add(_typesAndNames[i].FeedbackName);
      }

      _typeDisplays = typeNames.ToArray();

      _directionButtonStyle = new GUIStyle();
      _directionButtonStyle.border.left = 0;
      _directionButtonStyle.border.right = 0;
      _directionButtonStyle.border.top = 0;
      _directionButtonStyle.border.bottom = 0;

      _playingStyle = new GUIStyle();
      _playingStyle.normal.textColor = Color.yellow;
    }
    protected virtual void RepairRoutine() {
      _targetMMFeedbacks = target as MMFeedbacks;
      if ((_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.EditorOnly) ||
          (_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.Full)) {
        _targetMMFeedbacks.AutoRepair();
      }

      serializedObject.ApplyModifiedProperties();
    }
    public override void OnInspectorGUI() {
      var e = Event.current;
      serializedObject.Update();
      Undo.RecordObject(target, "Modified Feedback Manager");

      EditorGUILayout.Space();

      if (!MMFeedbacks.GlobalMMFeedbacksActive) {
        Color baseColor = GUI.color;
        GUI.color = Color.red;
        EditorGUILayout.HelpBox(
          "All MMFeedbacks, including this one, are currently disabled. This is done via script, by changing the value of the MMFeedbacks.GlobalMMFeedbacksActive boolean. Right now this value has been set to false. Setting it back to true will allow MMFeedbacks to play again.",
          MessageType.Warning);
        EditorGUILayout.Space();
        GUI.color = baseColor;
      }

      if (MMFeedbacksConfiguration.Instance.ShowInspectorTips) {
        EditorGUILayout.HelpBox(
          "Select feedbacks from the 'add a feedback' dropdown and customize them. Remember, if you don't use auto initialization (Awake or Start), " +
          "you'll need to initialize them via script.", MessageType.None);
      }

      Rect helpBoxRect = GUILayoutUtility.GetLastRect();

      _settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, "Settings", true, EditorStyles.foldout);
      if (_settingsMenuDropdown) {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Initialization", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksSafeMode);
        EditorGUILayout.PropertyField(_mmfeedbacksInitializationMode);
        EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnStart);
        EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnEnable);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Direction", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksDirection);
        EditorGUILayout.PropertyField(_mmfeedbacksAutoChangeDirectionOnEnd);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Intensity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksFeedbacksIntensity);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksDurationMultiplier);
        EditorGUILayout.PropertyField(_mmfeedbacksDisplayFullDurationDetails);
        EditorGUILayout.PropertyField(_mmfeedbacksCooldownDuration);
        EditorGUILayout.PropertyField(_mmfeedbacksInitialDelay);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Play Conditions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksCanPlayWhileAlreadyPlaying);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_mmfeedbacksEvents);
      }

      float durationRectWidth = 70f;
      Rect durationRect = new Rect(helpBoxRect.xMax - durationRectWidth, helpBoxRect.yMax + 6, durationRectWidth, 17f);
      durationRect.xMin = helpBoxRect.xMax - durationRectWidth;
      durationRect.xMax = helpBoxRect.xMax;

      float playingRectWidth = 70f;
      Rect playingRect = new Rect(helpBoxRect.xMax - playingRectWidth - durationRectWidth, helpBoxRect.yMax + 6,
        playingRectWidth, 17f);
      playingRect.xMin = helpBoxRect.xMax - durationRectWidth - playingRectWidth;
      playingRect.xMax = helpBoxRect.xMax;

      float directionRectWidth = 16f;
      Rect directionRect =
        new Rect(helpBoxRect.xMax - directionRectWidth, helpBoxRect.yMax + 5, directionRectWidth, 17f);
      directionRect.xMin = helpBoxRect.xMax - directionRectWidth;
      directionRect.xMax = helpBoxRect.xMax;

      if ((target as MMFeedbacks).IsPlaying) {
        GUI.Label(playingRect, "[PLAYING] ", _playingStyle);
      }

      GUI.Label(durationRect, "[" + _targetMMFeedbacks.TotalDuration.ToString("F2") + "s]");

      if (_targetMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop) {
        Texture arrowUpIcon = Resources.Load("FeelArrowUp") as Texture;
        GUIContent directionIcon = new GUIContent(arrowUpIcon);

        if (GUI.Button(directionRect, directionIcon, _directionButtonStyle)) {
          _targetMMFeedbacks.Revert();
        }
      } else {
        Texture arrowDownIcon = Resources.Load("FeelArrowDown") as Texture;
        GUIContent directionIcon = new GUIContent(arrowDownIcon);

        if (GUI.Button(directionRect, directionIcon, _directionButtonStyle)) {
          _targetMMFeedbacks.Revert();
        }
      }

      MMFeedbackStyling.DrawSection("Feedbacks");

      for (int i = 0; i < _mmfeedbacks.arraySize; i++) {
        MMFeedbackStyling.DrawSplitter();

        SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(i);
        if (property.objectReferenceValue == null) {
          continue;
        }

        MMFeedback feedback = property.objectReferenceValue as MMFeedback;
        feedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;

        Undo.RecordObject(feedback, "Modified Feedback");

        int id = i;
        bool isExpanded = property.isExpanded;
        string label = feedback.Label;
        bool pause = false;

        if (feedback.Pause != null) {
          pause = true;
        }

        if ((feedback.LooperPause == true) && (Application.isPlaying)) {
          if ((feedback as MMFeedbackLooper).InfiniteLoop) {
            label = label + "[Infinite Loop] ";
          } else {
            label = label + "[ " + (feedback as MMFeedbackLooper).NumberOfLoopsLeft + " loops left ] ";
          }
        }

        Rect headerRect = MMFeedbackStyling.DrawHeader(
          ref isExpanded,
          ref feedback.Active,
          label,
          feedback.FeedbackColor,
          (GenericMenu menu) => {
            if (Application.isPlaying)
              menu.AddItem(new GUIContent("Play"), false, () => PlayFeedback(id));
            else
              menu.AddDisabledItem(new GUIContent("Play"));
            menu.AddSeparator(null);
            menu.AddItem(new GUIContent("Remove"), false, () => RemoveFeedback(id));
            menu.AddSeparator(null);
            menu.AddItem(new GUIContent("Copy"), false, () => CopyFeedback(id));
            if (FeedbackCopy.HasCopy() && FeedbackCopy.Type == feedback.GetType())
              menu.AddItem(new GUIContent("Paste"), false, () => PasteFeedback(id));
            else
              menu.AddDisabledItem(new GUIContent("Paste"));
          },
          feedback.FeedbackStartedAt,
          feedback.FeedbackDuration,
          feedback.TotalDuration,
          feedback.Timing,
          pause,
          _targetMMFeedbacks
        );

        switch (e.type) {
          case EventType.MouseDown:
            if (headerRect.Contains(e.mousePosition)) {
              _draggedStartID = i;
              e.Use();
            }

            break;
          default:
            break;
        }

        if (_draggedStartID == i && headerRect != Rect.zero) {
          Color color = new Color(0, 1, 1, 0.2f);
          EditorGUI.DrawRect(headerRect, color);
        }

        if (headerRect.Contains(e.mousePosition)) {
          if (_draggedStartID >= 0) {
            _draggedEndID = i;

            Rect headerSplit = headerRect;
            headerSplit.height *= 0.5f;
            headerSplit.y += headerSplit.height;
            if (headerSplit.Contains(e.mousePosition))
              _draggedEndID = i + 1;
          }
        }

        property.isExpanded = isExpanded;
        if (isExpanded) {
          EditorGUI.BeginDisabledGroup(!feedback.Active);

          string helpText = FeedbackHelpAttribute.GetFeedbackHelpText(feedback.GetType());

          if ((!string.IsNullOrEmpty(helpText)) && (MMFeedbacksConfiguration.Instance.ShowInspectorTips)) {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            float newHeight = style.CalcHeight(new GUIContent(helpText), EditorGUIUtility.currentViewWidth);
            EditorGUILayout.LabelField(helpText, style);
          }

          EditorGUILayout.Space();

          if (!_editors.ContainsKey(feedback)) {
            AddEditor(feedback);
          }

          Editor editor = _editors[feedback];
          CreateCachedEditor(feedback, feedback.GetType(), ref editor);

          editor.OnInspectorGUI();

          EditorGUI.EndDisabledGroup();

          EditorGUILayout.Space();

          EditorGUI.BeginDisabledGroup(!Application.isPlaying);
          EditorGUILayout.BeginHorizontal();
          {
            if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
              PlayFeedback(id);
            }

            if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
              StopFeedback(id);
            }
          }
          EditorGUILayout.EndHorizontal();
          EditorGUI.EndDisabledGroup();

          EditorGUILayout.Space();
          EditorGUILayout.Space();
        }
      }

      if (_mmfeedbacks.arraySize > 0) {
        MMFeedbackStyling.DrawSplitter();
      }

      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      {

        int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;
        if (newItem >= 0) {
          AddFeedback(_typesAndNames[newItem].FeedbackType);
        }

        if (FeedbackCopy.HasCopy()) {
          if (GUILayout.Button("Paste as new", EditorStyles.miniButton,
            GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x))) {
            PasteAsNew();
          }
        }

        if (FeedbackCopy.HasMultipleCopies()) {
          if (GUILayout.Button("Paste all as new", EditorStyles.miniButton,
            GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste all as new")).x))) {
            PasteAllAsNew();
          }
        }
      }

      if (!FeedbackCopy.HasMultipleCopies()) {
        if (GUILayout.Button("Copy all", EditorStyles.miniButton,
          GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x))) {
          CopyAll();
        }
      }

      EditorGUILayout.EndHorizontal();

      if (_draggedStartID >= 0 && _draggedEndID >= 0) {
        if (_draggedEndID != _draggedStartID) {
          if (_draggedEndID > _draggedStartID)
            _draggedEndID--;
          _mmfeedbacks.MoveArrayElement(_draggedStartID, _draggedEndID);
          _draggedStartID = _draggedEndID;
        }
      }

      if (_draggedStartID >= 0 || _draggedEndID >= 0) {
        switch (e.type) {
          case EventType.MouseUp:
            _draggedStartID = -1;
            _draggedEndID = -1;
            e.Use();
            break;
          default:
            break;
        }
      }

      bool wasRemoved = false;
      for (int i = _mmfeedbacks.arraySize - 1; i >= 0; i--) {
        if (_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue == null) {
          wasRemoved = true;
          _mmfeedbacks.DeleteArrayElementAtIndex(i);
        }
      }

      if (wasRemoved) {
        GameObject gameObject = (target as MMFeedbacks).gameObject;
        foreach (var c in gameObject.GetComponents<Component>()) {
          if (c != null) {
            c.hideFlags = HideFlags.None;
          }
        }
      }

      serializedObject.ApplyModifiedProperties();

      MMFeedbackStyling.DrawSection("All Feedbacks Debug");

      EditorGUI.BeginDisabledGroup(!Application.isPlaying);
      EditorGUILayout.BeginHorizontal();
      {
        if (GUILayout.Button("Initialize", EditorStyles.miniButtonLeft)) {
          (target as MMFeedbacks).Initialization();
        }
        _originalBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = _playButtonColor;
        if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
          (target as MMFeedbacks).PlayFeedbacks();
        }

        GUI.backgroundColor = _originalBackgroundColor;
        if ((target as MMFeedbacks).ContainsLoop) {
          if (GUILayout.Button("Pause", EditorStyles.miniButtonMid)) {
            (target as MMFeedbacks).PauseFeedbacks();
          }
        }
        if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
          (target as MMFeedbacks).StopFeedbacks();
        }
        if (GUILayout.Button("Reset", EditorStyles.miniButtonMid)) {
          (target as MMFeedbacks).ResetFeedbacks();
        }

        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("Revert", EditorStyles.miniButtonMid)) {
          (target as MMFeedbacks).Revert();
        }
        EditorGUI.BeginChangeCheck();
        {
          _debugView = GUILayout.Toggle(_debugView, "Debug View", EditorStyles.miniButtonRight);

          if (EditorGUI.EndChangeCheck()) {
            foreach (var f in (target as MMFeedbacks).Feedbacks)
              f.hideFlags = _debugView ? HideFlags.HideInInspector : HideFlags.None;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
          }
        }
      }
      EditorGUILayout.EndHorizontal();

      float pingPong = Mathf.PingPong(Time.unscaledTime, 0.25f);
      if (_targetMMFeedbacks.InScriptDrivenPause) {
        _scriptDrivenBoxColor = Color.Lerp(_scriptDrivenBoxColorFrom, _scriptDrivenBoxColorTo, pingPong);
        GUI.skin.box.normal.background = Texture2D.whiteTexture;
        GUI.backgroundColor = _scriptDrivenBoxColor;
        GUI.skin.box.normal.textColor = Color.black;
        GUILayout.Box("Script driven pause in progress, call Resume() to exit pause", GUILayout.ExpandWidth(true));
        GUI.backgroundColor = _originalBackgroundColor;
        GUI.skin.box.normal.background = _scriptDrivenBoxBackgroundTexture;
        if (GUILayout.Button("Resume")) {
          _targetMMFeedbacks.ResumeFeedbacks();
        }
      }
      if (_debugView) {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(_mmfeedbacks, true);
        EditorGUI.EndDisabledGroup();
      }
    }
    public override bool RequiresConstantRepaint() {
      return true;
    }
    protected virtual MMFeedback AddFeedback(System.Type type) {
      GameObject gameObject = (target as MMFeedbacks).gameObject;

      MMFeedback newFeedback = Undo.AddComponent(gameObject, type) as MMFeedback;
      newFeedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;
      newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(type);

      AddEditor(newFeedback);

      _mmfeedbacks.arraySize++;
      _mmfeedbacks.GetArrayElementAtIndex(_mmfeedbacks.arraySize - 1).objectReferenceValue = newFeedback;

      return newFeedback;
    }
    protected virtual void AddEditor(MMFeedback feedback) {
      if (feedback == null)
        return;

      if (!_editors.ContainsKey(feedback)) {
        Editor editor = null;
        CreateCachedEditor(feedback, null, ref editor);

        _editors.Add(feedback, editor as Editor);
      }
    }
    protected virtual void RemoveEditor(MMFeedback feedback) {
      if (feedback == null)
        return;

      if (_editors.ContainsKey(feedback)) {
        DestroyImmediate(_editors[feedback]);
        _editors.Remove(feedback);
      }
    }
    protected virtual void InitializeFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;
      feedback.Initialization(feedback.gameObject);
    }
    protected virtual void PlayFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;
      feedback.Play(feedback.transform.position, _targetMMFeedbacks.FeedbacksIntensity);
    }
    protected virtual void StopFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;
      feedback.Stop(feedback.transform.position);
    }
    protected virtual void ResetFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;
      feedback.ResetFeedback();
    }
    protected virtual void RemoveFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;

      (target as MMFeedbacks).Feedbacks.Remove(feedback);

      _editors.Remove(feedback);
      Undo.DestroyObjectImmediate(feedback);
    }
    protected virtual void CopyFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;

      FeedbackCopy.Copy(new SerializedObject(feedback));
    }
    protected virtual void CopyAll() {
      FeedbackCopy.CopyAll(target as MMFeedbacks);
    }
    protected virtual void PasteFeedback(int id) {
      SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
      MMFeedback feedback = property.objectReferenceValue as MMFeedback;

      SerializedObject serialized = new SerializedObject(feedback);

      FeedbackCopy.Paste(serialized);
      serialized.ApplyModifiedProperties();
    }
    protected virtual void PasteAsNew() {
      MMFeedback newFeedback = AddFeedback(FeedbackCopy.Type);
      SerializedObject serialized = new SerializedObject(newFeedback);

      serialized.Update();
      FeedbackCopy.Paste(serialized);
      serialized.ApplyModifiedProperties();
    }
    protected virtual void PasteAllAsNew() {
      serializedObject.Update();
      Undo.RecordObject(target, "Paste all MMFeedbacks");
      FeedbackCopy.PasteAll(this);
      serializedObject.ApplyModifiedProperties();
    }
  }
}