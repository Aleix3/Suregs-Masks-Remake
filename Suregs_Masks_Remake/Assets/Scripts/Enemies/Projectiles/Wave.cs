using UnityEngine;

public class Wave : MonoBehaviour
{
    public float expandSpeed = 3f;
    public float maxScale = 5f;
    public float lifeTime = 1f;
    public int damage = 15;

    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        if (transform.localScale.x >= maxScale)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>()?.TakeDamage(damage);
            hasHit = true;
        }
    }
}