using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Command("spin")]
sealed class SpinCommand2 : ICommand {
    // J'ai réduit la valeur de 810.0f à 90.0f pour une rotation plus lente
    static Action<float> PlaceObjectAtRotation(PlaceableShipObject shipObject) => (timeElapsed) =>
        Helper.PlaceObjectAtPosition(
            shipObject,
            shipObject.transform.position,
            new Vector3(0.0f, timeElapsed * 90.0f, 0.0f) 
        );

    static Action<PlaceableShipObject> SpinObject(ulong duration) => (shipObject) =>
        Helper.CreateComponent<TransientBehaviour>()
              .Init(SpinCommand2.PlaceObjectAtRotation(shipObject), duration);

    public async Task Execute(Arguments args, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            Chat.Print("Usage: spin <duration>");
            return; // Ajout d'un return pour éviter de continuer si l'argument manque
        }

        if (!ulong.TryParse(args[0], out ulong duration)) {
            Chat.Print($"Spin {nameof(duration)} must be a positive number!");
            return;
        }

        Helper.FindObjects<PlaceableShipObject>()
              .ForEach(SpinCommand2.SpinObject(duration));
    }
}