using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FenceGateTrigger : MonoBehaviour
{
    [SerializeField] private FenceGate gate;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gate == null || !other.CompareTag("Player")) return;

        gate.SlamAndLock();
    }
}
