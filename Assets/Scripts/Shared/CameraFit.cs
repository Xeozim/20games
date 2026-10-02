using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class CameraFit : MonoBehaviour
{
    [Header("Play area in world units")]
    [SerializeField] Vector2 targetSize = new Vector2(16f, 9f);
    [SerializeField] Vector2 center = Vector2.zero;

    [Header("Behaviour")]
    [Tooltip("Crop the viewport so no world outside the play area is shown.")]
    [SerializeField] bool letterbox = true;

    Camera cam;
    int lastW, lastH;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        Apply();
    }

    // Cheap check; catches window resizes, device rotation and editor Game view changes.
    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH) Apply();
    }

#if UNITY_EDITOR
    void OnValidate() { if (cam != null || TryGetComponent(out cam)) Apply(); }
#endif

    void Apply()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        if (lastH == 0) return;

        float targetAspect = targetSize.x / targetSize.y;
        float screenAspect = (float)lastW / lastH;

        // Orthographic size is half the visible height.
        // Wide target on a narrow screen: width is the limiting dimension.
        float orthoSize = targetSize.y * 0.5f;
        if (screenAspect < targetAspect)
            orthoSize = targetSize.x / screenAspect * 0.5f;

        cam.orthographicSize = orthoSize;
        transform.position = new Vector3(center.x, center.y, transform.position.z);

        if (!letterbox)
        {
            cam.rect = new Rect(0, 0, 1, 1);
            return;
        }

        // Shrink the viewport to exactly match the target aspect.
        // In this mode, orthoSize above is simply targetSize.y/2.
        if (screenAspect > targetAspect)
        {
            float w = targetAspect / screenAspect;      // pillarbox
            cam.rect = new Rect((1f - w) * 0.5f, 0, w, 1);
            cam.orthographicSize = targetSize.y * 0.5f;
        }
        else
        {
            float h = screenAspect / targetAspect;      // letterbox
            cam.rect = new Rect(0, (1f - h) * 0.5f, 1, h);
            cam.orthographicSize = targetSize.y * 0.5f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, targetSize);
    }
}