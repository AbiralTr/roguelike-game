using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private EnemyHealth enemyHealth;

    void Update()
    {
        if (enemyHealth.MaxHealth <= 0) return;
        fillImage.fillAmount = (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
    }
}
