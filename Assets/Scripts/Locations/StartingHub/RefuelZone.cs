using UnityEngine;

public class RefuelZone : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ResourceKind resourceKind = ResourceKind.Fuel;
    [SerializeField]
    private float amountToAdd = 20f;
    [SerializeField]
    private GameObject zoneObject;
    [SerializeField]
    private bool fillToMax = false;


    private void Awake()
    {
        if (zoneObject == null)
        {
            Debug.LogWarning($"RefuelZone '{name}' has no zoneObject assigned!");
        }
    }

    public void Interact(GameObject player)
    {
        IRefillable[] refillables = player.GetComponents<IRefillable>();
        if (refillables == null || refillables.Length == 0) return;

        for (int i = 0; i < refillables.Length; i++)
        {
            IRefillable refillable = refillables[i];
            if (refillable.Kind != resourceKind) continue;

            float amount = fillToMax ? refillable.Max : amountToAdd;
            refillable.Add(amount);
            break;
        }
    }

    public void OnChildTriggerEnter(Collider other)
    {
        //Debug.Log("Collider: " + other);
        GameObject rootObject = other.transform.root.gameObject;
        if (rootObject.CompareTag("Player"))
        { Interact(rootObject); }
    }

}