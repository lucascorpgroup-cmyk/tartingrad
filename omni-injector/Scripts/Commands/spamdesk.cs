// Chemin : Scripts/Commands/SpamDepositDeskCommand.cs
// Commande unique : /spamdesk

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GameNetcodeStuff;

namespace lc_hax.Commands
{
    [Command("spamdesk")]
    sealed class SpamDepositDeskCommand : ICommand
    {
        static bool Enabled;
        static float Delay = 0.05f; // 50 ms
        static SpamDeskUpdater UpdaterInstance;

        public Task Execute(Arguments args, CancellationToken _)
        {
            if (UpdaterInstance == null)
            {
                GameObject go = new GameObject("SpamDepositDeskUpdater");
                UnityEngine.Object.DontDestroyOnLoad(go);
                UpdaterInstance = go.AddComponent<SpamDeskUpdater>();
            }

            if (args.Length == 0)
            {
                Print();
                return Task.CompletedTask;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "on": Enabled = true; break;
                case "off": Enabled = false; break;
                case "toggle": Enabled = !Enabled; break;
                case "speed":
                    if (args.Length >= 2 && float.TryParse(args[1], out float s))
                        Delay = Mathf.Clamp(s / 1000f, 0.01f, 1f);
                    break;
                default:
                    Chat.Print("[spamdesk] Commande inconnue. Usage: /spamdesk on|off|toggle|speed 50");
                    break;
            }

            Print();
            return Task.CompletedTask;
        }

        static void Print()
        {
            Chat.Print($"[spamdesk] {(Enabled ? "ON" : "OFF")} | {(Delay * 1000f):0} ms");
        }

        class SpamDeskUpdater : MonoBehaviour
        {
            float _last;

            void Update()
            {
                if (!Enabled) return;
                if (Time.time - _last < Delay) return;
                _last = Time.time;

                foreach (var desk in UnityEngine.Object.FindObjectsOfType<DepositItemsDesk>())
                {
                    if (desk == null) continue;

                    // Essayer côté serveur via reflection (best-effort)
                    if (!TrySendServerRPC(desk))
                    {
                        // fallback local si non-host
                        try { desk.OpenShutDoorClientRpc(); } catch { }
                    }
                }
            }

            bool TrySendServerRPC(DepositItemsDesk desk)
            {
                try
                {
                    var type = desk.GetType();
                    var method = type.GetMethod("OpenShutDoorClientRpc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) return false;

                    // si méthode NetworkBehaviour RPC → vérifier networkObject
                    var netObjProp = type.GetProperty("NetworkObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    object instance = desk; // <-- type C# natif
                    if (netObjProp != null)
                        instance = netObjProp.GetValue(desk) ?? desk;

                    method.Invoke(instance, null);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
