using UnityEngine;

public class StatPickup : MonoBehaviour
{
    [SerializeField] private StatPickupData data;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PickupPopupManager popupManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerData.ApplyStatBoost(data.statType, data.amount);
        if (popupManager != null) popupManager.Show(data.statType, data.amount);
        Destroy(gameObject);
    }
}
