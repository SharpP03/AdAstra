using UnityEngine;
using System.Collections;

public abstract class EnemyAIBase : MonoBehaviour
{
    protected Transform player;
    protected EnemyWeaponBase weapon;
    protected EnemyParameters enemyStats;

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance == null || GameManager.Instance.Player == null)
            yield return null;

        player = GameManager.Instance.Player.transform;
        enemyStats = GetComponent<EnemyParameters>();
        weapon = GetComponent<EnemyWeaponBase>();
    }

    protected virtual void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    protected abstract void MoveLogic();
    protected abstract void AttackLogic();

    void Update()
    {
        if (player == null) return;
        MoveLogic();
        AttackLogic();
    }
}
