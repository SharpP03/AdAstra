using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float detectRange = 100f;
    public float attackRange = 20f;
    public float fireRate = 1.5f;
    private float nextFireTime;

    private Player_Spaceship player_Spaceship;
    private Transform player;

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
        if (player != null) { return true; } else
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

    void LookAtPlayer(float turnSpeed = 5f, float minDistance = 0.01f)
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

    void TryShoot()
    {
        if (Time.time > nextFireTime)
        {
            //Debug.Log("Enemy fires!");
            nextFireTime = Time.time + fireRate;
            // Tu mo¿esz dodaæ Instantiate(projectile, ...);
        }
    }
}