using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform player;

    // Altura del pivot sobre los pies del jugador. Si el pivot queda a ras del piso,
    // el AvoidObstacles de Cinemachine choca con el suelo (capa CameraObstacle) y la camara se hunde.
    [SerializeField] private float pivotHeight = 1.5f;

    private void LateUpdate()
    {
        transform.position = player.position + Vector3.up * pivotHeight;
    }
}
