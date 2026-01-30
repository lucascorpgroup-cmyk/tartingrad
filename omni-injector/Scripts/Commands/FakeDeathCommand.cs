using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

[Command("crash")]
sealed class crashCommand : ICommand {
    public async Task Execute(Arguments args, CancellationToken cancellationToken) {
        if (Helper.LocalPlayer is not PlayerControllerB player) return;

        Setting.EnableFakeDeath = true;

        int bodyCount = 100; // tu peux augmenter côté serveur
        float spread = 5f;

        // On spawn les corps en “fire-and-forget” pour ne pas bloquer
        for (int i = 0; i < bodyCount; i++) {
            Vector3 offset = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(1f, 3f),
                Random.Range(-spread, spread)
            );

            Vector3 randomVelocity = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(0f, 5f),
                Random.Range(-2f, 2f)
            );

            // Appel serveur non awaité pour que le client ne freeze pas
            player.KillPlayerServerRpc(
                playerId: player.PlayerIndex(),
                spawnBody: true,
                bodyVelocity: randomVelocity,
                causeOfDeath: unchecked((int)CauseOfDeath.Unknown),
                deathAnimation: 0,
                positionOffset: offset
            );
        }

        // Pas besoin d'attendre chaque corps, juste attendre le départ du ship
        await Helper.WaitUntil(() => player.playersManager.shipIsLeaving, cancellationToken);
        player.KillPlayer();
    }
}
