using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization; // Nécessaire pour gérer les virgules/points

namespace Hax; 

// ---------------------------------------------------------
// COMMANDE : /speed <nombre> (Accepte 4,3 et 4.3)
// ---------------------------------------------------------
[Command("speed")]
sealed class SpeedCommand : ICommand 
{
    public async Task Execute(Arguments args, CancellationToken cancellationToken) 
    {
        // Si on tape juste "/speed" -> On désactive
        if (args.Length == 0)
        {
            SpeedLogic.IsEnabled = false;
            Print("Speed Mod DÉSACTIVÉ (Retour à la normale).");
            await Task.CompletedTask;
            return;
        }

        // --- CORRECTION MAJEURE ICI ---
        // On prend le texte (ex: "4,3") et on remplace la virgule par un point ("4.3")
        // Cela permet de supporter les claviers FR et US sans bug.
        string inputSpeed = args[0].Replace(',', '.');

        // On essaie de convertir le texte nettoyé en nombre
        if (float.TryParse(inputSpeed, NumberStyles.Any, CultureInfo.InvariantCulture, out float newSpeed))
        {
            SpeedLogic.TargetSpeed = newSpeed;
            SpeedLogic.IsEnabled = true;
            Print($"Speed Mod ACTIVÉ : Vitesse fixée à {newSpeed} (Base: 4.6)");
        }
        else
        {
            Print($"Erreur : Impossible de lire '{args[0]}'. Essayez '/speed 5' ou '/speed 4,5'");
        }

        await Task.CompletedTask;
    }

    private void Print(string message)
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.DisplayTip("SPEED HACK", message, false, false, "LC_Tip1");
        }
    }
}

// ---------------------------------------------------------
// LOGIQUE : Force la vitesse
// ---------------------------------------------------------
public class SpeedLogic : MonoBehaviour 
{
    public static bool IsEnabled = false;
    public static float TargetSpeed = 4.6f; // 4.6f est la marche normale

    void Update() 
    {
        // Sécurité
        if (!IsEnabled || GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) 
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

        if (player.isPlayerDead) return;

        // On applique la vitesse définie
        player.movementSpeed = TargetSpeed;

        // On empêche le poids de l'inventaire de nous ralentir
        // Sinon, même avec une vitesse de 10, porter un objet lourd te ralentirait
        player.carryWeight = 1.0f; 
        
        // Optionnel : Si tu veux que la vitesse soit EXACTEMENT celle demandée
        // même en sprintant (sinon le sprint multiplie ta vitesse par 2.25)
        // Décommente la ligne ci-dessous si tu veux une vitesse constante :
        // player.isSprinting = false; 
    }
}