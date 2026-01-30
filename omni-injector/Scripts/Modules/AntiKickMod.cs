using System.Collections;
using UnityEngine;
using System.Linq;

sealed class AntiKickMod : MonoBehaviour {
    // Variables locales pour éviter les erreurs de compilation si elles manquent dans Setting
    static string? IdentityToCopy = null;
    static bool HideName = false;
    
    bool isRejoining = false;

    void OnEnable() {
        InputListener.OnBackslashPress += this.ToggleAntiKick;
        GameListener.OnGameStart += this.OnGameStart;
        GameListener.OnGameEnd += this.OnGameEnd;
    }

    void OnDisable() {
        InputListener.OnBackslashPress -= this.ToggleAntiKick;
        GameListener.OnGameStart -= this.OnGameStart;
        GameListener.OnGameEnd -= this.OnGameEnd;
    }

    void OnGameEnd() {
        // Ne pas intervenir si la déconnexion est volontaire
        if (State.DisconnectedVoluntarily) return;
        if (!Setting.EnableAntiKick) return;
        if (this.isRejoining) return;

        // Étape 1 : Déconnexion forcée pour nettoyer le cache réseau
        if (Helper.GameNetworkManager != null) {
            Chat.Print("Anti-Kick: Kick détecté. Reconnexion automatique lancée...");
            Helper.GameNetworkManager.Disconnect();
        }

        this.isRejoining = true;
        _ = this.StartCoroutine(this.InstantRejoin());
    }

    IEnumerator InstantRejoin() {
        if (State.ConnectedLobby is not ConnectedLobby connectedLobby) {
            this.isRejoining = false;
            yield break;
        }

        // Attente du retour au menu principal
        while (Helper.FindObject<MenuManager>() is null) {
            yield return new WaitForEndOfFrame();
        }

        // Reconnexion immédiate sans délai
        Helper.GameNetworkManager?.JoinLobby(connectedLobby.Lobby, connectedLobby.SteamId);
        this.isRejoining = false;
    }

    void OnGameStart() {
        // On applique l'invisibilité et le camouflage dès le début de la partie
        if (Setting.EnableAntiKick) {
            this.ApplyIdentitySettings();
        }
    }

    void ApplyIdentitySettings() {
        if (Helper.LocalPlayer == null) return;

        // PRIORITÉ 1 : Usurpation d'identité (Copy Name)
        if (IdentityToCopy != null) {
            this.UpdateLocalName(IdentityToCopy);
            return;
        }

        // PRIORITÉ 2 : Nom caché (Invisible)
        if (HideName) {
            this.UpdateLocalName("\u200B"); // Caractère invisible (Zero-width space)
        }
    }

    void UpdateLocalName(string newName) {
        if (Helper.LocalPlayer == null) return;
        
        // Mise à jour du pseudo sur l'objet joueur local
        Helper.LocalPlayer.playerUsername = newName;
        
        // Notification dans le chat local pour confirmation
        if (Helper.HUDManager != null) {
            Chat.Print($"Identité furtive appliquée : {newName}");
        }
    }

    void ToggleAntiKick() {
        // Bascule de l'Anti-Kick et des paramètres de furtivité
        Setting.EnableAntiKick = !Setting.EnableAntiKick;
        Setting.EnableInvisible = Setting.EnableAntiKick;
        HideName = Setting.EnableAntiKick;

        string status = Setting.EnableAntiKick ? "<color=green>ACTIF</color>" : "<color=red>INACTIF</color>";
        Chat.Print($"Anti-Kick & Stealth Mode : {status}");

        // Si on désactive, on réinitialise l'identité à copier
        if (!Setting.EnableAntiKick) {
            IdentityToCopy = null;
        }
    }

    // Commande pour voler l'identité d'un joueur cible
    public void CopyPlayerIdentity(string targetName) {
        var target = Helper.Players.FirstOrDefault(p => p.playerUsername.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        
        if (target != null) {
            IdentityToCopy = target.playerUsername;
            this.UpdateLocalName(target.playerUsername);
            Chat.Print($"Identité copiée sur : {target.playerUsername}");
        } else {
            Chat.Print("Joueur introuvable pour la copie d'identité.");
        }
    }
}