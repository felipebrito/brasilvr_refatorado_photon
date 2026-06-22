using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class BuildOculosAndroid
{
    private const string ScenePath = "Assets/Scenes/2.unity";
    private const string OutputPath = "Builds/final_ VR_evento_oculos.apk";

    public static void Build()
    {
        EnsureXRLoaderEnabled();

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string sceneFullPath = Path.Combine(projectRoot, ScenePath);
        string outputFullPath = Path.Combine(projectRoot, OutputPath);

        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = string.Empty;
        PlayerSettings.Android.keyaliasName = string.Empty;
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

        if (!File.Exists(sceneFullPath))
        {
            Debug.LogError($"Scene not found: {ScenePath}");
            EditorApplication.Exit(1);
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputFullPath));

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outputFullPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Android build failed: {report.summary.result}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Android build succeeded: {OutputPath}");
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
                Debug.Log("Oculus Loader assigned successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to assign Oculus Loader: {ex.Message}");
            }

            try
            {
                XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, openXRLoader, BuildTargetGroup.Android);
                Debug.Log("OpenXR Loader assigned successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to assign OpenXR Loader: {ex.Message}");
            }

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

    [MenuItem("Tools/Build Oculus Android")]
    public static void BuildFromMenu()
    {
        Build();
    }
}
