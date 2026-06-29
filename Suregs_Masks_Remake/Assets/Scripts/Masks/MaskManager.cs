using UnityEngine;


public class MaskManager : MonoBehaviour
{

    [Header("Máscaras disponibles")]
    public BaseMask slotPrimary;
    public BaseMask slotSecondary;

    [Header("Input")]
    public KeyCode  swapKey         = KeyCode.R;
    public string   swapButton      = "Y"; 


    public BaseMask Primary   { get; private set; }
    public BaseMask Secondary { get; private set; }


    public System.Action<BaseMask, BaseMask> OnSwap; 


    private void Start()
    {
        SetPrimary(slotPrimary, slotSecondary);
    }


    private void Update()
    {
        HandleSwapInput();
        HandleActivateInput();

        Secondary?.ApplyPassive();
    }


    private void HandleSwapInput()
    {
        bool pressedKey    = Input.GetKeyDown(swapKey);
        bool pressedButton = false;

        try { pressedButton = Input.GetButtonDown(swapButton); }
        catch { }

        if (pressedKey || pressedButton)
            Swap();
    }

    private void HandleActivateInput()
    {

        if (Input.GetKeyDown(KeyCode.F))
            Primary?.TryActivate();
    }

    public void Swap()
    {
        if (Primary == null || Secondary == null) return;

        // Quitar pasiva de la secundaria antes de intercambiar
        Secondary.RemovePassive();

        (Primary, Secondary) = (Secondary, Primary);

        OnSwap?.Invoke(Primary, Secondary);
        Debug.Log($"[MaskManager] Swap → Primaria: {Primary.data.maskName} | Secundaria: {Secondary.data.maskName}");
    }

    public void SetPrimary(BaseMask primary, BaseMask secondary)
    {
        // Limpiar estado previo
        Secondary?.RemovePassive();

        Primary   = primary;
        Secondary = secondary;

        OnSwap?.Invoke(Primary, Secondary);
    }


    public void NotifyBasicAttack()
    {
        Primary?.OnBasicAttack();
    }

    public void GrantKillXP(float amount)
    {
        Primary?.AddXP(amount);
        Secondary?.AddXP(amount);
    }
}
