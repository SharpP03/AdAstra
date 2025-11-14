using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ParticleStateSettings
{
    public float emissionRate;
    public float startSpeed;
    public Color startColor;
    public float velocityZ; 
}

public class PlayerThrusterFXController : MonoBehaviour
{
    [Header("Particle Systems attached to the ship")]
    [SerializeField] private List<ParticleSystem> thrusters = new();

    [Header("Particle settings")]
    [SerializeField] private ParticleStateSettings standstillSettings;
    [SerializeField] private ParticleStateSettings movingSettings;
    [SerializeField] private ParticleStateSettings sprintingSettings;

    [Header("Lerp speed (transition smoothness)")]
    [SerializeField] private float transitionSpeed = 5f;

    private Player_Spaceship player;
    private ParticleStateSettings targetSettings;

    private void Start()
    {
        //player = GetComponent<Player_Spaceship>();
        player = GameManager.Instance.Player;
        if (player == null)
        {
            Debug.LogError("ThrusterFXController: Player_Spaceship not found!");
            enabled = false;
            return;
        }

        targetSettings = GetSettingsForState(player.CurrentState);
        ApplySettingsInstant(targetSettings);
    }

    private void Update()
    {
        // aktualny target w zale¿noœci od stanu gracza
        targetSettings = GetSettingsForState(player.CurrentState);

        foreach (var ps in thrusters)
        {
            if (ps == null) continue;

            // --- EMISSION ---
            var emission = ps.emission;
            float currentRate = emission.rateOverTime.constant;
            emission.rateOverTime = Mathf.Lerp(currentRate, targetSettings.emissionRate, Time.deltaTime * transitionSpeed);

            // --- MAIN ---
            var main = ps.main;
            main.startSpeed = Mathf.Lerp(main.startSpeed.constant, targetSettings.startSpeed, Time.deltaTime * transitionSpeed);
            main.startColor = Color.Lerp(main.startColor.color, targetSettings.startColor, Time.deltaTime * transitionSpeed);

            // --- VELOCITY OVER LIFETIME (Z axis) ---
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            float currentVelZ = vel.z.constant;
            vel.z = Mathf.Lerp(currentVelZ, targetSettings.velocityZ, Time.deltaTime * transitionSpeed);
        }
    }

    private ParticleStateSettings GetSettingsForState(PlayerState state)
    {
        return state switch
        {
            PlayerState.Sprinting => sprintingSettings,
            PlayerState.Moving => movingSettings,
            _ => standstillSettings
        };
    }

    private void ApplySettingsInstant(ParticleStateSettings settings)
    {
        foreach (var ps in thrusters)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.rateOverTime = settings.emissionRate;

            var main = ps.main;
            main.startSpeed = settings.startSpeed;
            main.startColor = settings.startColor;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.z = settings.velocityZ;
        }
    }
}
