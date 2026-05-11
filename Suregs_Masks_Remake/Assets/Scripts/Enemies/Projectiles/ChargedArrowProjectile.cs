using UnityEngine;

public class ChargedArrowProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 4f;

    [Header("Poison Trail")]
    [SerializeField] private GameObject poisonAreaPrefab;
    [SerializeField] private float poisonSpawnRate = 5f;

    private Rigidbody2D rb;

    private float poisonTimer;

    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        poisonTimer -= Time.deltaTime;

        if (poisonTimer <= 0)
        {
            SpawnPoisonArea();

            poisonTimer = poisonSpawnRate;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;

        rb.velocity = moveDirection * speed;

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

            Destroy(gameObject);
        }

        //if (collision.CompareTag("Wall"))
        //{
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