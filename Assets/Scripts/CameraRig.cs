using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void LateUpdate()
    {
        transform.position = player.position;
    }
}