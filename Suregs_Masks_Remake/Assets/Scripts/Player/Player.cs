using Cinemachine;
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

    [Header("Vacios (huecos en el suelo)")]
    public float fallRotationSpeed = 900f; 
    public float fallDuration = 0.6f;      
    private bool isFallingIntoVoid = false;
    public bool IsFallingIntoVoid => isFallingIntoVoid;
    private Vector3 scaleBeforeFalling;
    private bool restoreScaleAfterFall;
    private Vector3 baseScale;
    public CircleCollider2D colliderPlayerNoTrigger;
    private CinemachineConfiner2D cinemachineConfiner;
    private PolygonCollider2D initialRoomCollider;


    private readonly HashSet<object> movementLocks = new HashSet<object>();

    public bool canMove => movementLocks.Count == 0;


    public void LockMovement(object source)
    {
        if (source == null) return;
        movementLocks.Add(source);
    }


    public void UnlockMovement(object source)
    {
        if (source == null) return;
        movementLocks.Remove(source);
    }

    public void ClearAllMovementLocks()
    {
        movementLocks.Clear();
    }

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

    public bool isDead = false;

    private bool lowHealthSoundPlaying = false;
    private const float LOW_HEALTH_THRESHOLD = 0.25f;
    private float footstepTimer = 0f;
    [SerializeField] private float footstepInterval = 0.15f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Guardamos la escala "normal" del jugador para poder restaurarla
        // tras la animacion de caida en un vacio (o cualquier otra que la modifique)
        baseScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));

        SceneManager.sceneLoaded += OnSceneLoaded;

        MaskManager = GetComponent<MaskManager>();

        LoadUpgrades();
    }

    private const string WEAPON_LEVEL_KEY = "Player_WeaponLevel";
    private const string ARMOR_LEVEL_KEY = "Player_ArmorLevel";

    // Carga el nivel de arma/armadura guardado (si no hay nada guardado, se queda en el valor por defecto del Inspector)
    private void LoadUpgrades()
    {
        weaponLevel = PlayerPrefs.GetInt(WEAPON_LEVEL_KEY, weaponLevel);
        armorLevel = PlayerPrefs.GetInt(ARMOR_LEVEL_KEY, armorLevel);

        if (weaponLevel > 1)
            baseSwordDamage = PlayerStatsTable.GetWeaponDamage(weaponLevel);

        if (armorLevel > 1)
            baseMaxHealth = PlayerStatsTable.GetArmorHealth(armorLevel);
    }

    // Guarda el nivel actual de arma/armadura para que persista al recargar escena o reiniciar el juego
    private void SaveUpgrades()
    {
        PlayerPrefs.SetInt(WEAPON_LEVEL_KEY, weaponLevel);
        PlayerPrefs.SetInt(ARMOR_LEVEL_KEY, armorLevel);
        PlayerPrefs.Save();
    }

    void ResetPlayerState()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.rotation = 0f;
        }

        transform.rotation = Quaternion.identity;

        if (restoreScaleAfterFall)
        {
            transform.localScale = scaleBeforeFalling;
            restoreScaleAfterFall = false;
        }

        isFallingIntoVoid = false;

        ClearAllMovementLocks();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        cinemachineConfiner = FindFirstObjectByType<CinemachineConfiner2D>();

        initialRoomCollider = (PolygonCollider2D)cinemachineConfiner.m_BoundingShape2D;
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

        ClearAllMovementLocks(); // por si acaso venimos de una recarga de escena con locks colgados

        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.Save();
    }

    void Update()
    {
        if (isDead)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if (isFallingIntoVoid)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        UpdateKnockback();
        UpdatePlayerMovement();
        UpdateAttack();
        UpdateLowHealthSound();
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

        //if (isAttacking)
        //{
        //    rb.velocity = Vector2.zero;
        //    return;
        //}

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
        // Movimiento normal
        if (!isDashing)
        {
            rb.velocity = inputDir * Speed;

            if (inputDir != Vector2.zero)
            {
                footstepTimer -= Time.deltaTime;

                if (footstepTimer <= 0f)
                {
                    AudioManager.Instance.PlayRandomFootstep();
                    footstepTimer = footstepInterval;
                }

                lastMovementDirection = inputDir;
            }
            else
            {
                footstepTimer = 0f;
            }

            if (inputDir.x > 0 && isFacingLeft) Flip();
            else if (inputDir.x < 0 && !isFacingLeft) Flip();
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && inputDir != Vector2.zero)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.dash);
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

        // Reproducir sonido según el combo
        switch (attackNum)
        {
            case 1:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.attackCombo1);
                break;
            case 2:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.attackCombo2);
                break;
            case 3:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.attackCombo3);
                break;
        }

        Vector2 attackPos = transform.position;

        if (lastMovementDirection == Vector2.zero)
            lastMovementDirection = Vector2.right;

        if (lastMovementDirection.y > 0)
            attackPos += offsetUp;
        else if (lastMovementDirection.y < 0)
            attackPos += offsetDown;
        else if (lastMovementDirection.x < 0)
            attackPos += offsetLeft;
        else if (lastMovementDirection.x > 0)
            attackPos += offsetRight;

        // Instanciar el hitbox temporal
        GameObject hitbox = Instantiate(attackHitboxPrefab, attackPos, Quaternion.identity);
        hitbox.transform.localScale = new Vector3(attackWidth, attackHeight, 1f);

        Destroy(hitbox, attackDuration);

        // Empuje hacia la dirección del ataque
        rb.AddForce(lastMovementDirection.normalized * attackForce, ForceMode2D.Impulse);

        animator.SetInteger("attackIndex", attackNum);
        animator.SetTrigger("attackTrigger");

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

    public void DamageToPercent(float percent)
    {
        int damage = Mathf.RoundToInt(MaxHealth * percent);
        TakeDamage(damage);
    }
    public void TakeDamage(int damage)
    {
        if (godMode || isDead)
            return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.getDamage);
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
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
            AudioManager.Instance.PlaySFX(AudioManager.Instance.getItem);
            Item item = collision.GetComponent<Item>();
            InventoryManager.instance.CreateInventoryItem(item.type, item.itemType, item.itemName, item.description, item.sr.sprite);
            Destroy(collision.gameObject);
        }

        //NOTES
        if (collision.CompareTag("Note"))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.getItem);
            Note note = collision.GetComponent<Note>();
            NotesManager.instance.CreateNoteItem(note.id, note.itemName, note.description);
            Destroy(collision.gameObject);
        }

        //MASKS
        if (collision.CompareTag("Mask"))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.getItem);
            collision.GetComponent<MaskItem>().GetMask();
        }

        if (isDashing)
            return;
        if (!colliderPlayerNoTrigger.IsTouching(collision))
            return;
        //VACIOS (huecos en el suelo)
        if (collision.gameObject.CompareTag("Void"))
        {
            FallIntoVoid();
        }
        


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
            currentInteractable = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
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

        SaveUpgrades();

        Debug.Log($"Weapon level: {level} | Damage: {SwordDamage}");
    }

    public void UpgradeArmor(int level)
    {
        armorLevel = level;

        float previousMax = MaxHealth;

        baseMaxHealth = PlayerStatsTable.GetArmorHealth(level);

        health = (health / previousMax) * MaxHealth;

        SaveUpgrades();

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

    public void UsePotion()
    {
        animator.SetTrigger("usePotion");
    }

    public void UseMaskSkill()
    {
        animator.SetTrigger("useMask");
    }

    public void Die()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.death);
        isDead = true;
        QuestManager.Instance.CompleteMainStepById("2");
        animator.SetTrigger("die");
        StartCoroutine(DieRoutine());
    }

    public IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(0.833f);
        yield return StartCoroutine(CameraManager.Instance.Fade(1));
        if (Player.Instance.isFacingLeft)
        {
            transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        }
        else
        {
            transform.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
        }
        Heal(MaxHealth);
        isDead = false;
        SceneManager.LoadScene("Town");

    }

    // Vacios / huecos en el suelo: gira y encoge al jugador y recarga la escena actual
    public void FallIntoVoid()
    {
        if (isDead || isFallingIntoVoid) return;

        scaleBeforeFalling = transform.localScale;
        restoreScaleAfterFall = true;

        StartCoroutine(FallIntoVoidRoutine());
    }

    private IEnumerator FallIntoVoidRoutine()
    {
        
        isFallingIntoVoid = true;

        LockMovement(this);
        isDashing = false;
        animator.SetBool("isRunning", false);
        animator.SetBool("isDashing", false);
        rb.velocity = Vector2.zero;

        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            transform.Rotate(
                0f,
                0f,
                fallRotationSpeed * Time.deltaTime
            );

            transform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                t
            );

            yield return null;
        }

        transform.localScale = Vector3.zero;

        DamageToPercent(0.15f);

        godMode = true;

        yield return StartCoroutine(CameraManager.Instance.Fade(2, 1));

        cinemachineConfiner = FindFirstObjectByType<CinemachineConfiner2D>();

        cinemachineConfiner.m_BoundingShape2D = initialRoomCollider;

        actualRoom.isPlayerInRoom = false;

        actualRoom = initialRoomCollider.transform.parent.GetComponent<Room>();

        StartCoroutine(SetSpawn());

        yield return StartCoroutine(CameraManager.Instance.Fade(0, 1));

        godMode = false;

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }



    void UpdateLowHealthSound()
    {
        bool lowHealth = health <= MaxHealth * LOW_HEALTH_THRESHOLD;

        if (lowHealth && !lowHealthSoundPlaying && !isDead)
        {
            lowHealthSoundPlaying = true;
            StartCoroutine(LowHealthSoundRoutine());
        }
    }

    IEnumerator LowHealthSoundRoutine()
    {
        while (health <= MaxHealth * LOW_HEALTH_THRESHOLD && !isDead)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.lowHealth);

            yield return new WaitForSeconds(AudioManager.Instance.lowHealth.length);
        }

        lowHealthSoundPlaying = false;
    }
}