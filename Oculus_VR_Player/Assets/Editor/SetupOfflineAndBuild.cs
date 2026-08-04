using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupOfflineAndBuild
{
    public static void Perform()
    {
        string scenePath = "Assets/Scenes/2.unity";
        Debug.Log("Opening scene: " + scenePath);
        Scene scene = EditorSceneManager.OpenScene(scenePath);

        // Find UserStatusSend
        UserStatusSend onlineScript = Object.FindObjectOfType<UserStatusSend>();
        if (onlineScript != null)
        {
            Debug.Log("Found UserStatusSend on " + onlineScript.gameObject.name);
            GameObject go = onlineScript.gameObject;

            // Check if it already has the offline script
            UserStatusSendOffline offlineScript = go.GetComponent<UserStatusSendOffline>();
            if (offlineScript == null)
            {
                offlineScript = go.AddComponent<UserStatusSendOffline>();
                Debug.Log("Added UserStatusSendOffline.");
            }

            // Copy references
            offlineScript.vrVideoPlayer = onlineScript.vrVideoPlayer;
            offlineScript.videoPlayerCtrl = onlineScript.videoPlayerCtrl;
            offlineScript.Mensagem = onlineScript.Mensagem;
            offlineScript.aviso = onlineScript.aviso;
            offlineScript.ambiente = onlineScript.ambiente;
            offlineScript.sphere = onlineScript.sphere;

            // Mark scene as dirty and save
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Scene saved successfully with new offline component.");
        }
        else
        {
            Debug.LogError("Could not find UserStatusSend in the scene!");
        }

        // Call the existing build process
        Debug.Log("Starting APK Build...");
        BuildOculosAndroid.Build();
    }
}
