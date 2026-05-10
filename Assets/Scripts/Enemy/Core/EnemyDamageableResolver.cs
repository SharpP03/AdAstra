using UnityEngine;

public static class EnemyDamageableResolver
{
    public static IDamageable Resolve(GameObject rootObject)
    {
        if (rootObject == null) return null;

        var behaviours = rootObject.GetComponentsInChildren<MonoBehaviour>();
        for (var i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IDamageable damageable)
                return damageable;
        }

        return null;
    }
}
