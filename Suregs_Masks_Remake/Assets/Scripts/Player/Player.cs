using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Stats Base")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private int baseSwordDamage = 100;
    [SerializeField] private float baseMaxHealth = 100f;

    public float Speed => baseSpeed * SpeedMultiplier;
    public int SwordDamage => Mathf.RoundToInt(baseSwordDamage * BasicDamageMultiplier);
    public float MaxHealth => baseMaxHealth * MaxHealthMultiplier;

    [SerializeField] private float health = 100f;

    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float dashDuration = 1f;


    private Rigidbody2D rb;
    public Vector2 lastMovementDirection;
    public bool isFacingLeft = false;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    public bool canMove = true;

    [SerializeField] public Room actualRoom;

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

    [Header("Mejoras")]
    public int weaponLevel = 1;
    public int armorLevel = 1;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    // Multiplicadores (las m�scaras los suman/restan)
    [HideInInspector] public float BasicDamageMultiplier = 1f;
    [HideInInspector] public float SpeedMultiplier = 1f;
    [HideInInspector] public float MaxHealthMultiplier = 1f;

    [Header("Mascaras")]
    public float basicAttackBonusPercent = 0.15f;
    public bool BonusBasicDamageActive { get; set; } = false;
    public MaskManager MaskManager { get; private set; }

    public bool spawnPointChanged = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        MaskManager = GetComponent<MaskManager>();
    }

    void ResetPlayerState()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetSpawn());
    }

    System.Collections.IEnumerator SetSpawn()
    {
        yield return null;

        GameObject spawn = GameObject.FindWithTag("SpawnPoint");

        if (spawn != null)
        {
            ResetPlayerState();
            transform.position = spawn.transform.position;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        health = Mathf.Clamp(health, 0, MaxHealth);
        rb = GetComponent<Rigidbody2D>();
        health = MaxHealth;

        canMove = true;

        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.Save();
    }

    void Update()
    {
        UpdateKnockback();
        UpdatePlayerMovement();
        UpdateAttack();
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && !UIState.IsUIOpen)
        {
            currentInteractable.Interact(this);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            ToggleGodMode();
        }

        //healthBar.fillAmount = health / MaxHealth;

    }

    void UpdatePlayerMovement()
    {
        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            animator.SetBool("isRunning", false);
        }

        // Durante el ataque, el personaje queda "anclado": no aceptamos input
        // de movimiento ni tocamos isRunning, para que solo la animaci�n
        // de ataque est� activa en el Animator (evita el solape de estados).
        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isKnockedBack || !canMove)
            return;

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
            rb.velocity = inputDir * Speed;

            if (inputDir != Vector2.zero)
            {
                lastMovementDirection = inputDir;
            }

            if (inputDir.x > 0 && isFacingLeft) Flip();
            else if (inputDir.x < 0 && !isFacingLeft) Flip();
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && inputDir != Vector2.zero)
        {
            if (lastMovementDirection == Vector2.zero)
                lastMovementDirection = Vector2.right; // si no hay direcci�n, dash hacia la derecha

            // �rbol 2 Musri � bonus de distancia durante invisibilidad activa
            float dashBonus = 0f;
            if (MaskManager?.Primary is MaskMusri musriP) dashBonus = musriP.GetDashBonus();
            if (MaskManager?.Secondary is MaskMusri musriS) dashBonus = musriS.GetDashBonus();
            rb.AddForce(lastMovementDirection * dashForce * (1f + dashBonus), ForceMode2D.Impulse);
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            animator.SetBool("isDashing", true);

            // Pasiva Musri: dash normal, invisibilidad
            (MaskManager?.Primary as MaskMusri)?.OnPlayerDash();
            (MaskManager?.Secondary as MaskMusri)?.OnPlayerDash();
        }

        // Control de duraci�n del dash
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
        // Reset combo si pasa demasiado tiempo (solo cuando expira, no cada frame)
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                attackNum = 0;
                animator.SetInteger("attackIndex", 0);
            }
        }

        // Input ataque - se ignora mientras isAttacking sea true,
        // y ahora isAttacking dura lo mismo que la animación real (ver Attack())
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            Attack();
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

        // Detectar direcci�n y aplicar offset
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

        // Empuje hacia la direcci�n del ataque
        rb.AddForce(lastMovementDirection.normalized * attackForce, ForceMode2D.Impulse);

        animator.SetInteger("attackIndex", attackNum);
        animator.SetTrigger("attackTrigger");

        // Duracion del "lock" de ataque = duracion real de la animacion de ESTE golpe
        // del combo, no el valor fijo attackDuration (que solo controla la vida del hitbox).
        AnimationClip clip = (attackClips != null && attackClips.Length >= attackNum)
            ? attackClips[attackNum - 1]
            : null;
        float lockDuration = clip != null ? clip.length : attackDuration;

        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(ResetAttack), lockDuration);

        StopCoroutine(nameof(ResetAttackIndex));
        StartCoroutine(ResetAttackIndex(clip != null ? clip : attackClips[0]));
    }

    void ResetAttack()
    {
        isAttacking = false;

    }

    /// Musri: activa y desactiva invisibilidad visual e invulnerabilidad
    /// Tambi�n para y reanuda los enemigos de la sala

    public void SetInvisible(bool invisible)
    {

        godMode = invisible;


        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.color = invisible ? new Color(1f, 1f, 1f, 0.3f) : Color.white;


        if (actualRoom != null)
        {
            foreach (Enemy e in actualRoom.enemiesInRoom)
            {
                if (e == null || e.isDead) continue;
                e.SetFrozen(invisible);
            }
        }
    }

    //Surma: recorta la vida al nuevo m�ximo (tras retirar buff)
    public void ClampHealthToMax()
    {
        if (health > MaxHealth) health = MaxHealth;
    }

    //Surma: rellena vida hasta el porcentaje del m�ximo actual
    public void HealToPercent(float percent)
    {
        health = Mathf.Min(MaxHealth, MaxHealth * percent);
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
            InventoryManager.instance.CreateInventoryItem(item.type, item.itemType, item.itemName, item.description, item.sr.sprite);
            Destroy(collision.gameObject);
        }

        //NOTES
        if (collision.CompareTag("Note"))
        {
            Note note = collision.GetComponent<Note>();
            NotesManager.instance.CreateNoteItem(note.id, note.itemName, note.description);
            Destroy(collision.gameObject);
        }

        //MASKS
        if (collision.CompareTag("Mask"))
        {
            collision.GetComponent<MaskItem>().GetMask();
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
        return MaxHealth;
    }

    public int GetSwordDamage()
    {
        return SwordDamage;
    }

    public void UpgradeSword(int level)
    {
        weaponLevel = level;

        baseSwordDamage = PlayerStatsTable.GetWeaponDamage(level);

        Debug.Log($"Weapon level: {level} | Damage: {SwordDamage}");
    }

    public void UpgradeArmor(int level)
    {
        armorLevel = level;

        float previousMax = MaxHealth;

        baseMaxHealth = PlayerStatsTable.GetArmorHealth(level);

        health = (health / previousMax) * MaxHealth;

        Debug.Log($"Armor level: {level} | MaxHealth: {MaxHealth}");
    }

    public void Heal(float amount)
    {
        health += amount;

        if (health > MaxHealth)
            health = MaxHealth;
    }

    // Cura un porcentaje del MaxHealth actual (ej. 0.2f = 20%)
    public void HealPercentOfMax(float percent)
    {
        Heal(MaxHealth * percent);
    }

    // Cura gradualmente un porcentaje del MaxHealth a lo largo de "duration" segundos
    public void HealOverTime(float percentOfMax, float duration)
    {
        StartCoroutine(HealOverTimeRoutine(percentOfMax, duration));
    }

    private IEnumerator HealOverTimeRoutine(float percentOfMax, float duration)
    {
        float totalHeal = MaxHealth * percentOfMax;
        float elapsed = 0f;
        float healedSoFar = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float target = Mathf.Lerp(0f, totalHeal, elapsed / duration);
            float delta = target - healedSoFar;

            if (delta > 0f)
            {
                Heal(delta);
                healedSoFar += delta;
            }

            yield return null;
        }
    }

    // Aumenta BasicDamageMultiplier durante "duration" segundos y luego lo revierte
    public void ApplyTemporaryDamageBuff(float bonusPercent, float duration)
    {
        StartCoroutine(TemporaryDamageBuffRoutine(bonusPercent, duration));
    }

    private IEnumerator TemporaryDamageBuffRoutine(float bonusPercent, float duration)
    {
        BasicDamageMultiplier += bonusPercent;
        yield return new WaitForSeconds(duration);
        BasicDamageMultiplier -= bonusPercent;
    }

    // Aumenta SpeedMultiplier durante "duration" segundos y luego lo revierte
    public void ApplyTemporarySpeedBuff(float bonusPercent, float duration)
    {
        StartCoroutine(TemporarySpeedBuffRoutine(bonusPercent, duration));
    }

    private IEnumerator TemporarySpeedBuffRoutine(float bonusPercent, float duration)
    {
        SpeedMultiplier += bonusPercent;
        yield return new WaitForSeconds(duration);
        SpeedMultiplier -= bonusPercent;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;

        knockbackTimer = duration;

        rb.velocity = direction.normalized * force;
    }
    void UpdateKnockback()
    {
        if (!isKnockedBack)
            return;

        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0f)
        {
            isKnockedBack = false;

            rb.velocity = Vector2.zero;
        }
    }
}