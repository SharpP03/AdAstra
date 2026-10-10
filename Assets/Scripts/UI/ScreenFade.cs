using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen color fade, used by the Warp Gate jump (fade to white and back).
/// </summary>
public class ScreenFade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    /// <summary>Fades in with ease-in, so the screen whites out at the very end of the duration.</summary>
    public void FadeIn(float duration)
    {
        StartFade(1f, duration, easeIn: true);
    }

    /// <summary>Fades out with ease-out, revealing the scene quickly and settling softly.</summary>
    public void FadeOut(float duration)
    {
        StartFade(0f, duration, easeIn: false);
    }

    private void StartFade(float targetAlpha, float duration, bool easeIn)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, easeIn));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool easeIn)
    {
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = easeIn ? t * t : 1f - (1f - t) * (1f - t);
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, eased));
            yield return null;
        }

        SetAlpha(targetAlpha);
        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        fadeImage.enabled = alpha > 0f;
    }
}
