using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Proto.Promises;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using Venly.Editor.Utils;
using Venly.Models;

namespace Venly.Editor.Tools.SDKManager
{
    public class SDKManager
    {
        #region Singleton

        private static SDKManager _instance;

        public static SDKManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SDKManager();
                }

                return _instance;
            }
        }

        #endregion

        #region GIT Helpers
        private class GitReleaseInfo
        {
            public string name;
        }
        #endregion

        private VenlyEditorDataSO.SDKManagerData _managerData => VenlySettingsEd.Instance.EditorData.SDKManager;

        #region MenuItem

        [MenuItem("Window/Venly/SDK Manager")]
        public static void ShowSdkManager()
        {
            var types = new List<Type>()
            { 
                // first add your preferences
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow"),
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow")
            };

            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>(types.ToArray());
            wnd.titleContent = new GUIContent("Venly SDK Manager");
        }

        [MenuItem("Window/Venly/Force Close Manager")]
        public static void ForceCloseManager()
        {
            SDKManagerView wnd = EditorWindow.GetWindow<SDKManagerView>();
            if (wnd != null)
            {
                wnd.Close();
            }
        }

        #endregion

        #region MANAGER FUNCTIONS
        public void ConfigureForBackend(eVyBackendProvider backend)
        {
            //Set Defines
            var buildTarget = NamedBuildTarget.Standalone;

            PlayerSettings.GetScriptingDefineSymbols(buildTarget, out var currentDefines);

            //Clear Current Venly Defines
            var definesList = currentDefines.ToList();
            definesList.RemoveAll(define => define.Contains("_VENLY_"));

            //Populate with required Defines
            if (backend == eVyBackendProvider.PlayFab) definesList.Add("ENABLE_VENLY_PLAYFAB");

            PlayerSettings.SetScriptingDefineSymbols(buildTarget, definesList.ToArray());

            //SET BACKEND
            VenlySettingsEd.Instance.Settings.BackendProvider = backend;
        }

        public Promise<string> GetLatestVersion()
        {
            var deferredPromise = Promise<string>.NewDeferred();

            UnityWebRequest request = UnityWebRequest.Get(_managerData.GitReleaseURL);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += (op) =>
            {
                if (request.isDone && request.result == UnityWebRequest.Result.Success)
                {
                    var gitInfos = JsonConvert.DeserializeObject<GitReleaseInfo[]>(request.downloadHandler.text);
                    var latestVersion = VenlyEditorUtils.GetLatestSemVer(gitInfos?.Select(gi => gi.name).ToList());

                    if(string.IsNullOrEmpty(latestVersion)) deferredPromise.Reject("Latest version not found");
                    else deferredPromise.Resolve(latestVersion);
                }
                else
                {
                    Debug.LogWarning("[Venly SDK] Failed to retrieve SDK release list.");
                    deferredPromise.Reject("Failed to retrieve SDK release list");
                }
            };

            return deferredPromise.Promise;
        }

        public void UpdateSDK(string targetVersion)
        {
            VenlySDKUpdater.Instance.UpdateSDK(targetVersion);

            //Prepare for Update
            VenlyEditorUtils.StoreBackup(VenlySettingsEd.Instance.Settings);
            VenlyEditorUtils.StoreBackup(VenlySettingsEd.Instance.EditorData);

            //Update Link
            var packages = new List<string>();

            //ProtoPromise
            packages.Add(@"git+https://github.com/TimCassell/ProtoPromise.git?path=ProtoPromise_Unity/Assets/Plugins/ProtoPromise#v2.3.0");

            //Venly SDK
            packages.Add($"{VenlySettingsEd.Instance.EditorData.SDKManager.GitSdkURL}#{targetVersion}");

            Client.AddAndRemove(packages.ToArray(), null);
        }
        #endregion
    }
}