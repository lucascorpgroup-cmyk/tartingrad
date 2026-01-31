using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using System.Threading;
using System.Threading.Tasks;

namespace Hax;

// ---------------------------------------------------------
// COMMANDE : /disco
// ---------------------------------------------------------
[Command("disco")]
sealed class DiscoCommand : ICommand 
{
    public async Task Execute(Arguments args, CancellationToken cancellationToken) 
    {
        DiscoLogic.IsEnabled = !DiscoLogic.IsEnabled;

        string status = DiscoLogic.IsEnabled ? "ACTIVÉ (Épilepsie Mode)" : "DÉSACTIVÉ";
        
        if (HUDManager.Instance != null)
            HUDManager.Instance.DisplayTip("Disco Mod", status, false, false, "LC_Tip1");

        await Task.CompletedTask;
    }
}

// ---------------------------------------------------------
// LOGIQUE : Bypass Cooldown & Spam
// ---------------------------------------------------------
public class DiscoLogic : MonoBehaviour 
{
    public static bool IsEnabled = false;
    private float _timer = 0f;
    
    // Vitesse du stroboscope
    private float _delay = 0.1f; 

    void Update() 
    {
        if (!IsEnabled) return;

        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            return;

        _timer += Time.deltaTime;

        if (_timer >= _delay)
        {
            _timer = 0f;
            ForceFlashlightToggle();
        }
    }

    void ForceFlashlightToggle()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        GrabbableObject heldObject = player.currentlyHeldObjectServer;

        // Est-ce une lampe torche ?
        if (heldObject != null && heldObject is FlashlightItem flashlight)
        {
            // A-t-elle de la batterie ?
            if (flashlight.insertedBattery.charge > 0)
            {
                // --- CORRECTION ---
                // Le "useCooldown" est directement sur l'objet (GrabbableObject), 
                // pas dans ses propriétés (Item).
                // On le met à 0 pour autoriser le spam immédiat.
                if (flashlight.useCooldown > 0f)
                {
                    flashlight.useCooldown = 0f;
                }

                // Simulation du clic
                // True = bouton enfoncé
                flashlight.UseItemOnClient(true);
            }
        }
    }
}