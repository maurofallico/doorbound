using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly HashSet<string> keys = new HashSet<string>();

    public bool HasAnyKey => keys.Count > 0;

    public void AddKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId)) return;

        keys.Add(keyId);
        GameHud.Instance?.SetInventory(string.Join(", ", keys));
        GameHud.Instance?.ShowMessage($"Llave obtenida: {keyId}");
        Debug.Log($"Llave obtenida: {keyId}");
    }

    public bool HasKey(string keyId)
    {
        return !string.IsNullOrWhiteSpace(keyId) && keys.Contains(keyId);
    }
}
