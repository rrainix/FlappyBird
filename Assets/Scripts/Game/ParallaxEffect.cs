using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera Transform to follow for parallax calculation.")]
    public Transform cam;

    [Header("Parallax Settings")]
    [Tooltip("How much the background moves relative to the camera movement. X = horizontal multiplier, Y = vertical multiplier.")]
    public Vector2 parallaxMultiplier = new Vector2(0.5f, 0f);

    [Tooltip("Smoothness of the parallax movement. Higher = slower, smoother.")]
    [Range(0.01f, 1f)]
    public float smoothing = 0.1f;

    [Header("Repeat Background")]
    [Tooltip("Enable horizontal looping of the background sprite.")]
    public bool repeatX = false;
    [Tooltip("Enable vertical looping of the background sprite.")]
    public bool repeatY = false;

    private Vector3 _previousCamPos;
    private float _textureUnitSizeX;
    private float _textureUnitSizeY;

    public void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        _previousCamPos = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && (repeatX || repeatY))
        {
            // Calculate texture size in world units
            _textureUnitSizeX = sr.sprite.bounds.size.x;
            _textureUnitSizeY = sr.sprite.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 delta = cam.position - _previousCamPos;
        Vector3 move = new Vector3(delta.x * parallaxMultiplier.x, delta.y * parallaxMultiplier.y, 0);
        Vector3 targetPos = transform.position + move;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);


        if (repeatX && Mathf.Abs(cam.position.x - transform.position.x) >= _textureUnitSizeX)
        {
            float offsetX = (cam.position.x - transform.position.x) % _textureUnitSizeX;
            transform.position = new Vector3(cam.position.x + offsetX, transform.position.y, transform.position.z);
        }

        if (repeatY && Mathf.Abs(cam.position.y - transform.position.y) >= _textureUnitSizeY)
        {
            float offsetY = (cam.position.y - transform.position.y) % _textureUnitSizeY;
            transform.position = new Vector3(transform.position.x, cam.position.y + offsetY, transform.position.z);
        }

        _previousCamPos = cam.position;
    }
}