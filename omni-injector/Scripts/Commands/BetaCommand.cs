using HarmonyLib;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalMenu.Commands
{
    internal class BuildAnywhereCommand : ICommand
    {
        public string Name => "buildanywhere";
        public string Description => "Permet de construire partout, toujours actif.";

        public Task Execute(Arguments args, CancellationToken token)
        {
            var harmony = new Harmony("com.lcd.buildanywhere");
            harmony.PatchAll();

            return Task.CompletedTask;
        }
    }

    // Patches Harmony
    [HarmonyPatch(typeof(ShipBuildModeManager), "PlayerMeetsConditionsToBuild")]
    public static class PlayerMeetsConditionsToBuildPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = true; // Toujours autorisé
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipBuildModeManager), "Update")]
    public static class ShipBuildModeManagerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool ___CanConfirmPosition, ref PlaceableShipObject ___placingObject, ref bool ___InBuildMode)
        {
            if (___InBuildMode)
            {
                ___CanConfirmPosition = true;
                ___placingObject.AllowPlacementOnWalls = true;
                ___placingObject.AllowPlacementOnCounters = true;
            }
        }
    }

    [HarmonyPatch(typeof(ShipBuildModeManager), "PlaceShipObject")]
    public static class PlaceShipObjectPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Vector3 placementPosition, ref Vector3 placementRotation, ref PlaceableShipObject placeableObject)
        {
            placeableObject.transform.position = placementPosition;
            placeableObject.transform.rotation = Quaternion.Euler(placementRotation);
            StartOfRound.Instance.suckingFurnitureOutOfShip = false;

            var unlock = StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID];
            unlock.placedPosition = placementPosition;
            unlock.placedRotation = placementRotation;
            unlock.hasBeenMoved = true;

            if (placeableObject.parentObjectSecondary != null)
            {
                Quaternion quaternion = Quaternion.Euler(placementRotation) * Quaternion.Inverse(placeableObject.mainMesh.transform.rotation);
                placeableObject.parentObjectSecondary.transform.rotation = quaternion * placeableObject.parentObjectSecondary.transform.rotation;

                placeableObject.parentObjectSecondary.position = placementPosition +
                    (placeableObject.parentObjectSecondary.transform.position - placeableObject.mainMesh.transform.position) +
                    (placeableObject.mainMesh.transform.position - placeableObject.placeObjectCollider.transform.position);
            }
        }
    }
}
