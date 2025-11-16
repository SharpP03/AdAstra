using UnityEngine;

public abstract class EnemyAIBase : MonoBehaviour
{
    protected Transform player;
    protected EnemyWeaponBase weapon;

    protected virtual void Start()
    {
        player = GameManager.Instance.Player.transform;
        weapon = GetComponent<EnemyWeaponBase>();
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
