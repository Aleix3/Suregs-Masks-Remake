using UnityEngine;

public class ChargedArrowProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float speed = 10f;

    [SerializeField] private GameObject poisonAreaPrefab;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        rb.velocity = dir * speed;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player ph =
                collision.GetComponent<Player>();

            if (ph != null)
                ph.TakeDamage(damage);

            SpawnPoisonArea();

            Destroy(gameObject);
        }

        //if (collision.CompareTag("Wall"))
        //{
        //    SpawnPoisonArea();

        //    Destroy(gameObject);
        //}
    }

    private void SpawnPoisonArea()
    {
        Instantiate(
            poisonAreaPrefab,
            transform.position,
            Quaternion.identity);
    }
}