using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTarget;
    [SerializeField] private float smoothTime;
    [SerializeField] private float followOrthoSize = 11;
    [SerializeField] private float followOrthoSizeLerpSpeed = 0.1f;

    private Vector3 velocity;

    private void OnEnable()
    {
        BirdController.OnGameOver += OnGameOver;
    }
    private void OnDisable()
    {
        BirdController.OnGameOver -= OnGameOver;
    }
    private void OnGameOver()
    {
        followOrthoSize = 5;
    }


    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 targetPos = new Vector3(
            followTarget.position.x,
            transform.position.y,
            transform.position.z
        );


        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, followOrthoSize, Time.deltaTime * followOrthoSizeLerpSpeed);
    }
}
