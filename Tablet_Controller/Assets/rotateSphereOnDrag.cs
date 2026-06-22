using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RotateSphereOnDrag : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform sphere; // Referência à esfera na cena
    [SerializeField] private RawImage renderTextureImage; // Referência à RawImage
    [SerializeField] private float rotationSpeed = 5f; // Velocidade da rotação

    public void OnDrag(PointerEventData eventData)
    {
        if (IsPointerOverRawImage(eventData))
        {
            float rotateX = eventData.delta.y * rotationSpeed * Time.deltaTime;
            float rotateY = -eventData.delta.x * rotationSpeed * Time.deltaTime;

            sphere.Rotate(Vector3.up, rotateY, Space.World);
            sphere.Rotate(Vector3.right, rotateX, Space.World);
        }
    }

    private bool IsPointerOverRawImage(PointerEventData eventData)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            renderTextureImage.rectTransform, eventData.position, null);
    }
}
