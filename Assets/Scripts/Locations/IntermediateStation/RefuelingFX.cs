using UnityEngine;

/// <summary>
/// Plays the looping refuel effect only while fuel is flowing from the station into the ship.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class RefuelingFX : MonoBehaviour
{
    [SerializeField] private RefuelStation station;

    private ParticleSystem effect;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (station != null) station.RefuelingChanged += Show;
    }

    private void OnDisable()
    {
        if (station != null) station.RefuelingChanged -= Show;
    }

    private void Start()
    {
        if (station == null) Debug.LogError("RefuelingFX: missing station reference.", this);
    }

    private void Show(bool refueling)
    {
        if (refueling)
            effect.Play(true);
        else
            effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
