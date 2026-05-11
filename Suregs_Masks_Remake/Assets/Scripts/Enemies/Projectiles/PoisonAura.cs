using UnityEngine;

public class PoisonAura : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float tickRate = 2f;

    private float timer;

    private void Start()
    {
        Destroy(gameObject, duration);

        timer = tickRate;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (timer > 0)
            return;

        Player ph =
            collision.GetComponent<Player>();

        if (ph != null)
        {
            ph.TakeDamage(damage);
        }

        timer = tickRate;
    }
}