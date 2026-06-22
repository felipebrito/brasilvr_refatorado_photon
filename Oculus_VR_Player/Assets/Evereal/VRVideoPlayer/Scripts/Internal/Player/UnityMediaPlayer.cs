using System;
using UnityEngine;
using UnityEngine.Video;

namespace Evereal.VRVideoPlayer
{
  public class UnityMediaPlayer : IMediaPlayer
  {
    private GameObject targetGameObject;
    private Renderer targetRenderer;
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private bool paused;

    public string url
    {
      get => videoPlayer != null ? videoPlayer.url : string.Empty;
      set
      {
        if (videoPlayer != null)
          videoPlayer.url = value;
      }
    }

    public bool isPlaying => videoPlayer != null && videoPlayer.isPlaying;
    public bool isPrepared => videoPlayer != null && videoPlayer.isPrepared;
    public bool isPaused => paused && !isPlaying;
    public double time
    {
      get => videoPlayer != null ? videoPlayer.time : 0d;
      set
      {
        if (videoPlayer != null)
          videoPlayer.time = value;
      }
    }
    public double length => videoPlayer != null ? videoPlayer.length : 0d;
    public float frameRate => videoPlayer != null ? (float)videoPlayer.frameRate : 0f;
    public int width => videoPlayer != null ? (int)videoPlayer.width : 0;
    public int height => videoPlayer != null ? (int)videoPlayer.height : 0;
    public Texture texture => videoPlayer != null ? videoPlayer.texture : null;

    public event Action<IMediaPlayer> prepareCompleted = delegate { };
    public event Action<IMediaPlayer> started = delegate { };
    public event Action<IMediaPlayer> firstFrameReady = delegate { };
    public event Action<IMediaPlayer> loopPointReached = delegate { };

    public void SetGameObject(GameObject target)
    {
      targetGameObject = target;
    }

    public void SetTargetRenderer(Renderer renderer)
    {
      if (renderer != null)
      {
        targetRenderer = renderer;
        videoPlayer.targetMaterialRenderer = targetRenderer;
      }
    }

    public void SetLooping(bool looping)
    {
      if (videoPlayer != null)
        videoPlayer.isLooping = looping;
    }

    public void Init()
    {
      if (targetGameObject == null)
        return;

      videoPlayer = targetGameObject.GetComponent<VideoPlayer>();
      if (videoPlayer == null)
        videoPlayer = targetGameObject.AddComponent<VideoPlayer>();

      audioSource = targetGameObject.GetComponent<AudioSource>();
      if (audioSource == null)
        audioSource = targetGameObject.AddComponent<AudioSource>();

      videoPlayer.playOnAwake = false;
      videoPlayer.waitForFirstFrame = true;
      videoPlayer.skipOnDrop = true;
      videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
      videoPlayer.EnableAudioTrack(0, true);
      videoPlayer.SetTargetAudioSource(0, audioSource);

      videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
    }

    public void OnEnable()
    {
      if (videoPlayer == null)
        return;

      videoPlayer.prepareCompleted += HandlePrepareCompleted;
      videoPlayer.started += HandleStarted;
      videoPlayer.frameReady += HandleFrameReady;
      videoPlayer.loopPointReached += HandleLoopPointReached;
      videoPlayer.sendFrameReadyEvents = true;
    }

    public void OnDisable()
    {
      if (videoPlayer == null)
        return;

      videoPlayer.prepareCompleted -= HandlePrepareCompleted;
      videoPlayer.started -= HandleStarted;
      videoPlayer.frameReady -= HandleFrameReady;
      videoPlayer.loopPointReached -= HandleLoopPointReached;
    }

    public void Load(string sourceUrl, bool play)
    {
      if (videoPlayer == null)
        return;

      paused = false;
      videoPlayer.url = sourceUrl;
      videoPlayer.Prepare();
      if (play)
        videoPlayer.Play();
    }

    public void Play()
    {
      if (videoPlayer == null)
        return;

      paused = false;
      videoPlayer.Play();
    }

    public void Pause()
    {
      if (videoPlayer == null)
        return;

      paused = true;
      videoPlayer.Pause();
    }

    public void Stop()
    {
      if (videoPlayer == null)
        return;

      paused = false;
      videoPlayer.Stop();
    }

    public bool IsAudioMute(ushort track)
    {
      return audioSource != null && audioSource.mute;
    }

    public void SetAudioMute(ushort track, bool mute)
    {
      if (audioSource != null)
        audioSource.mute = mute;
    }

    public float GetAudioVolume(ushort track)
    {
      return audioSource != null ? audioSource.volume : 0f;
    }

    public void SetAudioVolume(ushort track, float volume)
    {
      if (audioSource != null)
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private RenderTexture renderTexture;

    private void HandlePrepareCompleted(VideoPlayer source)
    {
      prepareCompleted(this);
    }

    private void HandleStarted(VideoPlayer source)
    {
      started(this);
    }

    private void HandleFrameReady(VideoPlayer source, long frameIdx)
    {
      firstFrameReady(this);
      source.sendFrameReadyEvents = false;
    }

    private void HandleLoopPointReached(VideoPlayer source)
    {
      loopPointReached(this);
    }
  }
}
