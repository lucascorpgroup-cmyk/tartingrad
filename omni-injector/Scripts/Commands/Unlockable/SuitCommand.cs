using HarmonyLib;
using Steamworks;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace lc_hax.Scripts.Commands.Unlockable
{
    // =========================
    // STEAM ID SPOOFER (SAFE)
    // =========================
    [HarmonyPatch]
    internal static class SteamIDSpoofer
    {
        public static bool Enabled = false;
        public static ulong OriginalSteamId = 0;
        public static ulong CurrentSteamId = 0;

        // Init SAFE (lazy)
        public static void EnsureInit()
        {
            if (OriginalSteamId != 0)
                return;

            // Steam pas prêt ? on annule
            if (!SteamClient.IsValid)
            {
                Debug.Log("[SteamIDSpoofer] SteamClient not ready");
                return;
            }

            OriginalSteamId = SteamClient.SteamId.Value;
            CurrentSteamId = OriginalSteamId;

            Debug.Log($"[SteamIDSpoofer] Original SteamID saved: {OriginalSteamId}");
        }

        public static void Spoof(ulong newId)
        {
            EnsureInit();

            if (OriginalSteamId == 0)
                return;

            Enabled = true;
            CurrentSteamId = newId;
            Debug.Log($"[SteamIDSpoofer] Spoofing SteamID -> {newId}");
        }

        public static void Cancel()
        {
            if (OriginalSteamId == 0)
                return;

            Enabled = false;
            CurrentSteamId = OriginalSteamId;
            Debug.Log("[SteamIDSpoofer] Spoof canceled");
        }

        // 🔹 SEUL hook Steam (safe)
        [HarmonyPatch(typeof(SteamClient), "get_SteamId")]
        [HarmonyPrefix]
        static bool SteamClient_GetSteamId(ref SteamId __result)
        {
            if (!Enabled)
                return true;

            __result = (SteamId)CurrentSteamId;
            return false;
        }
    }

    // =========================
    // RANDOM STEAMID GENERATOR
    // =========================
    internal static class SteamIdGenerator
    {
        public static ulong GenerateRandom()
        {
            const ulong baseId = 76561197960265728UL;
            ulong offset = (ulong)UnityEngine.Random.Range(10_000_000, 800_000_000);
            return baseId + offset;
        }
    }

    // =========================
    // CHAT COMMAND : /spoof
    // =========================
    [Command("spoof")]
    sealed class SpoofCommand : ICommand
    {
        public Task Execute(Arguments args, CancellationToken token)
        {
            if (args.Length == 0)
            {
                Debug.Log("[/spoof] Usage: /spoof random | /spoof cancel");
                return Task.CompletedTask;
            }

            string sub = args[0].ToLowerInvariant();

            if (sub == "random")
            {
                ulong id = SteamIdGenerator.GenerateRandom();
                SteamIDSpoofer.Spoof(id);
                Debug.Log($"[/spoof] Random SteamID set: {id}");
            }
            else if (sub == "cancel")
            {
                SteamIDSpoofer.Cancel();
                Debug.Log("[/spoof] SteamID restored");
            }
            else
            {
                Debug.Log("[/spoof] Unknown argument");
            }

            return Task.CompletedTask;
        }
    }
}
