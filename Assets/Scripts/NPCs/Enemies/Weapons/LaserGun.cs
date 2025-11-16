using UnityEngine;
using System.Collections;

public class LaserGun : EnemyWeaponBase
{
    private Transform player;
    private Rigidbody playerRb;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance == null || GameManager.Instance.Player == null)
            yield return null;

        player = GameManager.Instance.Player.transform;
        playerRb = player.GetComponent<Rigidbody>();
    }

    public override void TryShoot()
    {
        if (player == null || shootPoint == null || projectilePrefab == null)
        { Debug.Log("Current gun " + this.name + " settings are incomplete"); return; }

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + fireRate;

        Vector3 playerPos = player.position;
        Vector3 aimPoint = playerPos;

        // Predict move -> position + velocity
        if (playerRb != null)
        {
            float projectileSpeed = projectilePrefab.GetComponent<EnemyProjectile>().speed;
            float distance = Vector3.Distance(shootPoint.position, playerPos);
            float timeToHit = distance / projectileSpeed;

            aimPoint += playerRb.linearVelocity * timeToHit;
        }

        // shoot at ship correction (not below model)
        aimPoint += Vector3.up * 1.2f;

        // Minimalny rozrzut ¿eby wygl¹da³o naturalniej
        float spread = 0.05f;
        aimPoint += Random.insideUnitSphere * spread;

        Vector3 dir = (aimPoint - shootPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        Instantiate(projectilePrefab, shootPoint.position, rot);
    }
}
