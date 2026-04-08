using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Rendering.Universal;

public class GameDayNightCycle : MonoBehaviour
{
    [Header("Tiempo")]
    [SerializeField] private float realSecondsPerGameHour = 10f;

    [Header("Referencias")]
    [SerializeField] private GameObject fatherNPC;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Light2D globalLight;

    [Header("Luz")]
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.2f;

    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.1f, 0.1f, 0.25f);

    private int currentHour;
    private bool isNight;

    private void Start()
    {
        SetTextAlpha(0f);
        SetDayLight();
    }

    public void StartNight()
    {
        if (!isNight)
        {
            StartCoroutine(NightRoutine());
        }
    }

    IEnumerator NightRoutine()
    {
        isNight = true;

        // Reiniciar hora correctamente
        currentHour = 18;

        // Actualizar UI inmediatamente
        UpdateTimeUI();

        // Ocultar padre
        fatherNPC.SetActive(false);

        // Transición a noche
        yield return StartCoroutine(ChangeLight(nightIntensity, nightColor));

        // Mostrar contador
        yield return StartCoroutine(FadeText(1f));

        while (currentHour < 30)
        {
            yield return new WaitForSeconds(realSecondsPerGameHour);

            currentHour++;

            UpdateTimeUI();
            Debug.Log("Hora: " + FormatHour(currentHour));
        }

        EndNight();
    }

    void EndNight()
    {
        isNight = false;

        // Mostrar padre
        fatherNPC.SetActive(true);

        // Ocultar contador
        StartCoroutine(FadeText(0f));

        // Volver a día
        StartCoroutine(ChangeLight(dayIntensity, dayColor));
    }

    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            timeText.text = FormatHour(currentHour);
        }
    }

    string FormatHour(int hour)
    {
        int formattedHour = hour % 24;

        if (formattedHour == 0)
            formattedHour = 12;
        else if (formattedHour > 12)
            formattedHour -= 12;

        string ampm = (hour >= 24) ? "AM" : "PM";

        return formattedHour + ":00 " + ampm;
    }

    IEnumerator ChangeLight(float targetIntensity, Color targetColor)
    {
        float duration = 2f;
        float time = 0;

        float startIntensity = globalLight.intensity;
        Color startColor = globalLight.color;

        while (time < duration)
        {
            globalLight.intensity =
                Mathf.Lerp(startIntensity, targetIntensity, time / duration);

            globalLight.color =
                Color.Lerp(startColor, targetColor, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        globalLight.intensity = targetIntensity;
        globalLight.color = targetColor;
    }

    IEnumerator FadeText(float targetAlpha)
    {
        float duration = 2f;
        float time = 0;

        Color startColor = timeText.color;
        Color targetColor = new Color(
            startColor.r,
            startColor.g,
            startColor.b,
            targetAlpha
        );

        while (time < duration)
        {
            timeText.color =
                Color.Lerp(startColor, targetColor, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        timeText.color = targetColor;
    }

    void SetTextAlpha(float alpha)
    {
        Color color = timeText.color;
        color.a = alpha;
        timeText.color = color;
    }

    void SetDayLight()
    {
        globalLight.intensity = dayIntensity;
        globalLight.color = dayColor;
    }
}