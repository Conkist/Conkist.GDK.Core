using UnityEngine;
using Conkist.GDK;

namespace Conkist.GDK.Samples
{
    /// <summary>
    /// A simple demo script demonstrating how to trigger popup dialogues and loading overlays.
    /// Place this component on any GameObject in your scene to test the overlays.
    /// </summary>
    [AddComponentMenu("Conkist/Samples/PopupDemoController")]
    public class PopupDemoController : MonoBehaviour
    {
        private void Start()
        {
            // Trigger a standard Popup overlay
            Debug.Log("[PopupDemoController] Triggering ShowPopupEvent...");
            EventManager.TriggerEvent(new ShowPopupEvent(
                title: "Demonstração de Popup",
                message: "Esta é uma mensagem de teste disparada pelo PopupDemoController ao iniciar. Clique em Confirmar ou Cancelar.",
                confirmText: "Confirmar",
                cancelText: "Cancelar",
                onConfirm: () => Debug.Log("[PopupDemoController] Usuário clicou em Confirmar."),
                onCancel: () => Debug.Log("[PopupDemoController] Usuário clicou em Cancelar."),
                isError: false
            ));
        }
    }
}
