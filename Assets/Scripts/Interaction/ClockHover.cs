
using UnityEngine;
using UnityEngine.UI;

public class ClockHover : MonoBehaviour
{
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Transform rotationPivot;

    [SerializeField] private ClockHandType handType;

    public ClockHandType HandType => handType;
    public Transform RotationPivot => rotationPivot;

    private Material handMaterial;
    [SerializeField] private GameObject outline;

    private Color originalColor;

    public enum ClockHandType
    {
        Hour,
        Minute
    }

    private void Awake()
    {
        if (outline != null)
            outline.SetActive(false);
    }

    public void SetHover(bool hovering)
    {
        if (outline != null)
            outline.SetActive(hovering);
    }



    private void OnDestroy()
    {
        if (handMaterial != null)
            Destroy(handMaterial);
    }
}
