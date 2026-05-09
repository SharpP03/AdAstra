using UnityEngine;

public class RefuelChildCollider : MonoBehaviour
{
    private RefuelZone parentZone;

    private void Awake()
    {
        parentZone = GetComponentInParent<RefuelZone>();
        if (parentZone == null)
            Debug.LogWarning("TriggerForwarder could not find RefuelZone in parent!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parentZone != null)
        {
            parentZone.OnChildTriggerEnter(other);
        }
    }
}