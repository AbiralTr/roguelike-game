using System.Collections;
using UnityEngine;
using TMPro;

public class PickupPopupManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup popupGroup;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    public void Show(StatType type, float amount)
    {
        string label = type switch
        {
            StatType.Health => "Max Health",
            StatType.Damage => "Damage",
            StatType.Speed => "Speed",
            _ => type.ToString()
        };

        popupText.text = $"+{amount:0.#} {label}";

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        popupGroup.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            popupGroup.alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        popupGroup.alpha = 0f;
    }
}
