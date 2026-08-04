using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class BuildAllAPKs
{
    private const string ScenePath = "Assets/Scenes/2.unity";
    
    public static void Build()
    {
        EnsureXRLoaderEnabled();

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string sceneFullPath = Path.Combine(projectRoot, ScenePath);

        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = string.Empty;
        PlayerSettings.Android.keyaliasName = string.Empty;
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan });

        if (!File.Exists(sceneFullPath))
        {
            Debug.LogError($"Scene not found: {ScenePath}");
            EditorApplication.Exit(1);
            return;
        }

        string codePath = "Assets/Scripts/UserStatusSend.cs";
        string originalCode = File.ReadAllText(codePath);

        for (int i = 3; i < 4; i++)
        {
            string outputFullPath = Path.Combine(projectRoot, $"Builds/OVR_Player_{i+1}.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFullPath));

            // Replace slotIndex
            string newCode = System.Text.RegularExpressions.Regex.Replace(originalCode, @"int slotIndex = \d+; // FORCANDO PARA PLAYER \d+ \(Slot \d+\)", $"int slotIndex = {i}; // FORCANDO PARA PLAYER {i+1} (Slot {i})");
            File.WriteAllText(codePath, newCode);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputFullPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log($"Building Player {i+1}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Android build failed for Player {i+1}: {report.summary.result}");
                File.WriteAllText(codePath, originalCode); // restore
                EditorApplication.Exit(1);
                return;
            }
        }
        
        File.WriteAllText(codePath, originalCode); // restore
        Debug.Log($"All 4 Android builds succeeded!");
        EditorApplication.Exit(0);
    }

    private static void EnsureXRLoaderEnabled()
    {
        try
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
            {
                buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(buildTargetSettings, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
            }

            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<XRGeneralSettings>("Assets/XR/XRGeneralSettingsAndroid.asset");
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
                    AssetDatabase.CreateAsset(settings, "Assets/XR/XRGeneralSettingsAndroid.asset");
                }
                buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Android, settings);
            }

            if (settings.AssignedSettings == null)
            {
                var manager = AssetDatabase.LoadAssetAtPath<XRManagerSettings>("Assets/XR/XRManagerSettingsAndroid.asset");
                if (manager == null)
                {
                    manager = ScriptableObject.CreateInstance<XRManagerSettings>();
                    AssetDatabase.CreateAsset(manager, "Assets/XR/XRManagerSettingsAndroid.asset");
                }
                settings.AssignedSettings = manager;
            }

            settings.InitManagerOnStart = true;

            string oculusLoader = "Unity.XR.Oculus.OculusLoader";
            string openXRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";

            try
            {
                XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, oculusLoader, BuildTargetGroup.Android);
            }
            catch { }

            try
            {
                XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, openXRLoader, BuildTargetGroup.Android);
            }
            catch { }

            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(settings.AssignedSettings);
            EditorUtility.SetDirty(buildTargetSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in EnsureXRLoaderEnabled: {ex}");
        }
    }
}
