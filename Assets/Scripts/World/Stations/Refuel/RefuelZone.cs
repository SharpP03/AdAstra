using System;
using UnityEngine;

public class RefuelZone : MonoBehaviour, IInteractable
{
    [SerializeField]
    private float fuelToAdd = 20f;
    [SerializeField]
    private GameObject zoneObject;
    public bool addMaxFuel = false;


    private void Awake()
    {
        if (zoneObject == null)
        {
            Debug.LogWarning($"RefuelZone '{name}' has no zoneObject assigned!");
        }
    }

    public void Interact(GameObject player)
    {
        FuelSystem fuelSystem = player.GetComponent<FuelSystem>();


        if (fuelSystem)
        {
            fuelSystem.AddFuel(addMaxFuel ? fuelSystem.MaxFuel : fuelToAdd);

        }

    }

    public void OnChildTriggerEnter(Collider other)
    {
        Debug.Log("Collider: " + other);
        GameObject rootObject = other.transform.root.gameObject;
        if (rootObject.CompareTag("Player"))
        { Interact(rootObject); }
    }

}
