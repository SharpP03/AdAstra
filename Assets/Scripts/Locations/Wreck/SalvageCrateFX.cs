using UnityEngine;

/// <summary>
/// Cutting feedback of a salvage crate: the crate heats from its own color to white as the cut progresses,
/// sparks fly from the beam contact point, and a cut crate bursts.
/// </summary>
[RequireComponent(typeof(SalvageCrate))]
public class SalvageCrateFX : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Renderer visual;
    [Tooltip("Color the crate reaches right before it bursts (HDR, so it glows).")]
    [ColorUsage(false, true)]
    [SerializeField] private Color hotColor = new Color(4f, 3.6f, 3f);

    [Header("Sparks")]
    [Tooltip("Plays once and destroys itself; emits nothing on its own.")]
    [SerializeField] private ParticleSystem sparksPrefab;
    [SerializeField] private float sparkInterval = 0.06f;
    [SerializeField] private int sparksPerBurst = 4;

    [Header("Burst")]
    [Tooltip("Spawned where the crate was when it is cut through; plays and destroys itself.")]
    [SerializeField] private ParticleSystem burstPrefab;

    private SalvageCrate crate;
    private MaterialPropertyBlock block;
    private Color coldBaseColor;
    private Color coldEmission;
    private float nextSparkTime;

    private void Awake()
    {
        crate = GetComponent<SalvageCrate>();
        block = new MaterialPropertyBlock();
        if (visual != null)
        {
            Material material = visual.sharedMaterial;
            coldBaseColor = material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId) : Color.white;
            coldEmission = material.HasProperty(EmissionColorId) ? material.GetColor(EmissionColorId) : Color.black;
        }
    }

    private void OnEnable()
    {
        crate.OnCutProgress += HandleCutProgress;
        crate.OnOpened += HandleOpened;
    }

    private void OnDisable()
    {
        crate.OnCutProgress -= HandleCutProgress;
        crate.OnOpened -= HandleOpened;
    }

    #region EDUCATION NOTE material property block
    // DO PRACY INŻ. - MaterialPropertyBlock zmienia kolor jednego renderera bez tworzenia kopii materiału
    // (renderer.material by ją tworzył). Wszystkie skrzynie dzielą ten sam materiał i batching,
    // a każda rozgrzewa się niezależnie.
    #endregion
    private void HandleCutProgress(float progress)
    {
        if (visual != null)
        {
            visual.GetPropertyBlock(block);
            block.SetColor(BaseColorId, Color.Lerp(coldBaseColor, Color.white, progress));
            block.SetColor(EmissionColorId, Color.Lerp(coldEmission, hotColor, progress * progress));
            visual.SetPropertyBlock(block);
        }

        if (sparksPrefab != null && Time.time >= nextSparkTime)
        {
            nextSparkTime = Time.time + sparkInterval;
            Vector3 normal = crate.ContactPoint - transform.position;
            if (normal.sqrMagnitude < 0.0001f) normal = transform.up;
            ParticleSystem sparks = Instantiate(sparksPrefab, crate.ContactPoint, Quaternion.LookRotation(normal));
            sparks.Emit(sparksPerBurst);
        }
    }

    private void HandleOpened(float refilled)
    {
        if (burstPrefab != null)
            Instantiate(burstPrefab, transform.position, transform.rotation).Play();
    }
}
