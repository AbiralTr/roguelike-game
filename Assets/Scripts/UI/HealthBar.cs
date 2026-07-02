using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private PlayerData playerData;

    void Update()
    {
        if (playerData.maxHealth <= 0) return;
        fillImage.fillAmount = (float)playerData.currentHealth / playerData.maxHealth;
    }
}