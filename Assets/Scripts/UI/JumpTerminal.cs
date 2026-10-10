using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Warp Gate jump authorization: the player types the shown word on the keyboard.
/// Correct letters turn green with a small pop; a wrong letter is rejected with a red flash
/// and a panel shake, and the progress is kept.
/// </summary>
public class JumpTerminal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform shakeTarget;
    [SerializeField] private Image frame;
    [SerializeField] private TMP_Text wordText;

    [Header("Colors")]
    [SerializeField] private Color typedColor = new Color(0.36f, 1f, 0.48f);
    [SerializeField] private Color pendingColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private Color frameColor = new Color(0.2f, 0.85f, 1f);
    [SerializeField] private Color errorColor = new Color(1f, 0.2f, 0.2f);

    [Header("Feedback")]
    [SerializeField] private float popScale = 1.5f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float errorFlashDuration = 0.25f;
    [SerializeField] private float shakeAmplitude = 14f;
    [SerializeField] private float shakeDecay = 4f;

    public bool IsOpen { get; private set; }

    /// <summary>Raised once the whole word has been typed correctly.</summary>
    public event Action OnWordCompleted;

    private string word;
    private int typedCount;
    private float popTimer;
    private float errorTimer;
    private float shakeTrauma;
    private Vector2 shakeBasePosition;

    private void Awake()
    {
        panelRoot.SetActive(false);
        shakeBasePosition = shakeTarget.anchoredPosition;
    }

    private void OnDisable()
    {
        StopListening();
    }

    public void Open(string authorizationWord)
    {
        word = authorizationWord.ToUpperInvariant();
        typedCount = 0;
        popTimer = 0f;
        errorTimer = 0f;
        shakeTrauma = 0f;
        IsOpen = true;

        panelRoot.SetActive(true);
        frame.color = frameColor;
        RefreshWord();

        if (Keyboard.current != null) Keyboard.current.onTextInput += HandleTextInput;
    }

    public void Close()
    {
        StopListening();
        IsOpen = false;
        panelRoot.SetActive(false);
    }

    private void StopListening()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput -= HandleTextInput;
    }

    private void HandleTextInput(char c)
    {
        if (!IsOpen || typedCount >= word.Length) return;

        char letter = char.ToUpperInvariant(c);
        if (letter < 'A' || letter > 'Z') return; // ignore spaces, digits and control characters

        if (letter == word[typedCount])
        {
            typedCount++;
            popTimer = popDuration;
            RefreshWord();

            if (typedCount == word.Length)
            {
                StopListening();
                OnWordCompleted?.Invoke();
            }
        }
        else
        {
            errorTimer = errorFlashDuration;
            shakeTrauma = 1f;
        }
    }

    private void Update()
    {
        if (!IsOpen) return;

        UpdateErrorFlash();
        UpdateShake();
        UpdateLetterPop();
    }

    private void RefreshWord()
    {
        string typed = word.Substring(0, typedCount);
        string pending = word.Substring(typedCount);
        wordText.text =
            $"<color=#{ColorUtility.ToHtmlStringRGBA(typedColor)}>{typed}</color>" +
            $"<color=#{ColorUtility.ToHtmlStringRGBA(pendingColor)}>{pending}</color>";
    }

    private void UpdateErrorFlash()
    {
        if (errorTimer <= 0f) return;

        errorTimer = Mathf.Max(0f, errorTimer - Time.deltaTime);
        frame.color = Color.Lerp(frameColor, errorColor, errorTimer / errorFlashDuration);
    }

    private void UpdateShake()
    {
        #region EDUCATION NOTE
        // DO PRACY INŻ. - wstrząs oparty o "trauma" (game feel): błąd ustawia trauma = 1, które wygasa liniowo,
        // a amplituda rośnie z kwadratem trauma, więc drganie zaczyna się mocno i szybko łagodnieje.
        // Sinusy o różnych częstotliwościach dają płynny ruch zamiast losowego "szumu" w każdej klatce.
        // Drga tylko panel terminala, nie kamera ani statek.
        #endregion
        if (shakeTrauma <= 0f)
        {
            shakeTarget.anchoredPosition = shakeBasePosition;
            return;
        }

        shakeTrauma = Mathf.Max(0f, shakeTrauma - shakeDecay * Time.deltaTime);
        float strength = shakeAmplitude * shakeTrauma * shakeTrauma;
        float t = Time.time * 60f;
        shakeTarget.anchoredPosition = shakeBasePosition + new Vector2(Mathf.Sin(t * 1.7f), Mathf.Sin(t * 2.3f)) * strength;
    }

    private void UpdateLetterPop()
    {
        if (popTimer <= 0f) return;

        popTimer = Mathf.Max(0f, popTimer - Time.deltaTime);
        float t = 1f - popTimer / popDuration;               // 0 -> 1
        float scale = Mathf.Lerp(popScale, 1f, 1f - Mathf.Pow(1f - t, 3f)); // ease-out back to rest

        // Scale the vertices of the last typed character around its center.
        wordText.ForceMeshUpdate();
        TMP_TextInfo info = wordText.textInfo;
        int charIndex = typedCount - 1;
        if (charIndex < 0 || charIndex >= info.characterCount) return;

        TMP_CharacterInfo charInfo = info.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        Vector3[] vertices = info.meshInfo[charInfo.materialReferenceIndex].vertices;
        int v = charInfo.vertexIndex;
        Vector3 center = (vertices[v] + vertices[v + 2]) * 0.5f;
        for (int i = 0; i < 4; i++)
        {
            vertices[v + i] = center + (vertices[v + i] - center) * scale;
        }
        wordText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
