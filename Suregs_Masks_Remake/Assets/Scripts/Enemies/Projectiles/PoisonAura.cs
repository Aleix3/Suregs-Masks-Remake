using UnityEngine;

public class PoisonAura : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float tickRate = 1f;
    [SerializeField] private float duration = 5f;

    private float timer;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        timer += Time.deltaTime;

        if (timer >= tickRate)
        {
            Player ph =
                collision.GetComponent<Player>();

            if (ph != null)
                ph.TakeDamage(damagePerTick);

            timer = 0f;
        }
    }
}