// TerminalErrorSpamCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Command("termerror")]
sealed class TerminalErrorSpamCommand : ICommand
{
    // Toggle token (nullable)
    static CancellationTokenSource? _cts;

    // Keywords pour repérer terminaux
    static readonly string[] TerminalKeywords = new[] { "terminal", "companyterminal", "shop", "console" };

    private const int DefaultIntervalMs = 400;

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        // Si déjà actif : toggle off
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            Chat.Print("<color=#00FF88>termerror:</color> stopped.");
            return;
        }

        // parse args
        ulong durationMs = 0;
        int intervalMs = DefaultIntervalMs;
        if (args.Length >= 1 && ulong.TryParse(args[0], out ulong d)) durationMs = d;
        if (args.Length >= 2 && int.TryParse(args[1], out int i) && i > 0) intervalMs = i;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationToken token = _cts.Token;

        List<GameObject> terminals = FindTerminals();
        if (terminals.Count == 0)
        {
            Chat.Print("<color=#FF5555>termerror:</color> aucun terminal trouvé dans la scène.");
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            return;
        }

        Chat.Print($"<color=#00FF88>termerror:</color> trouvé {terminals.Count} terminal(s) — spam toutes les {intervalMs}ms{(durationMs>0? $" pendant {durationMs}ms": "")}.");

        // build handlers
        var handlers = terminals.Select(t => PrepareTerminalAudioHandler(t)).ToList();

        // fallback if no handler can play
        AudioSource? fallbackSource = null;
        if (!handlers.Any(h => h.CanPlay))
        {
            fallbackSource = CreateFallbackAudioSource();
            if (fallbackSource == null)
            {
                Chat.Print("<color=#FF5555>termerror:</color> impossible de créer un fallback audio.");
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                return;
            }
            Chat.Print("<color=#FFAA00>termerror:</color> aucun clip natif trouvé — utilisation d'un son fallback local.");
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var h in handlers)
                {
                    if (!h.CanPlay) continue;
                    try { h.Play(); }
                    catch (Exception ex) { Debug.LogWarning($"termerror: handler.Play() failed: {ex}"); }
                }

                if (fallbackSource != null)
                {
                    fallbackSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f);
                    fallbackSource.PlayOneShot(fallbackSource.clip);
                }

                // wait respecting cancellation
                int waited = 0;
                while (waited < intervalMs && !token.IsCancellationRequested)
                {
                    await Task.Delay(50, token).ContinueWith(_ => { }); // small chunk delay
                    waited += 50;
                }

                if (durationMs > 0 && (ulong)watch.ElapsedMilliseconds >= durationMs) break;
            }
        }
        catch (OperationCanceledException) { /* expected */ }
        finally
        {
            foreach (var h in handlers) h.Dispose();
            if (fallbackSource != null) UnityEngine.Object.Destroy(fallbackSource.gameObject);
            if (_cts != null) { _cts.Cancel(); _cts.Dispose(); _cts = null; }
            Chat.Print("<color=#00FF88>termerror:</color> finished.");
        }
    }

    // ---- helpers ----

    static List<GameObject> FindTerminals()
    {
        var list = new List<GameObject>();
        try
        {
            var allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
            foreach (var tr in allTransforms)
            {
                if (tr == null || tr.gameObject == null) continue;
                string name = tr.name.ToLowerInvariant();
                if (TerminalKeywords.Any(k => name.Contains(k)))
                {
                    // add root gameobject (avoid duplicates)
                    var go = tr.gameObject;
                    if (!list.Contains(go)) list.Add(go);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"termerror: FindTerminals failed: {ex}");
        }
        return list;
    }

    // handler class (nullable-aware)
    class TerminalAudioHandler : IDisposable
    {
        public GameObject Terminal { get; }
        public AudioSource? NativeSource { get; }
        public AudioClip? ErrorClip { get; }
        public MethodInfo? PlayMethod { get; }

        public bool CanPlay => (NativeSource != null && ErrorClip != null) || PlayMethod != null;

        public TerminalAudioHandler(GameObject g, AudioSource? native, AudioClip? clip, MethodInfo? playMethod)
        {
            Terminal = g;
            NativeSource = native;
            ErrorClip = clip;
            PlayMethod = playMethod;
        }

        public void Play()
        {
            // Try method first (best-effort)
            if (PlayMethod != null)
            {
                try
                {
                    var target = Terminal.GetComponent(PlayMethod.DeclaringType!) ?? Terminal.GetComponent<MonoBehaviour>();
                    if (target != null)
                    {
                        if (PlayMethod.GetParameters().Length == 0)
                            PlayMethod.Invoke(target, null);
                        else
                            PlayMethod.Invoke(target, new object[] { });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"termerror: PlayMethod.Invoke failed: {ex}");
                }
            }

            if (NativeSource != null && ErrorClip != null)
            {
                try { NativeSource.PlayOneShot(ErrorClip); }
                catch (Exception ex) { Debug.LogWarning($"termerror: PlayOneShot failed: {ex}"); }
            }
        }

        public void Dispose()
        {
            // Do not destroy native sources (they belong to scene)
        }
    }

    static TerminalAudioHandler PrepareTerminalAudioHandler(GameObject terminal)
    {
        if (terminal == null) return new TerminalAudioHandler(terminal ?? new GameObject("null_terminal_fallback"), null, null, null);

        AudioSource? bestSource = null;
        AudioClip? bestClip = null;

        try
        {
            var sources = terminal.GetComponentsInChildren<AudioSource>(true);
            foreach (var s in sources)
            {
                if (s == null) continue;
                if (s.clip != null)
                {
                    string clipName = s.clip.name.ToLowerInvariant();
                    if (clipName.Contains("error") || clipName.Contains("fail") || clipName.Contains("beep") || clipName.Contains("wrong"))
                    {
                        bestSource = s;
                        bestClip = s.clip;
                        break;
                    }
                    if (bestSource == null)
                    {
                        bestSource = s;
                        bestClip = s.clip;
                    }
                }
            }

            // reflection: look for parameterless methods that look like PlayError/Beep/Trigger
            MethodInfo? playMethod = null;
            var comps = terminal.GetComponents<MonoBehaviour>() ?? Array.Empty<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var methods = c.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var m in methods)
                {
                    string mn = m.Name.ToLowerInvariant();
                    if ((mn.Contains("play") || mn.Contains("trigger") || mn.Contains("beep")) &&
                        (mn.Contains("error") || mn.Contains("fail") || mn.Contains("alert") || mn.Contains("noise")))
                    {
                        if (m.GetParameters().Length == 0)
                        {
                            playMethod = m;
                            break;
                        }
                    }
                }
                if (playMethod != null) break;
            }

            // If still no clip, search AudioClip fields on components
            if (bestClip == null)
            {
                foreach (var c in comps)
                {
                    if (c == null) continue;
                    var fields = c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var f in fields)
                    {
                        if (!typeof(AudioClip).IsAssignableFrom(f.FieldType)) continue;
                        try
                        {
                            var val = f.GetValue(c) as AudioClip;
                            if (val == null) continue;
                            string n = val.name.ToLowerInvariant();
                            if (n.Contains("error") || n.Contains("fail") || n.Contains("beep") || n.Contains("wrong"))
                            {
                                bestClip = val;
                                break;
                            }
                            if (bestClip == null) bestClip = val;
                        }
                        catch { }
                    }
                    if (bestClip != null) break;
                }
            }

            return new TerminalAudioHandler(terminal, bestSource, bestClip, playMethod);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"termerror: PrepareTerminalAudioHandler failed for {terminal.name}: {ex}");
            return new TerminalAudioHandler(terminal, bestSource, bestClip, null);
        }
    }

    static AudioSource? CreateFallbackAudioSource()
    {
        try
        {
            GameObject? anchor = Camera.main?.gameObject ?? Helper.LocalPlayer?.gameObject;
            GameObject goAnchor = anchor ?? new GameObject("TermErrorFallbackAnchor");

            var go = new GameObject("TermErrorFallbackSource");
            go.transform.SetParent(goAnchor.transform, false);
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            AudioClip clip = CreateSineAudioClip(0.08f, 440f);
            src.clip = clip;
            src.volume = 0.9f;
            return src;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"termerror: fallback audio creation failed: {ex}");
            return null;
        }
    }

    static AudioClip CreateSineAudioClip(float lengthSeconds, float frequency)
    {
        int samplerate = 44100;
        int sampleCount = Mathf.CeilToInt(samplerate * lengthSeconds);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)samplerate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.25f;
        }
        AudioClip clip = AudioClip.Create("term_beep_" + Guid.NewGuid().ToString("N"), sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
