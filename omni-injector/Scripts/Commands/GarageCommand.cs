using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Command("hidebodies")]
sealed class HideBodiesCommand : ICommand
{
    private static bool enabled;

    // Track pour nettoyage
    private static readonly HashSet<GameObject> nukedBodies = new();

    private static void NukeBody(GameObject body)
    {
        if (!body || nukedBodies.Contains(body))
            return;

        // Désactive tout
        foreach (var r in body.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        foreach (var c in body.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        body.SetActive(false);
        nukedBodies.Add(body);
    }

    private static void NukeAllBodies()
    {
        var bodies = GameObject.FindObjectsOfType<DeadBodyInfo>(true);
        foreach (var body in bodies)
            NukeBody(body.gameObject);
    }

    // Hook permanent : annule les nouveaux spawns (fakedeath inclus)
    private static void UpdateHook()
    {
        if (!enabled) return;

        var bodies = GameObject.FindObjectsOfType<DeadBodyInfo>(true);
        foreach (var body in bodies)
            NukeBody(body.gameObject);
    }

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Chat.Print("Usage: hidebodies on | off");
            return;
        }

        if (args[0].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            if (enabled)
            {
                Chat.Print("🟡 Anti-crash déjà actif.");
                return;
            }

            enabled = true;

            // Nettoyage immédiat
            NukeAllBodies();

            // Hook update longue durée
            Helper.CreateComponent<TransientBehaviour>()
                  .Init(_ => UpdateHook(), int.MaxValue);

            Chat.Print("🟢 Anti-crash activé — fakedeath neutralisé côté client.");
        }
        else if (args[0].Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            enabled = false;
            nukedBodies.Clear();
            Chat.Print("🔴 Anti-crash désactivé.");
        }
        else
        {
            Chat.Print("Usage: hidebodies on | off");
        }
    }
}
