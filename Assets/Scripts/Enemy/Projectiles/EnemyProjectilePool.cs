using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyProjectilePool : MonoBehaviour
{
    [SerializeField] private EnemyProjectile preloadProjectilePrefab;
    [SerializeField] [Min(0)] private int initialSize = 12;
    [SerializeField] private bool autoExpand = true;
    [SerializeField] private Transform poolContainer;

    private readonly Queue<EnemyProjectile> availableProjectiles = new();

    private void Awake()
    {
        if (poolContainer == null)
            poolContainer = transform;

        if (preloadProjectilePrefab == null)
        {
            if (initialSize > 0)
                Debug.LogWarning(
                    $"EnemyProjectilePool on '{name}' has no preloadProjectilePrefab. Prewarm skipped until first runtime request.");
            return;
        }

        for (var i = 0; i < initialSize; i++)
        {
            var projectile = CreateInstance(preloadProjectilePrefab);
            if (projectile == null) break;
            availableProjectiles.Enqueue(projectile);
        }
    }

    public EnemyProjectile Get()
    {
        return Get(preloadProjectilePrefab);
    }

    public EnemyProjectile Get(EnemyProjectile projectilePrefab)
    {
        if (projectilePrefab == null) return null;

        if (availableProjectiles.Count == 0)
        {
            if (!autoExpand) return null;
            var created = CreateInstance(projectilePrefab);
            if (created == null) return null;
            availableProjectiles.Enqueue(created);
        }

        var projectile = availableProjectiles.Dequeue();
        if (projectile == null) return null;

        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void Release(EnemyProjectile projectile)
    {
        if (projectile == null) return;
        if (!projectile.gameObject.activeSelf) return;

        projectile.gameObject.SetActive(false);
        availableProjectiles.Enqueue(projectile);
    }

    private EnemyProjectile CreateInstance(EnemyProjectile projectilePrefab)
    {
        if (projectilePrefab == null) return null;

        var instance = Instantiate(projectilePrefab, poolContainer);
        if (instance == null) return null;

        instance.SetOwningPool(this);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
