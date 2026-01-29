using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

[Command("fakedeath")]
sealed class FakeDeathCommand : ICommand
{
    private const int NumBodies = 60;
    private const float SpreadRadius = 2f;

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (Helper.LocalPlayer is not PlayerControllerB player)
            return;

        Setting.EnableFakeDeath = true;

        // --- MORT SERVEUR PRINCIPALE ---
        player.KillPlayerServerRpc(
            playerId: player.PlayerIndex(),
            spawnBody: true,
            bodyVelocity: Vector3.zero,
            causeOfDeath: unchecked((int)CauseOfDeath.Unknown),
            deathAnimation: 0,
            positionOffset: Vector3.zero
        );

        // --- MORT CLIENT-SIDE POUR ÉVITER LE FREEZE DE L'ÉCRAN ---
        // (Sinon le client ne déclenche pas l'état de mort → image figée)
        player.KillPlayer();

        // --- SPAWN DES FAUX CORPS ---
        for (int i = 0; i < NumBodies; i++)
        {
            Vector3 offset = Random.insideUnitSphere * SpreadRadius;

            player.KillPlayerServerRpc(
                playerId: player.PlayerIndex(),
                spawnBody: true,
                bodyVelocity: offset,
                causeOfDeath: unchecked((int)CauseOfDeath.Unknown),
                deathAnimation: 0,
                positionOffset: offset
            );
        }

        // Attend que le vaisseau décolle
        await Helper.WaitUntil(() => player.playersManager.shipIsLeaving, cancellationToken);

        // Tue le joueur local proprement si jamais quelque chose l’a "réanimé"
        player.KillPlayer();
    }
}
