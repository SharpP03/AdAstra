using UnityEngine;

public class EnemyTargetProvider : MonoBehaviour
{
    [SerializeField] private bool useGameManagerPlayer = true;
    [SerializeField] private string fallbackPlayerTag = "Player";
    [SerializeField] [Min(0.05f)] private float reacquireInterval = 0.5f;

    public Transform CurrentTarget { get; private set; }

    private float nextReacquireTime;

    private void OnEnable()
    {
        nextReacquireTime = 0f;
        TryAcquireTarget();
    }

    private void Update()
    {
        if (CurrentTarget != null && Time.time < nextReacquireTime) return;

        nextReacquireTime = Time.time + reacquireInterval;
        TryAcquireTarget();
    }

    private void TryAcquireTarget()
    {
        if (useGameManagerPlayer && GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            CurrentTarget = GameManager.Instance.Player.transform;
            return;
        }

        if (string.IsNullOrWhiteSpace(fallbackPlayerTag))
        {
            CurrentTarget = null;
            return;
        }

        var found = GameObject.FindGameObjectWithTag(fallbackPlayerTag);
        CurrentTarget = found != null ? found.transform : null;
    }
}
