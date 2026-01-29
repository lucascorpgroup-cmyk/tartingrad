using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;

[Command("say")]
sealed class SayCommand : ICommand {
    public async Task Execute(Arguments args, CancellationToken cancellationToken) {

        // Vérif arguments
        if (args.Length < 2) {
            Chat.Print("Usage: say <player> <message>");
            return;  // IMPORTANT !
        }

        // Vérif joueur
        if (Helper.GetPlayer(args[0]) is not PlayerControllerB player) {
            Chat.Print("Target player is not found!");
            return;
        }

        // Construction du message
        string message = string.Join(" ", args[1..]);

        // Vérif longueur
        if (message.Length > 50) {
            Chat.Print($"You have exceeded the max message length by {message.Length - 50} characters!");
            return;
        }

        // Vérif HUDManager
        if (Helper.HUDManager == null) {
            Chat.Print("HUDManager is null! Cannot send message.");
            return;
        }

        // Envoi du message
        Helper.HUDManager.AddTextToChatOnServer(message, player.PlayerIndex());
        Chat.Print($"Message sent to {player.playerUsername} !");
    }
}
