using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;
using ZLinq;

[Command("destroy")]
sealed class DestroyCommand : ICommand {

    static IEnumerator DestroyAllItemsAsync(PlayerControllerB player) {
        float currentWeight = player.carryWeight;

        // Copie la liste pour éviter les problèmes si Helper.Grabbables change pendant la coroutine
        GrabbableObject[] grabbables = Helper.Grabbables.ToArray();

        foreach (GrabbableObject grabbable in grabbables) {
            if (grabbable is null) continue;

            // Trouve un slot vide ; si aucun, on utilise le slot courant (mais normalement on vérifie avant de lancer la commande)
            int emptySlot = -1;
            for (int i = 0; i < player.ItemSlots.Length; i++) {
                if (player.ItemSlots[i] == null) {
                    emptySlot = i;
                    break;
                }
            }

            if (emptySlot != -1) {
                // Force le joueur à utiliser ce slot pour le grab afin d'éviter d'empiler tout dans le même slot
                player.currentItemSlot = emptySlot;
            }

            // Essayer de grab l'objet plusieurs fois pendant un court délai si nécessaire
            float grabStart = Time.time;
            const float grabAttemptTimeout = 0.5f; // 500 ms
            bool grabbedLocally = false;

            while (Time.time - grabStart < grabAttemptTimeout) {
                player.GrabObject(grabbable);

                // On laisse une frame pour que l'update se fasse et que ItemSlots soit mis à jour
                yield return null;

                // Vérifie si l'objet est bien dans le slot que l'on a choisi / ou si currentlyHeldObjectServer correspond
                if (player.currentlyHeldObjectServer == grabbable ||
                    (player.currentItemSlot >= 0 && player.ItemSlots[player.currentItemSlot] == grabbable)) {
                    grabbedLocally = true;
                    break;
                }

                // petite attente avant une nouvelle tentative (1 frame)
                yield return null;
            }

            // Si on a réussi à "grab" localement, attend explicitement jusqu'à ce que le slot soit à jour (avec timeout)
            if (grabbedLocally) {
                float waitStart = Time.time;
                const float waitTimeout = 0.75f; // 750 ms max d'attente pour la sync
                bool readyToDespawn = false;

                while (Time.time - waitStart < waitTimeout) {
                    if (player.currentlyHeldObjectServer == grabbable ||
                        (player.currentItemSlot >= 0 && player.ItemSlots[player.currentItemSlot] == grabbable)) {
                        readyToDespawn = true;
                        break;
                    }
                    yield return null;
                }

                if (readyToDespawn) {
                    player.DespawnHeldObject();
                    // laisse une frame pour que le serveur/slot se remette à jour avant continuer
                    yield return null;
                } else {
                    // Si on n'a pas réussi la sync, passe à l'objet suivant (évite blocage)
                    continue;
                }
            } else {
                // Si on n'a pas pu grab l'objet dans le timeout, on tente quand même un despawn direct si le serveur pense que c'est tenu
                if (player.currentlyHeldObjectServer == grabbable) {
                    player.DespawnHeldObject();
                    yield return null;
                } else {
                    // Sinon on skip l'objet (on pourrait loguer si besoin)
                    continue;
                }
            }
        }

        // Restaure le poids
        player.carryWeight = currentWeight;
    }

    static Result DestroyHeldItem(PlayerControllerB player) {
        if (player.currentlyHeldObjectServer is null) {
            return new Result { Message = "You are not holding anything!" };
        }

        player.DespawnHeldObject();
        return new Result { Success = true };
    }

    static Result DestroyAllItems(PlayerControllerB player) {
        Helper.CreateComponent<AsyncBehaviour>()
              .Init(() => DestroyAllItemsAsync(player));

        return new Result { Success = true };
    }

    public async Task Execute(Arguments args, CancellationToken cancellationToken) {
        if (Helper.LocalPlayer is not PlayerControllerB player) return;

        if (player.ItemSlots.WhereIsNotNull().AsValueEnumerable().Count() >= 4) {
            Chat.Print("You must have an empty inventory slot!");
            return;
        }

        Result result = args[0] switch {
            null => DestroyHeldItem(player),
            "--all" => DestroyAllItems(player),
            _ => new Result { Message = "Invalid arguments!" }
        };

        if (!result.Success) {
            Chat.Print(result.Message);
        }
    }
}
