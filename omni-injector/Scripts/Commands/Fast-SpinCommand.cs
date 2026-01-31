using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Command("fastspin")]
sealed class SpinCommand : ICommand
{
    private const float BaseRotationSpeed = 750.0f;
    private static readonly System.Random Randomizer = new();

    private static Action<float> CreateRotationAction(PlaceableShipObject shipObject, Vector3 axis, float speed)
    {
        return timeElapsed =>
        {
            if (shipObject == null || shipObject.transform == null)
                return;

            // Calcule la rotation en fonction du temps et d’un axe spécifique
            Vector3 rotation = axis * (timeElapsed * speed);
            Helper.PlaceObjectAtPosition(shipObject, shipObject.transform.position, rotation);
        };
    }

    private static Action<PlaceableShipObject> CreateSpinAction(ulong duration)
    {
        return shipObject =>
        {
            if (shipObject == null)
                return;

            // Axe de rotation aléatoire (normalisé)
            Vector3 axis = new(
                (float)(Randomizer.NextDouble() * 2 - 1),
                (float)(Randomizer.NextDouble() * 2 - 1),
                (float)(Randomizer.NextDouble() * 2 - 1)
            );
            axis.Normalize();

            // Vitesse de rotation aléatoire (entre 80% et 150% de la base)
            float speed = BaseRotationSpeed * UnityEngine.Random.Range(0.8f, 1.5f);

            var transient = Helper.CreateComponent<TransientBehaviour>();
            transient?.Init(CreateRotationAction(shipObject, axis, speed), duration);
        };
    }

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Chat.Print("<color=#FF5555>Usage:</color> spin <durationMs>");
            return;
        }

        if (!ulong.TryParse(args[0], out ulong duration) || duration == 0)
        {
            Chat.Print("<color=#FF5555>Error:</color> duration must be a positive number!</color>");
            return;
        }

        // Récupère tous les objets posables du vaisseau (no host)
        PlaceableShipObject[] objects = Helper.FindObjects<PlaceableShipObject>();
        if (objects.Length == 0)
        {
            Chat.Print("<color=#AAAAAA>No placeable ship objects found.</color>");
            return;
        }

        Chat.Print($"<color=#00FF88>Spinning {objects.Length} object(s) randomly for {duration} ms...</color>");

        foreach (PlaceableShipObject obj in objects)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            CreateSpinAction(duration).Invoke(obj);
            await Task.Yield(); // Évite les freezes
        }

        Chat.Print("<color=#00FF88>All objects finished spinning.</color>");
    }
}
