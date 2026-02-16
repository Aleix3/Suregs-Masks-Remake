using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private float health = 100;
    public float maxHealth = 100;
    public float speed = 5f;
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float dashDuration = 1f;
    public int swordDamage = 100;

    private Rigidbody2D rb;
    private Vector2 lastMovementDirection;
    private bool isFacingLeft = false;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    [Header("Attack")]
    public GameObject attackHitboxPrefab;
    public float attackWidth = 1f;
    public float attackHeight = 1f;
    public float attackForce = 5f;
    public float attackDuration = 0.2f;

    [Header("Attack Offsets")]
    public Vector2 offsetUp = new Vector2(0, 1f);
    public Vector2 offsetDown = new Vector2(0, -1f);
    public Vector2 offsetLeft = new Vector2(-1f, 0);
    public Vector2 offsetRight = new Vector2(1f, 0);

    private int attackNum = 0;
    private float comboResetTimer = 1f;
    private float comboTimer = 0f;
    private bool isAttacking = false;

    public Image healthBar;

    [SerializeField] private Animator animator;

    public AnimationClip[] attackClips;

    private IInteractable currentInteractable;

    public bool godMode = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerMovement();
        UpdateAttack();
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            ToggleGodMode();
        }

        healthBar.fillAmount = health / maxHealth;

    }

    void UpdatePlayerMovement()
    {
        // Entrada del jugador
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (inputDir != Vector2.zero)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // Movimiento normal
        if (!isDashing)
        {
            rb.velocity = inputDir * speed;

            if (inputDir != Vector2.zero)
            {
                //isFacingLeft = (inputDir.x < 0);
                lastMovementDirection = inputDir;
            }

            if (inputDir.x > 0 && isFacingLeft) Flip();
            else if (inputDir.x < 0 && !isFacingLeft) Flip();
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && inputDir != Vector2.zero)
        {
            if (lastMovementDirection == Vector2.zero)
                lastMovementDirection = Vector2.right; // si no hay dirección, dash hacia la derecha

            rb.AddForce(lastMovementDirection * dashForce, ForceMode2D.Impulse);
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            animator.SetBool("isDashing", true);
        }

        // Control de duración del dash
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                animator.SetBool("isDashing", false);
            }
        }

        // Reducir cooldown
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void UpdateAttack()
    {
        // Reset combo si pasa demasiado tiempo
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                attackNum = 0;
            animator.SetInteger("attackIndex", 0);
        }

        // Input ataque
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            Attack();
            animator.SetTrigger("attackTrigger");
            StartCoroutine(ResetAttackIndex(attackClips[0]));
        }

        
    }

    IEnumerator ResetAttackIndex(AnimationClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        animator.ResetTrigger("attackTrigger");
    }

    void Attack()
    {
        isAttacking = true;

        attackNum++;
        if (attackNum > 3) attackNum = 1;
        comboTimer = comboResetTimer;

        // Detectar dirección y aplicar offset
        Vector2 attackPos = transform.position;

        if (lastMovementDirection == Vector2.zero)
            lastMovementDirection = Vector2.right;

        if (lastMovementDirection.y > 0)       // arriba
            attackPos += offsetUp;
        else if (lastMovementDirection.y < 0)  // abajo
            attackPos += offsetDown;
        else if (lastMovementDirection.x < 0)  // izquierda
            attackPos += offsetLeft;
        else if (lastMovementDirection.x > 0)  // derecha
            attackPos += offsetRight;

        

        // Instanciar el hitbox temporal
        GameObject hitbox = Instantiate(attackHitboxPrefab, attackPos, Quaternion.identity);
        hitbox.transform.localScale = new Vector3(attackWidth, attackHeight, 1f);

        Destroy(hitbox, attackDuration);

        // Empuje hacia la dirección del ataque
        rb.AddForce(lastMovementDirection.normalized * attackForce, ForceMode2D.Impulse);

        animator.SetInteger("attackIndex", attackNum);

        Invoke(nameof(ResetAttack), attackDuration);

        

    }

    void ResetAttack()
    {
        isAttacking = false;
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    protected void Flip()
    {
        isFacingLeft = !isFacingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //INTERACTUABLES
        var interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
            
        //ITEMS
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            InventoryManager.instance.CreateInventoryItem(item.type,item.itemType, item.itemName, item.description, item.sr.sprite);
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
            currentInteractable = null;
    }

    private void ToggleGodMode()
    {
        godMode = !godMode;
        this.GetComponent<CircleCollider2D>().isTrigger = !this.GetComponent<CircleCollider2D>().isTrigger;


    }

    public float GetHealth()
    {
        return health;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetSwordDamage()
    {
        return swordDamage;
    }

    public void UpgradeSword(int amount)
    {
        swordDamage += amount;
        Debug.Log("Espada mejorada. Nuevo daño: " + swordDamage);
    }

    public void UpgradeArmor(float amount)
    {
        maxHealth += amount;
        health += amount; // opcional: también cura la nueva vida añadida

        if (health > maxHealth)
            health = maxHealth;

        Debug.Log("Armadura mejorada. Nueva vida máxima: " + maxHealth);
    }

    public void Heal(float amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;
    }

}
