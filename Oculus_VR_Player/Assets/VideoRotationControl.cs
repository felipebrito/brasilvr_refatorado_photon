using System.Collections.Generic;
using UnityEngine;

public class VideoRotationControl : MonoBehaviour
{
    public CameraRotationLimiter cameraLimiter;
    public Evereal.VRVideoPlayer.VRVideoPlayer videoPlayer; // Reference to the video player

    public string currentVideoTitleID;

    [System.Serializable]
    public class VideoBlock
    {
        public string videoTitle;
        public List<BlockTime> blockTimes;
        public float angle = 75f;
    }

    [System.Serializable]
    public class BlockTime
    {
        public double startTime;
        public double endTime;
    }

    public List<VideoBlock> videoBlocks = new List<VideoBlock>();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (cameraLimiter != null && cameraLimiter.IsLimitActive)
        {
            float rotationY = cameraLimiter.transform.localEulerAngles.y;
            Debug.Log($"Current Y Rotation: {rotationY}");
        }
    }
#endif


    void Update()
    {
        if (videoPlayer == null || cameraLimiter == null || !videoPlayer.isPlaying) return;

        string currentVideoTitle = videoPlayer.GetFileName();
        double currentTime = videoPlayer.time;
        currentVideoTitleID = currentVideoTitle;
        
        if (videoBlocks != null)
        {
            foreach (var videoBlock in videoBlocks)
            {
                if (videoBlock != null && videoBlock.videoTitle == currentVideoTitle && videoBlock.blockTimes != null)
                {
                    foreach (var blockTime in videoBlock.blockTimes)
                    {
                        if (blockTime != null && currentTime >= blockTime.startTime && currentTime <= blockTime.endTime)
                        {
                            cameraLimiter.angle = videoBlock.angle;
                            cameraLimiter.IsLimitActive = true;
                            return;
                        }
                    }
                }
            }
        }

        // If no active block is found, disable the limitation
        cameraLimiter.IsLimitActive = false;
    }
}
