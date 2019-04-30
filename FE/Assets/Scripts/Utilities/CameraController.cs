using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private new Camera camera;

    public static bool IsDraggingCamera { get; private set; }
    private Vector2 lastMousePosition;

    public float desiredHeight = 5;
    public static int PixelsPerUnit {get {return 16; } }

    private void OnValidate()
    {
        ApplySize();
    }

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        ApplySize();
    }
    void LateUpdate()
    {
        Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 diff = lastMousePosition - mousePos;
        if (IsDraggingCamera)
            transform.Translate(diff, Space.World);

        mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition = mousePos;

        if (InputManager.LeftClick() && diff.magnitude >= 0.05f)
        {
            IsDraggingCamera = true;
        }
        else if (InputManager.LeftClickUp() && IsDraggingCamera)
        {
            IsDraggingCamera = false;
        }
    }

    void ApplySize()
    {
        if (camera == null)
            camera = GetComponent<Camera>();

        camera.orthographicSize = desiredHeight / (2 * PixelsPerUnit);
    }
}