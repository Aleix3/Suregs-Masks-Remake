using UnityEngine;


/// Componente reutilizable: se añade a CUALQUIER objeto que implemente IInteractable
/// (Chest, puertas, NPCs, etc.) y se encarga de:
///  - Detectar cuándo el player entra/sale de un radio de interacción.
///  - Mostrar/ocultar un sprite encima del objeto.

[RequireComponent(typeof(CircleCollider2D))]
public class InteractionPrompt : MonoBehaviour
{

    public GameObject promptSprite;

    public float radius = 1.5f;

    public string playerTag = "Player";

    [Header("Animación de levitación")]
    [Tooltip("Cuánto sube y baja el sprite")]
    public float floatAmplitude = 0.1f;

    public float floatSpeed = 2f;

    private CircleCollider2D detectionTrigger;
    private IInteractable interactable;
    private Vector3 promptBasePos;
    private float floatTimer;

    public bool PlayerInRange { get; private set; }

    private void Awake()
    {
        //interactable = GetComponent<IInteractable>();
        //if (interactable == null)
        //{
        //    Debug.LogWarning($"{name}: InteractionPrompt está en un objeto sin componente IInteractable.");
        //}

        detectionTrigger = GetComponent<CircleCollider2D>();
        detectionTrigger.isTrigger = true;
        detectionTrigger.radius = radius;

        if (promptSprite != null)
        {
            promptBasePos = promptSprite.transform.localPosition;
            promptSprite.SetActive(false);
        }
    }

    private void Update()
    {
        if (promptSprite == null || !promptSprite.activeSelf) return;


        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
        promptSprite.transform.localPosition = promptBasePos + new Vector3(0f, yOffset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        if (!other.CompareTag(playerTag)) return;

        PlayerInRange = true;
        if (promptSprite != null)
        {
            floatTimer = 0f;
            promptSprite.transform.localPosition = promptBasePos;
            promptSprite.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerInRange = false;
        if (promptSprite != null)
        {
            promptSprite.SetActive(false);
            promptSprite.transform.localPosition = promptBasePos;
        }
    }
    //public IInteractable GetInteractable() => interactable;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}