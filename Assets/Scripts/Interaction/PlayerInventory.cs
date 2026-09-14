using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly HashSet<string> keys = new HashSet<string>();

    public void AddKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId)) return;

        keys.Add(keyId);
        Debug.Log($"Llave obtenida: {keyId}");
    }

    public bool HasKey(string keyId)
    {
        return !string.IsNullOrWhiteSpace(keyId) && keys.Contains(keyId);
    }
}
