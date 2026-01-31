using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hax;

[Command("trap")]
sealed class TrapCommand : ICommand 
{
    public async Task Execute(Arguments args, CancellationToken cancellationToken) 
    {
        // 1. GESTION ON/OFF
        if (args.Length == 0)
        {
            TrapLogic.TargetPlayer = null;
            HUDManager.Instance.DisplayTip("TRAP MOD", "DÉSACTIVÉ. Libération.", false, false, "LC_Tip1");
            await Task.CompletedTask;
            return;
        }

        // 2. RECHERCHE DU JOUEUR
        string targetName = args[0].ToLower();
        PlayerControllerB foundPlayer = null;

        foreach (var player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player.playerUsername.ToLower().Contains(targetName))
            {
                foundPlayer = player;
                break;
            }
        }

        if (foundPlayer == null)
        {
            HUDManager.Instance.DisplayTip("ERREUR", $"Joueur '{targetName}' introuvable !", true, false, "LC_Tip1");
            return;
        }

        // 3. ACTIVATION
        TrapLogic.TargetPlayer = foundPlayer;
        HUDManager.Instance.DisplayTip("TRAP MOD", $"CIBLE : {foundPlayer.playerUsername}", false, false, "LC_Tip1");
        
        // Debug immédiat pour voir si on trouve des objets
        TrapLogic.DebugScan();

        await Task.CompletedTask;
    }
}

public class TrapLogic : MonoBehaviour 
{
    public static PlayerControllerB TargetPlayer = null;
    private float _debugTimer = 0f;

    // Fonction pour afficher ce qu'on trouve au moment de la commande
    public static void DebugScan()
    {
        var props = Object.FindObjectsOfType<AutoParentToShip>(true);
        var items = Object.FindObjectsOfType<GrabbableObject>();
        HUDManager.Instance.DisplayTip("SCAN", $"Meubles: {props.Length} | Items: {items.Length}", false, false, "LC_Tip1");
    }

    void LateUpdate() 
    {
        if (TargetPlayer == null || TargetPlayer.isPlayerDead) return;

        Vector3 targetPos = TargetPlayer.transform.position;

        // -----------------------------------------------------------------------
        // TYPE 1 : LES MEUBLES (Via AutoParentToShip)
        // C'est plus fiable que PlaceableShipObject car TOUT dans le vaisseau a ça
        // -----------------------------------------------------------------------
        AutoParentToShip[] shipObjects = Object.FindObjectsOfType<AutoParentToShip>();
        
        foreach (var obj in shipObjects)
        {
            // On ignore les objets désactivés
            if (obj == null || !obj.gameObject.activeInHierarchy) continue;

            // On ne bouge pas le vaisseau lui-même ou les murs fixes
            // On filtre pour ne prendre que ce qui semble être un meuble ou un décor
            if (obj.transform.parent == null) continue; // Sécurité

            // Astuce : On décale l'objet pour qu'il soit SUR le joueur
            // On utilise l'ID pour que chaque objet ait sa propre place stable
            float offset = (obj.GetInstanceID() % 10) / 5f; 
            
            // FORCE BRUTE POSITION
            obj.transform.position = targetPos + new Vector3(0, 0.5f + offset, 0);
            obj.transform.LookAt(targetPos);
        }

        // -----------------------------------------------------------------------
        // TYPE 2 : LES ITEMS (Loot, Outils)
        // -----------------------------------------------------------------------
        GrabbableObject[] items = Object.FindObjectsOfType<GrabbableObject>();

        foreach (var item in items)
        {
            // On ne touche pas ce qui est tenu par le joueur cible (sinon bug caméra)
            if (item == null || item.playerHeldBy == TargetPlayer) continue;

            // On vérifie si l'objet est dans le vaisseau (distance < 20m du centre)
            if (StartOfRound.Instance.elevatorTransform != null)
            {
                float dist = Vector3.Distance(item.transform.position, StartOfRound.Instance.elevatorTransform.position);
                if (dist > 25f) continue; // Trop loin (dans l'usine), on ignore
            }

            // On téléporte
            item.transform.position = targetPos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.2f, 1.5f), Random.Range(-0.5f, 0.5f));

            // On coupe la physique pour qu'ils ne volent pas
            if (item.GetComponent<Rigidbody>() is Rigidbody rb)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
}