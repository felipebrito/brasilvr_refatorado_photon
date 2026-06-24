using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SetVulkan
{
    public static void Apply()
    {
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        Debug.Log("Graphics API set to Vulkan for Android.");
    }
}
