using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float verticalFollowSpeed = 16f;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = player.position;
        float smoothing = 1f - Mathf.Exp(-verticalFollowSpeed * Time.deltaTime);
        transform.position = new Vector3(
            target.x,
            Mathf.Lerp(transform.position.y, target.y, smoothing),
            target.z
        );
    }
}
