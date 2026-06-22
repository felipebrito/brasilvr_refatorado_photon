using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class VideoFile
{
    public string videoName;
    public string videoUrl;
    public Sprite videoIcon;

    public void UpdateUI(Image icon, TextMeshProUGUI name)
    {
        if (videoIcon != null)
        {
            icon.sprite = videoIcon;
        }

        if (!string.IsNullOrEmpty(videoName))
        {
            name.text = videoName;
        }
    }
}
