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
                // Árbol 0 Musri — bonus de daño en el primer golpe invisible
                float firstHitBonus = 0f;
                if (player.MaskManager?.Primary is MaskMusri musriPA) firstHitBonus = musriPA.ConsumeFirstHitBonus();
                if (player.MaskManager?.Secondary is MaskMusri musriSA) firstHitBonus += musriSA.ConsumeFirstHitBonus();
                int finalDamage = Mathf.RoundToInt(player.SwordDamage * (1f + firstHitBonus));
                enemy.TakeDamage(finalDamage);
                player.MaskManager.NotifyBasicAttack();

                if (player.MaskManager.Secondary is MaskInuit inuit)
                    inuit.TriggerPassiveWave(enemy.transform);

                // Romper invisibilidad de Musri al atacar
                (player.MaskManager?.Primary as MaskMusri)?.OnPlayerAttacked();
                (player.MaskManager?.Secondary as MaskMusri)?.OnPlayerAttacked();
            }
        }
    }
}
