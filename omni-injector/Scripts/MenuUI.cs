using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hax
{
    public class MenuUI : MonoBehaviour
    {
        // ===== WINDOW =====
        private bool showMenu;
        private Rect windowRect = new Rect(50, 50, 440, 620);
        private int tabIndex;

        // ===== SELF =====
        private bool godMode, infiniteStamina, noclip;
        private float speedHack = 1f;
        private bool spammSuit;
        private CancellationTokenSource spammToken;

        // ===== WORLD =====
        private bool esp, brightVision;
        private float brightness = 1f;

        // ===== TROLL =====
        private bool fakeLag, invertControls, headSpin;
        private bool cameraShake, fovPulse, rainbowScreen;
        private bool timeJitter, fakeFreeze, notifSpam;
        private bool drunkCamera, uiGlitch;

        // HEAD SPIN EXTREME
        private bool extremeHeadSpin;
        private float headSpinSpeed = 1000f;

        // ===== SETTINGS =====
        private bool notifications = true;
        private float uiScale = 1f;
        private int themeIndex;
        private Key openKey = Key.Insert;

        // ===== THEME =====
        private Color bgColor, buttonColor, textColor, accentColor;

        // ===== UI =====
        private GUIStyle tabStyle, activeTab, toggleStyle, windowStyle;

        // ===== NOTIF =====
        private string notif;
        private float notifTimer;
        private float spamTimer;

        // ===== PLAYERS =====
        private List<string> players = new() { "You", "Alpha", "Beta", "Sigma" };

        // ===== PLAYER HEAD =====
        public Transform playerHead; // assigner dans l’inspecteur (la tête du joueur)

        void Update()
        {
            if (Keyboard.current[openKey].wasPressedThisFrame)
                ToggleMenu();

            if (notifTimer > 0) notifTimer -= Time.deltaTime;

            if (notifSpam)
            {
                spamTimer -= Time.deltaTime;
                if (spamTimer <= 0)
                {
                    Notify("ERROR 0x1337 😈");
                    spamTimer = 0.3f;
                }
            }

            if (timeJitter) Time.timeScale = UnityEngine.Random.Range(0.3f, 1.7f);
            else if (!fakeFreeze) Time.timeScale = 1f;

            if (fakeFreeze) Time.timeScale = 0f;

            // ===== HEAD SPIN EXTREME LOGIC =====
            if (extremeHeadSpin && playerHead != null)
            {
                playerHead.localRotation *= Quaternion.Euler(0, headSpinSpeed * Time.deltaTime, 0);
            }

            // SELF toggles
            if (godMode) ExecuteCommand("/god");
            if (infiniteStamina) ExecuteCommand("/stamina");
            if (noclip) ExecuteCommand("/noclip");
            ExecuteCommand($"speed {speedHack:F1}");

            // WORLD toggles
            if (esp) ExecuteCommand("esp");
            if (brightVision) ExecuteCommand("bright");
            ExecuteCommand($"brightness {brightness:F1}");
        }

        void ToggleMenu()
        {
            showMenu = !showMenu;
            Cursor.visible = showMenu;
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void OnGUI()
        {
            if (!showMenu) return;

            float glitchScale = uiGlitch ? UnityEngine.Random.Range(0.97f, 1.03f) : 1f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale * glitchScale);

            ApplyTheme();
            InitStyles();

            GUI.backgroundColor = bgColor;
            GUI.contentColor = textColor;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "★ KIKOU TOOL V10 ★", windowStyle);

            if (notifications && notifTimer > 0)
            {
                GUI.color = accentColor;
                GUI.Label(new Rect(10, Screen.height - 35, 500, 30), notif);
                GUI.color = Color.white;
            }
        }

        void ApplyTheme()
        {
            switch (themeIndex)
            {
                case 0:
                    bgColor = new Color(0.12f, 0.12f, 0.12f);
                    buttonColor = new Color(0.2f, 0.2f, 0.2f);
                    textColor = Color.white;
                    accentColor = Color.cyan;
                    break;
                case 1:
                    bgColor = new Color(0.05f, 0.05f, 0.1f);
                    buttonColor = new Color(0.1f, 0.1f, 0.25f);
                    textColor = Color.green;
                    accentColor = Color.magenta;
                    break;
                case 2:
                    bgColor = new Color(0.85f, 0.85f, 0.85f);
                    buttonColor = Color.white;
                    textColor = Color.black;
                    accentColor = Color.blue;
                    break;
                case 3:
                    bgColor = Color.black;
                    buttonColor = Color.gray;
                    textColor = Color.yellow;
                    accentColor = Color.red;
                    break;
            }
        }

        void InitStyles()
        {
            if (tabStyle != null) return;
            windowStyle = new GUIStyle(GUI.skin.window);
            tabStyle = new GUIStyle(GUI.skin.button);
            activeTab = new GUIStyle(tabStyle);
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            activeTab.normal.textColor = accentColor;
        }

        void DrawWindow(int id)
        {
            DrawTabs();
            GUILayout.Space(10);

            switch (tabIndex)
            {
                case 0: DrawSelf(); break;
                case 1: DrawPlayers(); break;
                case 2: DrawWorld(); break;
                case 3: DrawTroll(); break;
                case 4: DrawSettings(); break;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("CLOSE", GUILayout.Height(30))) ToggleMenu();
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            DrawTab("SELF", 0);
            DrawTab("PLAYERS", 1);
            DrawTab("WORLD", 2);
            DrawTab("TROLL 🤡", 3);
            DrawTab("SETTINGS ⚙️", 4);
            GUILayout.EndHorizontal();
        }

        void DrawTab(string name, int id)
        {
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button(name, tabIndex == id ? activeTab : tabStyle))
                tabIndex = id;
        }

        void DrawSelf()
        {
            GUILayout.Label("PLAYER");

            godMode = GUILayout.Toggle(godMode, " God Mode");
            infiniteStamina = GUILayout.Toggle(infiniteStamina, " Infinite Stamina");
            noclip = GUILayout.Toggle(noclip, " NoClip");

            if (GUILayout.Button("Invisible"))
                ExecuteCommand("/invis");

            if (GUILayout.Button(spammSuit ? "Stop Spamm Wear" : "Spamm Wear"))
            {
                if (spammSuit)
                {
                    spammToken?.Cancel();
                    spammSuit = false;
                }
                else
                {
                    spammSuit = true;
                    spammToken = new CancellationTokenSource();
                    RunSpammWear(spammToken.Token);
                }
            }

            GUILayout.Label($"Speed: {speedHack:F1}");
            speedHack = GUILayout.HorizontalSlider(speedHack, 1f, 5f);
        }

        async void RunSpammWear(CancellationToken token)
        {
            string[] suits = new string[]
            {
                "armor", "hazmat", "spacesuit", "riot", "stealth"
            };

            try
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var suit in suits)
                    {
                        if (token.IsCancellationRequested) break;
                        ExecuteCommand($"/suit {suit}");
                        await System.Threading.Tasks.Task.Delay(50, token);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        void DrawPlayers()
        {
            GUILayout.Label("PLAYERS");
            foreach (var p in players)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(p);
                if (GUILayout.Button("TP", GUILayout.Width(40)))
                    ExecuteCommand($"tp {p}");
                GUILayout.EndHorizontal();
            }
        }

        void DrawWorld()
        {
            GUILayout.Label("WORLD");
            esp = GUILayout.Toggle(esp, " ESP");
            brightVision = GUILayout.Toggle(brightVision, " Bright Vision");
            GUILayout.Label($"Brightness: {brightness:F1}");
            brightness = GUILayout.HorizontalSlider(brightness, 0.5f, 3f);

            if (GUILayout.Button("LAND SHIP"))
                ExecuteCommand("/land");

            string[] planets = { "Assurance","Vow","Experimentation","Offense","March","Rend","Dine","Titan","Artifice","Embrion","Liquidation" };
            foreach (var p in planets)
            {
                if (GUILayout.Button($"Visit {p}"))
                    ExecuteCommand($"/visit {p}");
            }
        }

   void DrawTroll()
{
    GUILayout.Label("TROLL MODE 😈");

    fakeLag = GUILayout.Toggle(fakeLag, " Fake Lag");
    invertControls = GUILayout.Toggle(invertControls, " Invert Controls");
    headSpin = GUILayout.Toggle(headSpin, " Spin Head");
    cameraShake = GUILayout.Toggle(cameraShake, " Camera Shake");
    fovPulse = GUILayout.Toggle(fovPulse, " FOV Pulse");
    drunkCamera = GUILayout.Toggle(drunkCamera, " Drunk Camera");
    rainbowScreen = GUILayout.Toggle(rainbowScreen, " Rainbow Screen");
    uiGlitch = GUILayout.Toggle(uiGlitch, " UI Glitch");
    timeJitter = GUILayout.Toggle(timeJitter, " Time Jitter");
    fakeFreeze = GUILayout.Toggle(fakeFreeze, " Fake Freeze");
    notifSpam = GUILayout.Toggle(notifSpam, " Notification Spam");

    if (GUILayout.Button("RANDOM TELEPORT"))
        ExecuteCommand("/random kikou");

    if (GUILayout.Button("CRASH"))
        ExecuteCommand("/crash");

    if (GUILayout.Button("SPIN"))
        ExecuteCommand("/spin 2");

    // ✅ NOUVEAU BOUTON VOID (même méthode que SPIN)
    if (GUILayout.Button("VOID"))
        ExecuteCommand("/void");

    if (GUILayout.Button("UPRIGHT"))
        ExecuteCommand("/upright");

    GUILayout.Space(10);

    // HEAD SPIN EXTREME
    GUILayout.Label($"Head Spin Speed: {headSpinSpeed:F0}°/sec");
    headSpinSpeed = GUILayout.HorizontalSlider(headSpinSpeed, 100f, 5000f);

    if (GUILayout.Button(extremeHeadSpin ? "STOP HEAD SPIN" : "HEAD SPIN EXTREME"))
    {
        extremeHeadSpin = !extremeHeadSpin;
        Notify(extremeHeadSpin ? "Head Spin EXTREME activated!" : "Head Spin stopped!");
    }
}


        void DrawSettings()
        {
            GUILayout.Label("MENU SETTINGS");
            notifications = GUILayout.Toggle(notifications, " Notifications");
            GUILayout.Label($"UI Scale: {uiScale:F1}");
            uiScale = GUILayout.HorizontalSlider(uiScale, 0.8f, 1.5f);

            GUILayout.Label("Theme");
            int newTheme = GUILayout.Toolbar(themeIndex, new[] { "Dark", "Neon", "Classic", "Cyberpunk" });
            if (newTheme != themeIndex)
            {
                themeIndex = newTheme;
                Notify("Theme changed");
            }

            if (GUILayout.Button("RESET CONFIG"))
                ResetConfig();
        }

        void ResetConfig()
        {
            godMode = infiniteStamina = noclip = esp = brightVision = false;
            fakeLag = invertControls = headSpin = cameraShake = fovPulse = rainbowScreen = uiGlitch = timeJitter = fakeFreeze = notifSpam = drunkCamera = extremeHeadSpin = false;
            uiScale = speedHack = brightness = headSpinSpeed = 1f;
            Notify("Config reset");
        }

        void Notify(string msg)
        {
            notif = msg;
            notifTimer = 2.5f;
        }

        void ExecuteCommand(string commandName)
        {
            Chat.ExecuteCommand(commandName);
        }
    }
}
