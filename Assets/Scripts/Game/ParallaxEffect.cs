using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Transform cam;
    public Vector2 parallaxMultiplier = new Vector2(0.5f, 0f);

    [Range(0.01f, 1f)]
    public float smoothing = 0.1f;

    public bool repeatX = false;
    public bool repeatY = false;

    private Vector3 previousCamPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    public void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        previousCamPos = cam.position;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && (repeatX || repeatY))
        {
            textureUnitSizeX = spriteRenderer.sprite.bounds.size.x;
            textureUnitSizeY = spriteRenderer.sprite.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 delta = cam.position - previousCamPos;
        Vector3 move = new Vector3(delta.x * parallaxMultiplier.x, delta.y * parallaxMultiplier.y, 0);
        Vector3 targetPos = transform.position + move;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);


        if (repeatX && Mathf.Abs(cam.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetX = (cam.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cam.position.x + offsetX, transform.position.y, transform.position.z);
        }

        if (repeatY && Mathf.Abs(cam.position.y - transform.position.y) >= textureUnitSizeY)
        {
            float offsetY = (cam.position.y - transform.position.y) % textureUnitSizeY;
            transform.position = new Vector3(transform.position.x, cam.position.y + offsetY, transform.position.z);
        }

        previousCamPos = cam.position;
    }
}