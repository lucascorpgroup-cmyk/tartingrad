// Chemin : Scripts/Commands/PJSpammerCommand.cs
// Commande unique : /spampj

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace lc_hax.Commands
{
    [Command("spampj")]
    sealed class PJSpammerCommand : ICommand
    {
        // variables statiques pour persistance
        static bool Enabled;
        static float Delay = 0.1f; // secondes
        static PJSpammerUpdater UpdaterInstance;

        public Task Execute(Arguments args, CancellationToken _)
        {
            // créer le GameObject MonoBehaviour si pas encore
            if (UpdaterInstance == null)
            {
                GameObject go = new GameObject("PJSpammerUpdater");
                Object.DontDestroyOnLoad(go);
                UpdaterInstance = go.AddComponent<PJSpammerUpdater>();
            }

            if (args.Length == 0)
            {
                Print();
                return Task.CompletedTask;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "on":
                    Enabled = true;
                    break;

                case "off":
                    Enabled = false;
                    break;

                case "toggle":
                    Enabled = !Enabled;
                    break;

                case "speed":
                    if (args.Length >= 2 && float.TryParse(args[1], out float s))
                        Delay = Mathf.Clamp(s / 1000f, 0.02f, 5f); // convert ms → sec
                    break;

                default:
                    Chat.Print("[spamPJ] Commande inconnue. Usage: /spampj on|off|toggle|speed 150");
                    break;
            }

            Print();
            return Task.CompletedTask;
        }

        static void Print()
        {
            Chat.Print($"[spamPJ] {(Enabled ? "ON" : "OFF")} | {(Delay * 1000f):0} ms");
        }

        // -------------------------------
        // MonoBehaviour interne pour Update()
        class PJSpammerUpdater : MonoBehaviour
        {
            float _last;
            readonly System.Collections.Generic.List<Component> _triggers = new();

            void Start() => CacheTriggers();

            void Update()
            {
                if (!Enabled) return;
                if (Time.time - _last < Delay) return;
                _last = Time.time;

                var player = Helper.LocalPlayer;
                if (player == null) return;

                foreach (var t in _triggers)
                {
                    if (t == null) continue;

                    var m = t.GetType().GetMethod(
                        "TriggerAnimation",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic
                    );

                    if (m != null)
                    {
                        try { m.Invoke(t, new object[] { player }); }
                        catch { }
                    }
                }
            }

            void CacheTriggers()
            {
                _triggers.Clear();
                foreach (var go in Object.FindObjectsOfType<GameObject>())
                {
                    if (go == null) continue;
                    if (!go.name.StartsWith("PlushiePJManContainer")) continue;

                    foreach (var c in go.GetComponentsInChildren<Component>(true))
                    {
                        if (c == null) continue;
                        if (c.GetType().GetMethod("TriggerAnimation") != null)
                            _triggers.Add(c);
                    }
                }
            }
        }
    }
}
