using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Threading.Tasks;

namespace MoreMountains.Feedbacks {
  public static class MMFDependencyInstaller {
    static ListRequest _listRequest;
    static AddRequest _addRequest;
    static int _currentIndex;

    private static string[] _packages = new string[] {
      "com.unity.cinemachine",
      "com.unity.postprocessing",
      "com.unity.textmeshpro",
      "com.unity.2d.animation"
    };
    [MenuItem("Tools/More Mountains/MMFeedbacks/Install All Dependencies", false, 701)]
    public static void InstallAllDependencies() {
      _currentIndex = 0;
      _listRequest = null;
      _addRequest = null;

      Debug.Log("[MMFDependencyInstaller] Installation start");
      _listRequest = Client.List();

      EditorApplication.update += ListProgress;
    }
    public async static void InstallFromPlay() {
      EditorApplication.ExitPlaymode();
      while (Application.isPlaying) {
        await Task.Delay(500);
      }

      await Task.Delay(500);

      ClearConsole();

      await Task.Delay(500);
      InstallAllDependencies();
    }
    public static void ClearConsole() {
      var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
      if (logEntries != null) {
        var clearMethod = logEntries.GetMethod("Clear",
          System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        if (clearMethod != null) {
          clearMethod.Invoke(null, null);
        }
      }
    }
    static void InstallNext() {
      if (_currentIndex < _packages.Length) {
        bool packageFound = false;
        foreach (var package in _listRequest.Result) {
          if (package.name == _packages[_currentIndex]) {
            packageFound = true;
            Debug.Log("[MMFDependencyInstaller] " + package.name + " is already installed");
            _currentIndex++;
            InstallNext();
            return;
          }
        }

        if (!packageFound) {
          Debug.Log("[MMFDependencyInstaller] installing " + _packages[_currentIndex]);
          _addRequest = Client.Add(_packages[_currentIndex]);
          EditorApplication.update += AddProgress;
        }
      } else {
        Debug.Log("[MMFDependencyInstaller] Installation complete");
        Debug.Log("[MMFDependencyInstaller] It's recommended to now close that scene and reopen it before playing it.");
      }
    }
    static void ListProgress() {
      if (_listRequest.IsCompleted) {
        EditorApplication.update -= ListProgress;
        if (_listRequest.Status == StatusCode.Success) {
          InstallNext();
        } else if (_listRequest.Status >= StatusCode.Failure) {
          Debug.Log(_listRequest.Error.message);
        }
      }
    }
    static void AddProgress() {
      if (_addRequest.IsCompleted) {
        if (_addRequest.Status == StatusCode.Success) {
          Debug.Log("[MMFDependencyInstaller] " + _addRequest.Result.packageId + " has been installed");
          _currentIndex++;
          InstallNext();
        } else if (_addRequest.Status >= StatusCode.Failure) {
          Debug.Log(_addRequest.Error.message);
        }

        EditorApplication.update -= AddProgress;
      }
    }
  }
}