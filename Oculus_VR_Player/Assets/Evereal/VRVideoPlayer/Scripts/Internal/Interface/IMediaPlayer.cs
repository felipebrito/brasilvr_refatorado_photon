using System;
using UnityEngine;

namespace Evereal.VRVideoPlayer
{
  public interface IMediaPlayer
  {
    string url { get; set; }
    bool isPlaying { get; }
    bool isPrepared { get; }
    bool isPaused { get; }
    double time { get; set; }
    double length { get; }
    float frameRate { get; }
    int width { get; }
    int height { get; }
    Texture texture { get; }

    event Action<IMediaPlayer> prepareCompleted;
    event Action<IMediaPlayer> started;
    event Action<IMediaPlayer> firstFrameReady;
    event Action<IMediaPlayer> loopPointReached;

    void SetGameObject(GameObject target);
    void SetTargetRenderer(Renderer targetRenderer);
    void SetLooping(bool looping);
    void Init();
    void OnEnable();
    void OnDisable();
    void Load(string sourceUrl, bool play);
    void Play();
    void Pause();
    void Stop();
    bool IsAudioMute(ushort track);
    void SetAudioMute(ushort track, bool mute);
    float GetAudioVolume(ushort track);
    void SetAudioVolume(ushort track, float volume);
  }
}
