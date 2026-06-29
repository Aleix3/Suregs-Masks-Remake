using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = Player.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.isTrigger)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(player.swordDamage);
                player.MaskManager.NotifyBasicAttack();

                if (player.MaskManager.Secondary is MaskInuit inuit)
                    inuit.TriggerPassiveWave();

                // Romper invisibilidad de Musri si es primaria o secundaria
                //if (player.MaskManager.Primary is MaskMusri musriP) musriP.OnPlayerAttackedWhileInvisible();
                //if (player.MaskManager.Secondary is MaskMusri musriS) musriS.OnPlayerAttackedWhileInvisible();
            }
        }
    }
}
