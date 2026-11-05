using UnityEngine;

public sealed class RoomLightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private Light[] roomLights = new Light[0];
    [SerializeField] private bool startsOn = true;
    [SerializeField] private Transform rotaryMechanism;
    [SerializeField] private Vector3 onRotation = new Vector3(0f, 0f, 90f);
    [SerializeField] private Vector3 offRotation = Vector3.zero;
    [SerializeField] private Renderer[] emissiveRenderers = new Renderer[0];
    [ColorUsage(false, true)]
    [SerializeField] private Color onEmissionColor = new Color(1f, 0.7f, 0.4f);
    private MaterialPropertyBlock emissionProperties;
    public bool IsOn { get; private set; }
    public Light[] RoomLights => roomLights;
    public string Prompt => IsOn ? "Presiona E para apagar la luz" : "Presiona E para prender la luz";

    private void Awake() => SetLights(startsOn);

    public void Interact(PlayerInventory playerInventory)
    {
        SetLights(!IsOn);
        DoorboundAudio.PlayIfUnassigned(IsOn ? DoorboundAudio.Cue.LightOn : DoorboundAudio.Cue.LightOff, this);
    }

    public void SetLights(bool on)
    {
        IsOn = on;
        if (rotaryMechanism != null)
            rotaryMechanism.localRotation = Quaternion.Euler(on ? onRotation : offRotation);
        foreach (Light roomLight in roomLights)
            if (roomLight != null) roomLight.enabled = on;
        if (emissionProperties == null) emissionProperties = new MaterialPropertyBlock();
        foreach (Renderer source in emissiveRenderers)
        {
            if (source == null) continue;
            source.GetPropertyBlock(emissionProperties);
            emissionProperties.SetColor("_EmissionColor", on ? onEmissionColor : Color.black);
            source.SetPropertyBlock(emissionProperties);
        }
    }
}


