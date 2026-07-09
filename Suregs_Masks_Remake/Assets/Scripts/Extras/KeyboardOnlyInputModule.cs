using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Reemplaza el StandaloneInputModule ignorando completamente el ratón.
/// 
/// SETUP:
///   1. En el GameObject del EventSystem, elimina el componente StandaloneInputModule.
///   2. Añade este componente en su lugar.
///   3. Configura Horizontal Axis = "Horizontal", Vertical Axis = "Vertical",
///      Submit Button = "Submit", Cancel Button = "Cancel" igual que antes.
/// </summary>
public class KeyboardOnlyInputModule : StandaloneInputModule
{
    public override void Process()
    {
        // Procesar solo teclado y mando — ignorar ratón completamente
        bool usedEvent = SendUpdateEventToSelectedObject();

        if (!usedEvent)
            SendMoveEventToSelectedObject();

        SendSubmitEventToSelectedObject();
    }
}
