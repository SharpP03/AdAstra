using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody projectileRigidbody;
    [SerializeField] private Collider projectileCollider;

    private EnemyProjectilePool owningPool;
    private GameObject ownerRoot;
    private float damage;
    private LayerMask damageLayers;
    private float despawnTime;
    private bool isActiveProjectile;

    private void Reset()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (projectileRigidbody == null)
            projectileRigidbody = GetComponent<Rigidbody>();
        if (projectileCollider == null)
            projectileCollider = GetComponent<Collider>();
    }

    public void SetOwningPool(EnemyProjectilePool pool)
    {
        owningPool = pool;
    }

    public void Launch(Vector3 position, Vector3 direction, EnemyWeaponConfig weaponConfig, GameObject projectileOwnerRoot)
    {
        if (weaponConfig == null)
        {
            Debug.LogWarning($"EnemyProjectile on '{name}' received null EnemyWeaponConfig.");
            ReturnToPool();
            return;
        }

        ownerRoot = projectileOwnerRoot;
        damage = weaponConfig.damage;
        damageLayers = weaponConfig.damageLayers;
        despawnTime = Time.time + weaponConfig.projectileLifetime;
        isActiveProjectile = true;

        var safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(safeDirection, Vector3.up));

        if (projectileCollider != null)
            projectileCollider.enabled = true;

        if (projectileRigidbody != null)
            projectileRigidbody.linearVelocity = safeDirection * weaponConfig.projectileSpeed;
    }

    private void Update()
    {
        if (!isActiveProjectile) return;
        if (Time.time >= despawnTime)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveProjectile || other == null) return;
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActiveProjectile || collision.collider == null) return;
        HandleHit(collision.collider.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        if (hitObject == null)
        {
            ReturnToPool();
            return;
        }

        var rootObject = hitObject.transform.root.gameObject;
        if (rootObject == ownerRoot)
            return;

        var layerMaskBit = 1 << hitObject.layer;
        if ((damageLayers.value & layerMaskBit) == 0)
        {
            ReturnToPool();
            return;
        }

        var damageable = EnemyDamageableResolver.Resolve(rootObject);
        if (damageable != null)
            damageable.TakeDamage(damage);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!isActiveProjectile) return;
        isActiveProjectile = false;

        if (projectileCollider != null)
            projectileCollider.enabled = false;

        if (projectileRigidbody != null)
            projectileRigidbody.linearVelocity = Vector3.zero;

        if (owningPool != null)
            owningPool.Release(this);
        else
            Destroy(gameObject);
    }
}
