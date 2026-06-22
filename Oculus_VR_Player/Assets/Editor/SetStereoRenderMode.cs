using UnityEditor;
using UnityEngine;

public class SetStereoRenderMode {
    public static void SetMode() {
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
        Debug.Log("Set StereoRenderingPath to Instancing");
    }
}
