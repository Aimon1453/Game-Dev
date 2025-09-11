using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        IPickupable pickupItem = other.GetComponent<IPickupable>();

        if (pickupItem != null)
        {
            pickupItem.OnPickup();
        }
    }
}
