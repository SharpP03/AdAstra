using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 40f;
    public float lifetime = 5f;
    public int damage = 10;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trafiono coœ");

        GameObject rootObject = other.transform.root.gameObject;
        if (rootObject.CompareTag("Player"))
        {
            Debug.Log("Trafiono gracza");
            HealthSystem playerHealthSystem = rootObject.GetComponent<HealthSystem>();
            if (playerHealthSystem)
            {
                playerHealthSystem.PlayerTakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
