using UnityEngine;

public class ButtonProxy : MonoBehaviour 
{
    public enum ActionType { PlayVideo, TogglePlayPause, StopVideo }
    
    public SimpleController controller;
    public int slotIndex;
    public string videoUrl;
    public ActionType action = ActionType.PlayVideo;

    public void OnClick() 
    {
        if (controller == null) return;

        switch (action)
        {
            case ActionType.PlayVideo:
                controller.PlayVideo(slotIndex, videoUrl);
                break;
            case ActionType.TogglePlayPause:
                controller.TogglePlayPause(slotIndex);
                break;
            case ActionType.StopVideo:
                controller.StopVideo(slotIndex);
                break;
        }
    }
}
