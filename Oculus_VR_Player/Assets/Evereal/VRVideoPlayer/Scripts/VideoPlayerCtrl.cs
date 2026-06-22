/* Copyright (c) 2017-present Evereal. All rights reserved. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Evereal.VRVideoPlayer
{
  [RequireComponent(typeof(VRVideoPlayer))]
  public class VideoPlayerCtrl : MonoBehaviour
  {
    #region Properties

    // Video playlist
    public List<string> playlist = new List<string>();
    public VideoTitle videoTitle;
    public PlayButton playButton;
    public VideoTime currentTime;
    public VideoTime totalTime;
    public ProgressBar progressBar;
    public VolumeButton volumeButton;
    public VolumeBar volumeBar;
    public RenderModeButton normalButton;
    public RenderModeButton _180Button;
    public RenderModeButton _360Button;
    public StereoModeButton monoButton;
    public StereoModeButton leftRightButton;
    public StereoModeButton topBottomButton;
    public Fade fade;

    private VRVideoPlayer vrVideoPlayer;
    public bool isVideoJumping { get; private set; }

    public bool isVideoPlaying
    {
      get
      {
        return vrVideoPlayer.isPlaying;
      }
    }

    public bool isVideoPaused
    {
      get
      {
        return vrVideoPlayer.isPaused;
      }
    }

    public bool isAudioMute
    {
      get
      {
        return vrVideoPlayer.IsAudioMute(0);
      }
    }

    public double videoTime
    {
      get
      {
        return vrVideoPlayer.time;
      }
      set
      {
        vrVideoPlayer.time = value;
      }
    }

    public double videoLength
    {
      get
      {
        return vrVideoPlayer.length;
      }
    }

    // Get current video index
    private int videoIndex = 0;

    // Log message format template
    private const string LOG_FORMAT = "[VideoPlayerCtrl] {0}";

    #endregion

    #region Events

    private void OnStarted(VRVideoPlayer player)
    {
      isVideoJumping = false;
      // set video title
      if (videoTitle != null) videoTitle.SetText(player.GetFileName());
      // set total time
      if (totalTime != null) totalTime.SetTime(player.length);
      // toggle play button
      if (playButton != null) playButton.Toggle();
      // toggle render button
      SwitchVideoRendererButton(player.renderMode);
      // toggle volume button
      if (volumeButton != null) volumeButton.Toggle();
      // set volume bar
      if (volumeBar != null) volumeBar.SetProgress(vrVideoPlayer.GetAudioVolume(0));
    }

    private void OnLoopPointReached(VRVideoPlayer player)
    {
      // toggle play button
      if (playButton != null) playButton.Toggle();
    }

    #endregion

    #region Video Player Ctrl

    public string GetVideo()
    {
      if (playlist.Count == 0 || videoIndex >= playlist.Count)
        return null;

      string nextVideo = playlist[videoIndex];

      return nextVideo;
    }

    public void RestartVideo()
    {
      if (isVideoPlaying)
      {
        vrVideoPlayer.Stop();
      }

      string videoUrl = GetVideo();
      vrVideoPlayer.Load(videoUrl, true);
    }

    public void NextVideo()
    {
      if (playlist.Count == 0 || videoIndex >= playlist.Count)
        return;

      videoIndex += 1;
      videoIndex %= playlist.Count;
    }

    public void PrevVideo()
    {
      if (playlist.Count == 0 || videoIndex < 0)
        return;

      videoIndex -= 1;
      if (videoIndex < 0)
        videoIndex = playlist.Count - 1;
    }

    public void PlayNextVideo()
    {
      // move to next video index
      NextVideo();
      if (fade)
      {
        StartCoroutine(fade.StartFadeOut());
      }
      else
      {
        if (isVideoPlaying)
          vrVideoPlayer.Stop();
        RestartVideo();
      }
    }

    public void PlayPrevVideo()
    {
      // move to prev video index
      PrevVideo();
      if (fade)
      {
        StartCoroutine(fade.StartFadeOut());
      }
      else
      {
        if (isVideoPlaying)
          vrVideoPlayer.Stop();
        RestartVideo();
      }
    }

    public void PauseVideo()
    {
      vrVideoPlayer.Pause();
      playButton.Toggle();
    }

    public void PlayVideo()
    {
      vrVideoPlayer.Play();
      playButton.Toggle();
    }

    public void ReplayVideo()
    {
      vrVideoPlayer.Replay();
      playButton.Toggle();
    }

    public void ToggleVideoPlay()
    {
      if (isVideoPlaying)
      {
        PauseVideo();
      }
      else
      {
        PlayVideo();
      }
    }

    public void FastForward(double step = 5)
    {
      videoTime += step;
    }

    public void Rewind(double step = 5)
    {
      videoTime -= step;
    }

    public void ToggleAudioMute()
    {
      vrVideoPlayer.SetAudioMute(0, !isAudioMute);
      volumeButton.Toggle();
    }

    public void SetAudioVolume(float volume)
    {
      vrVideoPlayer.SetAudioVolume(0, volume);
      volumeBar.SetProgress(volume);
    }

    public void SetVideoRenderMode(RenderMode mode, ProjectionType proj = ProjectionType.NONE)
    {
      // TODO, support 360 equirectangular & cubemap
      if (mode == RenderMode._360)
      {
        proj = ProjectionType.EQUIRECT;
      }
      vrVideoPlayer.SetRenderMode(mode, proj);
      SwitchVideoRendererButton(mode);
    }

    public void SetVideoStereoMode(StereoMode mode)
    {
      vrVideoPlayer.SetStereoMode(mode);
      SwitchVideoStereoButton(mode);
    }

    #endregion

    #region Internal

    private void RestartPlayAndFadeIn()
    {
      StartCoroutine(RestartPlayAndDelayFadeIn());
    }

    private IEnumerator RestartPlayAndDelayFadeIn()
    {
      if (isVideoPlaying)
        vrVideoPlayer.Stop();
      RestartVideo();
      yield return new WaitForSeconds(0.3f);
      if (fade)
        StartCoroutine(fade.StartFadeIn());
    }

    private void SwitchVideoRendererButton(RenderMode mode)
    {
      switch (mode)
      {
        case RenderMode.NORMAL:
          if (normalButton != null) normalButton.SetEnable();
          if (_180Button != null) _180Button.SetDisable();
          if (_360Button != null) _360Button.SetDisable();
          break;
        case RenderMode._180:
          if (normalButton != null) normalButton.SetDisable();
          if (_180Button != null) _180Button.SetEnable();
          if (_360Button != null) _360Button.SetDisable();
          break;
        case RenderMode._360:
          if (normalButton != null) normalButton.SetDisable();
          if (_180Button != null) _180Button.SetDisable();
          if (_360Button != null) _360Button.SetEnable();
          break;
      }
    }

    private void SwitchVideoStereoButton(StereoMode mode)
    {
      switch (mode)
      {
        case StereoMode.NONE:
          if (monoButton != null) monoButton.SetEnable();
          if (leftRightButton != null) leftRightButton.SetDisable();
          if (topBottomButton != null) topBottomButton.SetDisable();
          break;
        case StereoMode.LEFT_RIGHT:
          if (monoButton != null) monoButton.SetDisable();
          if (leftRightButton != null) leftRightButton.SetEnable();
          if (topBottomButton != null) topBottomButton.SetDisable();
          break;
        case StereoMode.TOP_BOTTOM:
          if (monoButton != null) monoButton.SetDisable();
          if (leftRightButton != null) leftRightButton.SetDisable();
          if (topBottomButton != null) topBottomButton.SetEnable();
          break;
      }
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      vrVideoPlayer = GetComponent<VRVideoPlayer>();

      if (videoTitle == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "VideoTitle not attached!");
      }
      if (playButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "PlayButton not attached!");
      }
      if (currentTime == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "CurrentTime not attached!");
      }
      if (totalTime == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "TotalTime not attached!");
      }
      if (volumeButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "VolumeButton not attached!");
      }
      if (volumeBar == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "VolumeBar not attached!");
      }
      if (normalButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "NormalButton not attached!");
      }
      if (_180Button == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "180Button not attached!");
      }
      if (_360Button == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "360Button not attached!");
      }
      if (monoButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "MonoButton not attached!");
      }
      if (leftRightButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "LeftRightButton not attached!");
      }
      if (topBottomButton == null)
      {
        Debug.LogErrorFormat(LOG_FORMAT, "TopBottomButton not attached!");
      }
    }

    private void Start()
    {
      if (vrVideoPlayer.autoPlay)
      {
        RestartVideo();
      }
    }

    private void Update()
    {
      if (isVideoPlaying)
      {
        if (currentTime != null) currentTime.SetTime(vrVideoPlayer.time);
        if (progressBar != null) progressBar.SetProgress((float)(vrVideoPlayer.time / vrVideoPlayer.length));
      }
    }

    private void OnEnable()
    {
      vrVideoPlayer.started += OnStarted;
      vrVideoPlayer.loopPointReached += OnLoopPointReached;
      if (fade)
        fade.fadeOutCompleted += RestartPlayAndFadeIn;
    }

    private void OnDisable()
    {
      vrVideoPlayer.started -= OnStarted;
      vrVideoPlayer.loopPointReached -= OnLoopPointReached;
      if (fade)
        fade.fadeOutCompleted -= RestartPlayAndFadeIn;
    }

    #endregion
  }
}