using System;
using System.IO;
using Bootstrap;
using GameScreen.Presentation;
using IdleEmpireBuilder.Application;
using SplashScreen.Presentation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    /// <summary>One-shot Addressables + UI screen prefabs for the IdleEmpireBuilder book companion.</summary>
    public static class IdleEmpireBuilderProjectTools
    {
        const string AddressablesDir = "Assets/_Project/Addressables/IdleEmpireBuilder";
        const string TuningPath = AddressablesDir + "/IdleEmpireBuilderTuning.asset";
        const string PrefabDir = "Assets/_Project/Prefabs/UI";
        const string GamePrefabPath = PrefabDir + "/Screen_Game.prefab";
        const string SplashPrefabPath = PrefabDir + "/Screen_Splash.prefab";
        const string BootstrapScenePath = "Assets/_Project/Scenes/BootstrapScene.unity";

        [MenuItem("IdleEmpireBuilder/Project/Bootstrap Content (Scene + Prefabs + Addressables)")]
        public static void MenuBootstrap() => BootstrapAll(true);

        public static void BatchBootstrap()
        {
            try
            {
                BootstrapAll(true);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        static void BootstrapAll(bool buildAddressablesPlayerContent)
        {
            EnsureFolder("Assets/_Project/Addressables");
            EnsureFolder(AddressablesDir);
            EnsureFolder(PrefabDir);
            CreateOrUpdateTuningAsset();
            CreateScreenPrefabIfMissing(GamePrefabPath, "Screen_Game", typeof(GameScreenController));
            CreateScreenPrefabIfMissing(SplashPrefabPath, "Screen_Splash", typeof(SplashScreenController));
            EnsureAppEntryPointInBootstrapScene();
            RegisterAddressablesEntries();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (buildAddressablesPlayerContent)
                AddressableAssetSettings.BuildPlayerContent();

            Debug.Log("[IdleEmpireBuilder] Project bootstrap finished.");
        }

        static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;
            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var name = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        static void CreateOrUpdateTuningAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<IdleEmpireTuningConfig>(TuningPath) != null)
                return;

            var tuning = ScriptableObject.CreateInstance<IdleEmpireTuningConfig>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            EditorUtility.SetDirty(tuning);
        }

        static void CreateScreenPrefabIfMissing(string prefabPath, string rootName, Type controllerType)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                return;

            var go = new GameObject(rootName, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            go.AddComponent(controllerType);

            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath) ?? PrefabDir);
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
        }

        static void EnsureAppEntryPointInBootstrapScene()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject entry = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<AppEntryPoint>() != null)
                    entry = root;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root != entry)
                    UnityEngine.Object.DestroyImmediate(root);
            }

            if (entry == null || entry.transform is RectTransform)
            {
                if (entry != null)
                    UnityEngine.Object.DestroyImmediate(entry);

                entry = new GameObject("AppEntryPoint");
                entry.AddComponent<AppEntryPoint>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void RegisterAddressablesEntries()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[IdleEmpireBuilder] AddressableAssetSettings could not be created.");
                return;
            }

            var localGroup = settings.DefaultGroup;
            if (localGroup == null)
            {
                Debug.LogError("[IdleEmpireBuilder] Addressables DefaultGroup is null.");
                return;
            }

            var uiGroup = settings.FindGroup("UI_Screens");
            if (uiGroup == null)
                uiGroup = settings.CreateGroup("UI_Screens", false, false, true, null, typeof(BundledAssetGroupSchema));

            void Ensure(string path, string address, AddressableAssetGroup group)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"[IdleEmpireBuilder] Missing asset for Addressables: {path}");
                    return;
                }

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.SetAddress(address, false);
            }

            Ensure(TuningPath, IdleEmpireAddressKeys.Config, localGroup);
            Ensure(GamePrefabPath, "Screen_Game", uiGroup);
            Ensure(SplashPrefabPath, "Screen_Splash", uiGroup);

            EditorUtility.SetDirty(settings);
        }
    }
}
