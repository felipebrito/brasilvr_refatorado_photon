using UnityEngine;
public class ButtonProxy : MonoBehaviour {
    public SimpleController controller;
    public int slotIndex;
    public string videoUrl;
    public void OnClick() {
        controller.PlayVideo(slotIndex, videoUrl);
    }
}
