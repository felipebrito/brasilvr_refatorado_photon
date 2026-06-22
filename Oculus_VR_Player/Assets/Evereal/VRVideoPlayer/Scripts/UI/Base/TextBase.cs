using UnityEngine;

namespace Evereal.VRVideoPlayer
{
  public class TextBase : MonoBehaviour
  {
    public TextMesh textMesh;

    protected virtual void Awake()
    {
      if (textMesh == null)
        textMesh = GetComponent<TextMesh>();
    }

    public virtual void SetText(string value)
    {
      if (textMesh != null)
        textMesh.text = value;
    }
  }
}
