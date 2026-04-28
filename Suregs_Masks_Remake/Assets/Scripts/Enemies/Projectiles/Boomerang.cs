using System.Collections;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public float speed = 8f;
    public float returnSpeed = 10f;
    public float lifeTime = 3f;

    private Transform target;
    private bool returning = false;
    private Rigidbody2D rb;

    public int damage = 10;

    public Animator animator;

    public void Init(Transform boss, Vector2 direction)
    {
        target = boss;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction * speed;

        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        yield return new WaitForSeconds(1f);
        returning = true;
    }

    void Update()
    {
        if (returning && target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            rb.velocity = dir * returnSpeed;

            if (Vector2.Distance(transform.position, target.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>()?.TakeDamage(damage);
        }
    }
}