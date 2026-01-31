using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using System.Threading;
using System.Threading.Tasks;

// ---------------------------------------------------------
// PARTIE 1 : LA COMMANDE (/stamina)
// C'est le format spécifique demandé par ton injecteur
// ---------------------------------------------------------
[Command("stamina")]
sealed class StaminaCommand : ICommand 
{
    public async Task Execute(Arguments args, CancellationToken cancellationToken) 
    {
        // On inverse l'état (ON / OFF)
        StaminaLogic.IsEnabled = !StaminaLogic.IsEnabled;

        // Feedback visuel pour dire si c'est activé ou non
        string status = StaminaLogic.IsEnabled ? "ACTIVÉ" : "DÉSACTIVÉ";
        
        // On affiche une notification en bas de l'écran (HUD du jeu)
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.DisplayTip("Stamina Mod", $"Endurance illimitée : {status}", false, false, "LC_Tip1");
        }
        
        // Petite pause asynchrone pour respecter la signature Task
        await Task.CompletedTask;
    }
}

// ---------------------------------------------------------
// PARTIE 2 : LA LOGIQUE (La boucle infinie)
// Ce script tourne en fond et applique la stamina
// ---------------------------------------------------------
public class StaminaLogic : MonoBehaviour 
{
    // Variable statique accessible par la commande ci-dessus
    public static bool IsEnabled = false;

    void Update() 
    {
        // Si la commande n'a pas activé le cheat, on ne fait rien
        if (!IsEnabled) return;

        // Si le joueur est valide, on remplit la stamina
        if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null) 
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            
            player.isSpeedCheating = false;
            player.isSprinting = false; // Empêche le jeu de consommer
            player.isExhausted = false;
            player.sprintMeter = 1.0f;  // Force la barre à 100%
        }
    }
}