using UnityEngine;
using UnityEngine.InputSystem;

public class ClockHoverDetector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float maxDistance = 100f;

    private ClockInteraction clock;
    private ClockHover currentHover;

    private void Awake()
    {
        clock = GetComponent<ClockInteraction>();
    }

    private void Update()
    {
        if (!clock.CanInteract || !clock.IsFocused)
            return;

        if (targetCamera == null || clock == null || Mouse.current == null)
            return;

        if (!clock.IsFocused)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            ClockHover hoveredHand = hit.collider.GetComponentInParent<ClockHover>();

            if (hoveredHand != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
  
                clock.RotateHand(hoveredHand);
            }

            if (hoveredHand != currentHover)
            {
                if (currentHover != null)
                    currentHover.SetHover(false);

                currentHover = hoveredHand;

                if (currentHover != null)
                    currentHover.SetHover(true);
            }
        }
        else if (currentHover != null)
        {
            currentHover.SetHover(false);
            currentHover = null;
        }
    }

    private void OnDisable()
    {
        ClearHover();
    }

    public void ClearHover()
    {
        if (currentHover != null)
        {
            currentHover.SetHover(false);
            currentHover = null;
        }
    }

}