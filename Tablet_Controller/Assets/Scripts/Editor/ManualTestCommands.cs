using UnityEngine;
using UnityEditor;
using Photon.Pun;

public class ManualTestCommands : MonoBehaviour
{
    [MenuItem("BrasilVR/Manual Test/1. Send Pantanal to Oculus 3 (Slot 2)")]
    public static void SendVideoToOculus3()
    {
        if (PhotonNetwork.InRoom)
        {
            var controller = FindObjectOfType<ControllerScript>();
            if (controller != null)
            {
                controller.SelectPlayer(3); // Select Oculus 3
                controller.SendSelectVideoCommand("Videos/Pantanal.mp4");
                Debug.Log("Manual Command: Sent Pantanal to Oculus 3");
            }
            else
            {
                Debug.LogError("ControllerScript not found in scene!");
            }
        }
        else
        {
            Debug.LogError("Not in a Photon Room! Start the Tablet app first.");
        }
    }

    [MenuItem("BrasilVR/Manual Test/2. Send Play to Oculus 3")]
    public static void SendPlayToOculus3()
    {
        if (PhotonNetwork.InRoom)
        {
            var controller = FindObjectOfType<ControllerScript>();
            if (controller != null)
            {
                controller.SelectPlayer(3);
                controller.SendPlayCommand();
                Debug.Log("Manual Command: Sent PLAY to Oculus 3");
            }
        }
    }
    
    [MenuItem("BrasilVR/Manual Test/3. Send Pause to Oculus 3")]
    public static void SendPauseToOculus3()
    {
        if (PhotonNetwork.InRoom)
        {
            var controller = FindObjectOfType<ControllerScript>();
            if (controller != null)
            {
                controller.SelectPlayer(3);
                controller.SendPauseCommand();
                Debug.Log("Manual Command: Sent PAUSE to Oculus 3");
            }
        }
    }
}
