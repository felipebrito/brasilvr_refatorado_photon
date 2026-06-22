using UnityEngine;

namespace Evereal.VRVideoPlayer
{
  [RequireComponent(typeof(VRInteractiveItem))]
  public abstract class ButtonBase : MonoBehaviour
  {
    protected VideoPlayerCtrl videoPlayerCtrl;
    protected VRInteractiveItem interactiveItem;
    protected const string LOG_FORMAT = "[ButtonBase] {0}";

    protected virtual void Awake()
    {
      videoPlayerCtrl = GetComponentInParent<VideoPlayerCtrl>();
      interactiveItem = GetComponent<VRInteractiveItem>();
    }

    protected virtual void OnEnable()
    {
      if (interactiveItem != null)
        interactiveItem.OnClick += OnClick;
    }

    protected virtual void OnDisable()
    {
      if (interactiveItem != null)
        interactiveItem.OnClick -= OnClick;
    }

    protected abstract void OnClick();
  }
}
