using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;
using System.Collections.Generic;

[Command("trigger")]
sealed class TriggerCommand : ICommand
{
    private static Vector3? TP1 = null;
    private static Vector3? TP2 = null;
    private static CancellationTokenSource teleportCts = null;

    private static int teleportSpeedMs = 10;

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Chat.Print("Usage: /trigger set TP1|TP2 | on | off | speed <ms>");
            return;
        }

        if (Helper.LocalPlayer is not PlayerControllerB player)
        {
            Chat.Print("Player not found!");
            return;
        }

        string action = args[0].ToLower();

        if (action == "set" && args.Length > 1)
        {
            string point = args[1].ToUpper();
            if (point == "TP1") TP1 = player.transform.position;
            else if (point == "TP2") TP2 = player.transform.position;
            else { Chat.Print("Invalid point. Use TP1 or TP2."); return; }
            Chat.Print($"{point} set!");
            return;
        }

        if (action == "speed" && args.Length > 1 && int.TryParse(args[1], out int ms) && ms > 0)
        {
            teleportSpeedMs = ms;
            Chat.Print($"Teleport speed set to {teleportSpeedMs}ms.");
            return;
        }

        if (action == "on")
        {
            if (TP1 == null || TP2 == null)
            {
                Chat.Print("Both TP1 and TP2 must be set first!");
                return;
            }

            teleportCts?.Cancel();
            teleportCts = new CancellationTokenSource();
            CancellationToken token = teleportCts.Token;

            // On calcule la position relative des objets par rapport au joueur
            PlaceableShipObject[] allObjects = Helper.FindObjects<PlaceableShipObject>();
            Dictionary<PlaceableShipObject, Vector3> objectOffsets = new();
            foreach (var obj in allObjects)
                objectOffsets[obj] = obj.transform.position - player.transform.position;

            Chat.Print("Teleport loop started with ship objects!");

            _ = Task.Run(async () =>
            {
                bool toggle = false;
                while (!token.IsCancellationRequested)
                {
                    Vector3 target = toggle ? TP1.Value : TP2.Value;
                    toggle = !toggle;

                    // Déplace le joueur
                    player.transform.position = target;

                    // Déplace tous les objets en conservant leur offset initial
                    foreach (var kv in objectOffsets)
                    {
                        var obj = kv.Key;
                        var offset = kv.Value;
                        obj.transform.position = target + offset;
                    }

                    await Task.Delay(teleportSpeedMs, token);
                }
            }, token);

            return;
        }

        if (action == "off" && teleportCts != null)
        {
            teleportCts.Cancel();
            teleportCts = null;
            Chat.Print("Teleport loop stopped.");
        }
    }
}
