using UnityEngine;

public class ChoicePickup : MonoBehaviour
{
    [SerializeField] private StatChoiceManager choiceManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        choiceManager.Open();
        Destroy(gameObject);
    }
}
