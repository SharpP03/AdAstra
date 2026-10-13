using UnityEngine;

/// <summary>
/// Stasis presentation for the Annihilation Wave: while frozen, the plasma curtain and threat light
/// fade to cold colors, the curtain stops pulsing and the wave's particles pause mid-air.
/// </summary>
[RequireComponent(typeof(AnnihilationWaveController))]
public class AnnihilationWaveStasisFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer curtainRenderer;
    [SerializeField] private Light threatLight;

    [Header("Frozen Look")]
    [SerializeField, ColorUsage(false, true)] private Color frozenDeepColor = new Color(0.005f, 0.02f, 0.08f);
    [SerializeField, ColorUsage(false, true)] private Color frozenCoreColor = new Color(0.25f, 0.9f, 1.8f);
    [SerializeField, ColorUsage(false, true)] private Color frozenFilamentColor = new Color(0.8f, 2.2f, 3.5f);
    [SerializeField, ColorUsage(false, true)] private Color frozenContactGlowColor = new Color(1.2f, 3f, 4.5f);
    [SerializeField] private Color frozenLightColor = new Color(0.45f, 0.7f, 1f);
    [Tooltip("Seconds the colors take to blend in and out.")]
    [SerializeField] private float blendDuration = 0.4f;

    private static readonly int DeepColorID = Shader.PropertyToID("_DeepColor");
    private static readonly int CoreColorID = Shader.PropertyToID("_CoreColor");
    private static readonly int FilamentColorID = Shader.PropertyToID("_FilamentColor");
    private static readonly int ContactGlowColorID = Shader.PropertyToID("_ContactGlowColor");
    private static readonly int PulseIntensityID = Shader.PropertyToID("_PulseIntensity");

    private AnnihilationWaveController controller;
    private ParticleSystem[] particles;
    private bool[] particlesWerePlaying;
    private MaterialPropertyBlock mpb;

    private Color deepColor, coreColor, filamentColor, contactGlowColor, lightColor;
    private float pulseIntensity;
    private float blend;
    private bool frozen;

    private void Awake()
    {
        controller = GetComponent<AnnihilationWaveController>();
        particles = GetComponentsInChildren<ParticleSystem>(true);
        particlesWerePlaying = new bool[particles.Length];
        mpb = new MaterialPropertyBlock();

        // The material's own values are the "alive" look we blend back to.
        if (curtainRenderer != null)
        {
            Material mat = curtainRenderer.sharedMaterial;
            deepColor = mat.GetColor(DeepColorID);
            coreColor = mat.GetColor(CoreColorID);
            filamentColor = mat.GetColor(FilamentColorID);
            contactGlowColor = mat.GetColor(ContactGlowColorID);
            pulseIntensity = mat.GetFloat(PulseIntensityID);
        }
        if (threatLight != null) lightColor = threatLight.color;
    }

    private void OnEnable()
    {
        controller.OnStasisChanged += HandleStasisChanged;
    }

    private void OnDisable()
    {
        controller.OnStasisChanged -= HandleStasisChanged;
    }

    private void HandleStasisChanged(bool isFrozen)
    {
        frozen = isFrozen;

        for (int i = 0; i < particles.Length; i++)
        {
            if (isFrozen)
            {
                particlesWerePlaying[i] = particles[i].isPlaying;
                particles[i].Pause(false);
            }
            else if (particlesWerePlaying[i])
            {
                particles[i].Play(false);
            }
        }
    }

    private void Update()
    {
        float target = frozen ? 1f : 0f;
        if (Mathf.Approximately(blend, target)) return;

        blend = Mathf.MoveTowards(blend, target, Time.deltaTime / Mathf.Max(0.01f, blendDuration));
        float t = blend * blend * (3f - 2f * blend); // smoothstep

        if (curtainRenderer != null)
        {
            curtainRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(DeepColorID, LerpRGB(deepColor, frozenDeepColor, t));
            mpb.SetColor(CoreColorID, LerpRGB(coreColor, frozenCoreColor, t));
            mpb.SetColor(FilamentColorID, LerpRGB(filamentColor, frozenFilamentColor, t));
            mpb.SetColor(ContactGlowColorID, LerpRGB(contactGlowColor, frozenContactGlowColor, t));
            mpb.SetFloat(PulseIntensityID, Mathf.Lerp(pulseIntensity, 0f, t));
            curtainRenderer.SetPropertyBlock(mpb);
        }
        if (threatLight != null) threatLight.color = Color.Lerp(lightColor, frozenLightColor, t);
    }

    // The curtain colors carry the shader's opacity in alpha, so only the hue changes.
    private static Color LerpRGB(Color from, Color to, float t)
    {
        Color c = Color.Lerp(from, to, t);
        c.a = from.a;
        return c;
    }
}
