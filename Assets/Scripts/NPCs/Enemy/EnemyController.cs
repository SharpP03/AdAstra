using System.Collections;
using UnityEngine;

[SelectionBase]
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float detectRange = 200f;
    public float attackRange = 150f;
    public float fireRate = 1f;
    private float nextFireTime;

    private Player_Spaceship player_Spaceship;
    private Transform player;

    public GameObject projectilePrefab;
    public Transform shootPoint;


    private IEnumerator WaitForPlayer()
    {
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
            yield return null;

        player_Spaceship = GameManager.Instance.Player;
        player = player_Spaceship.transform;
    }

    void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    void Update()
    {
        if (!HasPlayer()) return;

        HandleDetection();
        HandleMovement();
        HandleAttack();
    }

    bool HasPlayer()
    {
        if (player != null) { return true; }
        else
        { Debug.LogWarning("Player not found ! {AI enemy}"); return false; }
    }

    void HandleDetection()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance >= detectRange) return;

        LookAtPlayer();
    }

    void HandleMovement()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void HandleAttack()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        TryShoot();
    }

    public float turnSpeed = 10f;
    void LookAtPlayer(float minDistance = 0.01f)
    {
        if (player == null) return;

        Vector3 rawDir = player.position - transform.position;
        if (rawDir.sqrMagnitude < minDistance * minDistance) return;

        Vector3 dir = rawDir.normalized;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir, Vector3.up),
            turnSpeed * Time.deltaTime
        );

    }

    // TODO: Przeanalizować system strzelania
    void TryShoot()
    {
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (projectilePrefab != null && shootPoint != null)
            {
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                Vector3 playerPos = player.position;

                Vector3 aimPoint = playerPos;

                if (playerRb != null)
                {
                    // 🧠 przewidywanie gdzie gracz będzie
                    float projectileSpeed = projectilePrefab.GetComponent<EnemyProjectile>().speed;
                    float distance = Vector3.Distance(shootPoint.position, playerPos);
                    float timeToHit = distance / projectileSpeed;

                    aimPoint = playerPos + playerRb.linearVelocity * timeToHit;
                }

                // 🎯 offset, aby nie strzelał pod gracza
                aimPoint += Vector3.up * 1.5f;

                // 🔥 lekkie niedokładności żeby AI nie było sniperem
                float spread = 0.05f;
                aimPoint += Random.insideUnitSphere * spread;

                // 🚀 kierunek strzału
                Vector3 dir = (aimPoint - shootPoint.position).normalized;
                Quaternion rot = Quaternion.LookRotation(dir);

                Instantiate(projectilePrefab, shootPoint.position, rot);
            }
        }
    }
}