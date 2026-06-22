using UnityEngine;

public class CameraSphereController : MonoBehaviour
{
    public Transform cameraTransform; // Referência à câmera
    public Transform sphereTransform; // Referência à esfera
    public bool limitView = true; // Controla se o limite está ativo

    public float maxAngle = 90f; // Ângulo máximo para cada lado a partir do centro
    public float recenterSpeed = 2f; // Velocidade de recentralização da esfera

    private float lastSphereYaw = 0f; // Armazena a última rotação Y da esfera

    void Update()
    {
        if (limitView)
        {
            CheckAndRecenterSphere();
        }
    }

    private void CheckAndRecenterSphere()
    {
        // Obtém o ângulo Y relativo entre a câmera e a esfera
        float cameraYaw = cameraTransform.eulerAngles.y;
        float sphereYaw = sphereTransform.eulerAngles.y;

        // Normaliza os ângulos para a faixa de -180 a 180 graus
        float relativeYaw = Mathf.DeltaAngle(sphereYaw, cameraYaw);

        // Se a rotação relativa exceder o limite, recentraliza a esfera
        if (Mathf.Abs(relativeYaw) > maxAngle )
        {
            // Calcula a direção e magnitude da rotação necessária
            float recenterAngle = Mathf.Lerp(0f, relativeYaw - Mathf.Sign(relativeYaw) * maxAngle, Time.deltaTime * recenterSpeed);

            // Aplica a rotação suave na esfera
            sphereTransform.Rotate(0f, recenterAngle, 0f, Space.World);
        }
    }
}
