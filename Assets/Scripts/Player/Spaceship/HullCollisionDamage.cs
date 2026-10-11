using UnityEngine;

/// <summary>
/// Converts physical impacts of the ship into hull damage. Head-on hits hurt the most,
/// sliding scrapes only lightly, and slow bumps not at all.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HullCollisionDamage : MonoBehaviour
{
    [SerializeField] private HullCollisionDamageConfigSO config;

    private IDamageable hull;
    private Rigidbody rb;
    private float lastHitTime = float.NegativeInfinity;

    private void Awake()
    {
        hull = GetComponent<IDamageable>();
        rb = GetComponent<Rigidbody>();

        if (config == null || hull == null)
        {
            Debug.LogError("HullCollisionDamage: missing config or IDamageable on the same GameObject.", this);
            enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Unity sends collision callbacks to disabled behaviours too.
        if (!enabled) return;
        if (Time.time - lastHitTime < config.hitCooldown) return;

        float damage = EvaluateDamage(GetImpactSpeed(collision)) * GetMassFactor(collision);
        if (damage <= 0f) return;

        lastHitTime = Time.time;
        hull.TakeDamage(damage);
    }

    private float GetImpactSpeed(Collision collision)
    {
        #region EDUCATION NOTE impact speed
        // DO PRACY INŻ. - rozkład prędkości względnej na składową normalną i styczną.
        // collision.relativeVelocity to prędkość względna obu ciał w chwili kontaktu.
        // Rzut na normalną kontaktu (iloczyn skalarny) mówi, jak mocno statek "wbił się" w przeszkodę,
        // reszta wektora (ProjectOnPlane) to ślizg po powierzchni, czyli otarcie.
        // Celowo nie używamy collision.impulse: impuls zależy od mas, a asteroida (masa 1)
        // przy statku (masa 100) dawałaby znikome wartości niezależnie od prędkości uderzenia.
        // Wpływ masy uwzględniamy osobno i jawnie w GetMassFactor: udział masy przeszkody m / (m + mStatku),
        // którego siłę projektant ustawia parametrem massInfluence w konfiguracji.
        #endregion
        Vector3 relativeVelocity = collision.relativeVelocity;

        Vector3 normal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
            normal += collision.GetContact(i).normal;

        if (normal.sqrMagnitude < 0.0001f)
            return relativeVelocity.magnitude;

        normal.Normalize();
        float normalSpeed = Mathf.Abs(Vector3.Dot(relativeVelocity, normal));
        float tangentSpeed = Vector3.ProjectOnPlane(relativeVelocity, normal).magnitude;

        return normalSpeed + config.scrapeFactor * tangentSpeed;
    }

    private float GetMassFactor(Collision collision)
    {
        // Static geometry and kinematic bodies do not yield, so they behave like an infinite mass.
        Rigidbody other = collision.rigidbody;
        if (other == null || other.isKinematic) return 1f;

        float massShare = other.mass / (other.mass + rb.mass);
        return Mathf.Lerp(1f, massShare, config.massInfluence);
    }

    private float EvaluateDamage(float impactSpeed)
    {
        if (impactSpeed < config.safeImpactSpeed) return 0f;

        return Mathf.Min(config.maxDamage, (impactSpeed - config.safeImpactSpeed) * config.damagePerSpeed);
    }
}
