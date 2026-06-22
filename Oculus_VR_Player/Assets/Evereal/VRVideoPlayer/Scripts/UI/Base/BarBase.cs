using UnityEngine;

namespace Evereal.VRVideoPlayer
{
  [RequireComponent(typeof(VRInteractiveItem))]
  public abstract class BarBase : MonoBehaviour
  {
    public Transform startPoint;
    public Transform endPoint;
    public Transform progressPoint;

    protected VideoPlayerCtrl videoPlayerCtrl;
    protected VRInteractiveItem interactiveItem;
    protected Vector3 currentPoint;
    protected float progressBarWidth;

    protected virtual void Awake()
    {
      videoPlayerCtrl = GetComponentInParent<VideoPlayerCtrl>();
      interactiveItem = GetComponent<VRInteractiveItem>();
      if (startPoint != null && endPoint != null)
        progressBarWidth = Vector3.Distance(startPoint.position, endPoint.position);
    }

    protected virtual void OnEnable()
    {
      if (interactiveItem != null)
      {
        interactiveItem.OnOver += OnOver;
        interactiveItem.OnClick += OnClick;
      }
    }

    protected virtual void OnDisable()
    {
      if (interactiveItem != null)
      {
        interactiveItem.OnOver -= OnOver;
        interactiveItem.OnClick -= OnClick;
      }
    }

    public virtual void SetProgress(float progress)
    {
      if (startPoint == null || endPoint == null || progressPoint == null)
        return;

      progress = Mathf.Clamp01(progress);
      progressPoint.position = Vector3.Lerp(startPoint.position, endPoint.position, progress);
    }

    protected virtual void OnOver(Vector3 point)
    {
      currentPoint = point;
    }

    protected abstract void OnClick();
  }
}
