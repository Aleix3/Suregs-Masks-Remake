using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ols_Projectile : MonoBehaviour
{
    public int damage;
    void Start()
    {
        Destroy(this.gameObject, 4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
                Destroy(this.gameObject);
        }
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                print(damage);
                Destroy(this.gameObject);
            }
        }
    }
}
