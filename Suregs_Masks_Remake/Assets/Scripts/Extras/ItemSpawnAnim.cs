using UnityEngine;
using System.Collections;

public class ItemSpawnAnim : MonoBehaviour
{
    private CircleCollider2D circleCollider2D;
    public IEnumerator Start()
    {
        circleCollider2D = this.GetComponent<CircleCollider2D>();
        circleCollider2D.enabled = false;
        Vector2 velocity = new Vector2(
            Random.Range(-2f, 2f),
            Random.Range(4f, 6f)
        );

        float gravity = -18f;
        float duration = 0.5f;
        float timer = 0f;

        Vector3 pos = transform.position;

        while (timer < duration)
        {
            // Actualiza
            velocity.y += gravity * Time.deltaTime;
            pos += (Vector3)(velocity * Time.deltaTime);
            transform.position = pos;

            timer += Time.deltaTime;
            yield return null;
        }
        circleCollider2D.enabled = true;
        Destroy(this);
    }
}
