using UnityEngine;

public class CameraRotationLimiter : MonoBehaviour
{
    public bool IsLimitActive = false;
    public float resetSpeed = 2f; // Speed to reset the sphere to face the camera.
    public Transform sphereTransform; // Reference to the sphere containing the camera.
    public float angle = 75f;

    private float currentRotationY;
    private Quaternion initialRotation;
    public Evereal.VRVideoPlayer.VRVideoPlayer videoPlayer;

    void Start()
    {
        if (sphereTransform == null)
        {
            Debug.LogError("Sphere Transform is not assigned!");
        }

        initialRotation = transform.rotation;
    }

    void Update()
    {
#if UNITY_EDITOR || !UNITY_ANDROID || !PLATFORM_ANDROID
        // Allow rotation control with mouse in the editor
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(0, mouseX * 2f, 0);
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            videoPlayer.Load("Videos/Fernando de Noronha.mp4", true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            videoPlayer.Load("Videos/Lencois Maranheses.mp4", true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            videoPlayer.Load("Videos/Pantanal.mp4", true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            videoPlayer.Load("Videos/Rio de Janeiro.mp4", true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            videoPlayer.Load("Videos/Amazonia.mp4", true);
        }

#endif

        if (IsLimitActive && sphereTransform != null)
        {
            // Get current local Y rotation
            currentRotationY = transform.localEulerAngles.y;

            if (currentRotationY > 180) // Normalize angle for comparison
                currentRotationY -= 360;

            // Check if rotation exceeds the limit
            if (Mathf.Abs(currentRotationY) > angle)
            {
                // Maintain fixed rotation for X and Z
                Quaternion targetRotation = Quaternion.Euler(-90, transform.localEulerAngles.y, -180);

                // Rotate the sphere to recentralize in front of the camera
                sphereTransform.rotation = Quaternion.Lerp(sphereTransform.rotation, targetRotation, Time.deltaTime * resetSpeed);
            }
        }
    }
}
