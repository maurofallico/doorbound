using UnityEngine;

public class PlayerContactHandler
{
    private readonly string playerTag;

    public PlayerContactHandler(string playerTag)
    {
        this.playerTag = playerTag;
    }

    public bool IsPlayerContact(GameObject other)
    {
        for (Transform contact = other.transform; contact != null; contact = contact.parent)
        {
            if (contact.CompareTag(playerTag))
            {
                return true;
            }
        }

        return false;
    }
}
