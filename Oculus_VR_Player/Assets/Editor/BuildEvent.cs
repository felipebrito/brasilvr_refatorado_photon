using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class BuildEvent
{
    public static void BuildVR3()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = string.Empty;
        PlayerSettings.Android.keyaliasName = string.Empty;
        
        // Exact replica of the settings from the corrupted ProjectSettings that worked!
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false); 
        
        // Also clear scripting define symbols just like the corrupted file did, 
        // in case Photon or Mirror had conditional code that was crashing IL2CPP/ARM64!
        // Actually it's safer to keep the symbols but just build ARMv7 Mono.
        
        EnsureSingleXRLoader();

        string codePath = "Assets/Scripts/UserStatusSend.cs";
        string originalCode = File.ReadAllText(codePath);

        string pkgName = "com.Vortex.BrasilVR3";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, pkgName);

        string outputFullPath = Path.Combine(projectRoot, "Builds/BrasilVR3.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFullPath));

        string newCode = System.Text.RegularExpressions.Regex.Replace(originalCode, @"int slotIndex = \d+; // FORCANDO PARA PLAYER \d+ \(Slot \d+\)", "int slotIndex = 2; // FORCANDO PARA PLAYER 3 (Slot 2)");
        File.WriteAllText(codePath, newCode);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/2.unity" },
            locationPathName = outputFullPath,
            target = BuildTarget.Android,
            options = BuildOptions.None // Removing Development build just in case
        };

        Debug.Log($"Building {pkgName}...");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        File.WriteAllText(codePath, originalCode); // restore
        
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Android build failed for {pkgName}: {report.summary.result}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Android build succeeded!");
        EditorApplication.Exit(0);
    }

    private static void EnsureSingleXRLoader()
    {
        XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
        if (buildTargetSettings != null)
        {
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings != null && settings.AssignedSettings != null)
            {
                string oculusLoader = "Unity.XR.Oculus.OculusLoader";
                string openXRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";
                try { XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, oculusLoader, BuildTargetGroup.Android); } catch {}
                try { XRPackageMetadataStore.RemoveLoader(settings.AssignedSettings, openXRLoader, BuildTargetGroup.Android); } catch {}
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(settings.AssignedSettings);
            }
        }
    }
}
