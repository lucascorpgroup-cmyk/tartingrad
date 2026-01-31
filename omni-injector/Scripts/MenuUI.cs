using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hax
{
    public class MenuUI : MonoBehaviour
    {
        // ===== FENETRE =====
        private bool showMenu;
        private Rect windowRect = new Rect(50, 50, 750, 650);
        private int tabIndex;
        private Vector2 scrollPosition;

        // ===== ETATS (Variables locales pour l'interface) =====
        // Note: Ces booléens servent à afficher l'état visuel des cases à cocher.
        private bool godMode, infiniteStamina, noclip, unlimitedJump;
        private bool stunClick, killClick, invis, hearAll, rapidUse;
        private bool esp, brightVision;
        
        // ===== VALEURS =====
        private float speedHack = 1f;
        private float brightness = 1f;
        private float uiScale = 1f;
        
        // ===== INPUTS (Champs texte) =====
        private string xpInput = "1000";
        private string visitInput = "Titan";
        private string moneyInput = "5000";
        private string quotaInput = "2000";
        private string buyInput = "shovel";
        private string buyQty = "1";
        private string chatInput = "Hello Omni!";
        private string noiseDuration = "30";
        
        // ===== TROLL & FX =====
        private bool fakeLag, invertControls, headSpin;
        private bool cameraShake, fovPulse, rainbowScreen;
        private bool timeJitter, fakeFreeze, notifSpam;
        private bool drunkCamera, uiGlitch;
        private bool extremeHeadSpin;
        private float headSpinSpeed = 1000f;
        
        // ===== SPAWN / HOST =====
        private string enemyToSpawn = "Girl"; // Ghost girl par défaut
        private string spawnAmount = "1";

        // ===== SYSTEME =====
        private bool spammSuit;
        private CancellationTokenSource spammToken;
        private bool notifications = true;
        private int themeIndex;
        private Key openKey = Key.Insert;

        // ===== STYLES =====
        private Color bgColor, buttonColor, textColor, accentColor;
        private GUIStyle tabStyle, activeTab, toggleStyle, windowStyle, labelStyle, boxStyle, buttonStyle, textFieldStyle;
        
        // ===== NOTIFICATIONS =====
        private string notif;
        private float notifTimer;

        // ===== REFS =====
        public Transform playerHead; 

        void Update()
        {
            // Ouverture/Fermeture du menu
            if (Keyboard.current[openKey].wasPressedThisFrame)
                ToggleMenu();

            // Timer Notification
            if (notifTimer > 0) notifTimer -= Time.deltaTime;

            // --- LOGIQUE FX (Doit rester dans Update pour être fluide) ---
            
            // Time Jitter (Lag temporel)
            if (timeJitter) Time.timeScale = UnityEngine.Random.Range(0.3f, 1.7f);
            else if (!fakeFreeze) Time.timeScale = 1f;
            
            // Fake Freeze (Arrêt du temps client)
            if (fakeFreeze) Time.timeScale = 0f;

            // HeadSpin Extreme (Rotation physique de la tête)
            if (extremeHeadSpin && playerHead != null)
            {
                playerHead.localRotation *= Quaternion.Euler(0, headSpinSpeed * Time.deltaTime, 0);
            }
            
            // Spam Suit (Async Logic handled separately, but safeguard here)
            if (!spammSuit && spammToken != null) { spammToken.Cancel(); spammToken = null; }
        }

        void ToggleMenu()
        {
            showMenu = !showMenu;
            Cursor.visible = showMenu;
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void OnGUI()
        {
            ApplyTheme();
            InitStyles();

            // 1. Notifications (Toujours visibles)
            if (notifications && notifTimer > 0)
            {
                GUI.color = accentColor;
                GUI.Box(new Rect(15, Screen.height - 55, 610, 50), ""); 
                GUI.Label(new Rect(20, Screen.height - 50, 600, 40), notif, labelStyle);
                GUI.color = Color.white;
            }

            // 2. Bouton d'ouverture discret
            if (!showMenu)
            {
                GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
                GUI.contentColor = accentColor;
                if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 40, 150, 30), "★ OPEN MENU ★", buttonStyle))
                {
                    ToggleMenu();
                }
                return;
            }

            // 3. Menu Principal
            float glitchScale = uiGlitch ? UnityEngine.Random.Range(0.99f, 1.01f) : 1f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * uiScale * glitchScale);

            GUI.backgroundColor = bgColor;
            GUI.contentColor = textColor;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "★ OMNI INJECTOR V4 ★", windowStyle);
        }

        void DrawWindow(int id)
        {
            DrawTabs();
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            switch (tabIndex)
            {
                case 0: DrawSelf(); break;
                case 1: DrawRealTimePlayers(); break;
                case 2: DrawWorld(); break;
                case 3: DrawGameAndItems(); break;
                case 4: DrawTrollAndFX(); break;
                case 5: DrawSettings(); break;
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("FERMER", GUILayout.Height(30))) ToggleMenu();
            if (GUILayout.Button("QUITTER LE JEU", GUILayout.Height(30))) Application.Quit();
            GUILayout.EndHorizontal();
            
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            DrawTab("PERSO", 0);
            DrawTab("JOUEURS", 1);
            DrawTab("MONDE", 2);
            DrawTab("HOST", 3);
            DrawTab("TROLL", 4);
            DrawTab("CONFIG", 5);
            GUILayout.EndHorizontal();
        }

        void DrawTab(string name, int id)
        {
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button(name, tabIndex == id ? activeTab : tabStyle))
            {
                tabIndex = id;
                scrollPosition = Vector2.zero;
            }
        }

        // ==========================================
        // ONGLET 1: PERSO
        // ==========================================
        void DrawSelf()
        {
            GUILayout.Label("--- ETAT DU JOUEUR ---", labelStyle);
            
            // Utilisation de DrawToggle pour éviter le spam
            // La commande n'est envoyée que si l'utilisateur clique
            DrawToggle(ref godMode, " God Mode (Invincible)", "/god");
            DrawToggle(ref infiniteStamina, " Stamina Infinie", "/stamina");
            DrawToggle(ref noclip, " NoClip (Voler)", "/noclip");
            DrawToggle(ref unlimitedJump, " Sauts Infinis", "/jump");
            DrawToggle(ref rapidUse, " Rapid Fire (Action Rapide)", "/rapid");
            DrawToggle(ref invis, " Invisible", "/invis");
            DrawToggle(ref hearAll, " Tout Entendre (Talkie Global)", "/hear");

            GUILayout.Space(10);
            GUILayout.Label("--- CLICS MAGIQUES ---", labelStyle);
            DrawToggle(ref stunClick, " Clic Gauche = STUN", "/stunclick");
            DrawToggle(ref killClick, " Clic Gauche = KILL", "/killclick");

            GUILayout.Space(10);
            GUILayout.Label("--- MOUVEMENT ---", labelStyle);
            GUILayout.Label($"Vitesse de course: {speedHack:F1}");
            // Slider intelligent : n'envoie la commande que si la valeur change
            float newSpeed = GUILayout.HorizontalSlider(speedHack, 1f, 10f);
            if (Math.Abs(newSpeed - speedHack) > 0.1f)
            {
                speedHack = newSpeed;
                ExecuteCommand($"/speed {speedHack:F1}");
            }

            GUILayout.Space(10);
            GUILayout.Label("--- XP & LEVEL ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("XP:", GUILayout.Width(40));
            xpInput = GUILayout.TextField(xpInput);
            if (GUILayout.Button("DEFINIR")) ExecuteCommand($"/xp {xpInput}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- COMBINAISONS ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Orange")) ExecuteCommand("/suit orange");
            if (GUILayout.Button("Vert")) ExecuteCommand("/suit green");
            if (GUILayout.Button("Hazmat")) ExecuteCommand("/suit hazard");
            if (GUILayout.Button("Pyjama")) ExecuteCommand("/suit pajama");
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button(spammSuit ? "ARRETER SPAM SUIT" : "LANCER SPAM SUIT"))
            {
                if (spammSuit) { spammSuit = false; spammToken?.Cancel(); }
                else { spammSuit = true; spammToken = new CancellationTokenSource(); RunSpammWear(spammToken.Token); }
            }
        }

        // ==========================================
        // ONGLET 2: JOUEURS
        // ==========================================
        void DrawRealTimePlayers()
        {
            GUILayout.Label("--- JOUEURS EN LIGNE ---", labelStyle);

            var players = Helper.Players; 

            if (players == null || players.Length == 0)
            {
                GUILayout.Label("Aucun joueur détecté.");
                return;
            }

            foreach (var player in players)
            {
                if (player == null) continue;

                string playerName = player.playerUsername;
                bool isMe = player == Helper.LocalPlayer;
                
                GUI.backgroundColor = isMe ? accentColor : buttonColor;
                GUILayout.BeginVertical(boxStyle);
                
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>{playerName}</b> {(isMe ? "(MOI)" : "")}", labelStyle);
                GUILayout.FlexibleSpace();
                if (!isMe)
                {
                    if (GUILayout.Button("TP A", GUILayout.Width(60))) ExecuteCommand($"/tp {playerName}");
                    if (GUILayout.Button("TP ICI", GUILayout.Width(60))) ExecuteCommand($"/tp {playerName} Alastor");
                }
                GUILayout.EndHorizontal();

                if (!isMe)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("KILL")) ExecuteCommand($"/kill {playerName}");
                    if (GUILayout.Button("BOMB")) ExecuteCommand($"/bomb {playerName}");
                    if (GUILayout.Button("VOID")) ExecuteCommand($"/void {playerName}");
                    if (GUILayout.Button("MASK")) ExecuteCommand($"/mask {playerName}");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("HEAL")) ExecuteCommand($"/heal {playerName}");
                    if (GUILayout.Button("POISON")) ExecuteCommand($"/poison");
                    if (GUILayout.Button("RANDOM")) ExecuteCommand($"/random {playerName}");
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("FATALITY (Giant)")) ExecuteCommand($"/fatality {playerName} ForestGiant");
                    if (GUILayout.Button("FATALITY (Jester)")) ExecuteCommand($"/fatality {playerName} Jester");
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUI.backgroundColor = buttonColor; 
                GUILayout.Space(5);
            }
        }

        // ==========================================
        // ONGLET 3: MONDE
        // ==========================================
        void DrawWorld()
        {
            GUILayout.Label("--- Crash/Spam ---", labelStyle);
            DrawToggle(ref esp, " Crash", "/crash");
            DrawToggle(ref brightVision, "SpamPJ", "/spampj on"); // Vérifier si commande existe ou interne
            
            GUILayout.Label($"Luminosité: {brightness:F1}");
            float newBright = GUILayout.HorizontalSlider(brightness, 0.5f, 5f);
            if (Math.Abs(newBright - brightness) > 0.1f)
            {
                brightness = newBright;
                ExecuteCommand($"/brightness {brightness:F1}");
            }

            GUILayout.Space(10);
            GUILayout.Label("--- SECURITÉ ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("UNLOCK ALL")) ExecuteCommand("/unlock");
            if (GUILayout.Button("LOCK ALL")) ExecuteCommand("/lock");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Experimentation door")) ExecuteCommand("/garage");
            if (GUILayout.Button("Explose Mines")) ExecuteCommand("/explode mine");
            if (GUILayout.Button("Explose Turrets")) ExecuteCommand("/explode turret"); // Alternative à berserk
            if (GUILayout.Button("Berserk turret")) ExecuteCommand("/berserk");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- VOYAGE ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lune:", GUILayout.Width(40));
            visitInput = GUILayout.TextField(visitInput);
            if (GUILayout.Button("GO")) ExecuteCommand($"/visit {visitInput}");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Titan")) ExecuteCommand("/visit Titan");
            if (GUILayout.Button("Rend")) ExecuteCommand("/visit Rend");
            if (GUILayout.Button("Artifice")) ExecuteCommand("/visit Artifice");
            if (GUILayout.Button("Dine")) ExecuteCommand("/visit Dine");
            if (GUILayout.Button("Embrion")) ExecuteCommand("/visit Embrion");
            if (GUILayout.Button("Vow")) ExecuteCommand("/visit Vow");
            if (GUILayout.Button("March")) ExecuteCommand("/visit March");
            GUILayout.EndHorizontal();
        }

        // ==========================================
        // ONGLET 4: HOST & GAME
        // ==========================================
        void DrawGameAndItems()
        {
            GUILayout.Label("--- GESTION PARTIE ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("START")) ExecuteCommand("/start");
            if (GUILayout.Button("LAND")) ExecuteCommand("/land");
            if (GUILayout.Button("FIN PARTIE")) ExecuteCommand("/end");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("REVIVE ALL")) ExecuteCommand("/revive");
            if (GUILayout.Button("GODS ALL")) ExecuteCommand("/gods");
            if (GUILayout.Button("EJECT ALL")) ExecuteCommand("/eject");
            GUILayout.EndHorizontal();
            
            GUILayout.Label("--- SPAWNER (HOST) ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mob:", GUILayout.Width(30));
            enemyToSpawn = GUILayout.TextField(enemyToSpawn);
            GUILayout.Label("Qté:", GUILayout.Width(30));
            spawnAmount = GUILayout.TextField(spawnAmount, GUILayout.Width(30));
            if (GUILayout.Button("SPAWN")) ExecuteCommand($"/spawn {enemyToSpawn} me {spawnAmount}"); // Spawn sur soi par défaut
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- BUILD ---", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TP")) ExecuteCommand("/build teleporter");
            if (GUILayout.Button("Inverse")) ExecuteCommand("/build inverse");
            if (GUILayout.Button("Terminal")) ExecuteCommand("/build terminal");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- ECONOMIE ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crédits:", GUILayout.Width(50));
            moneyInput = GUILayout.TextField(moneyInput);
            if (GUILayout.Button("SET")) ExecuteCommand($"/credit {moneyInput}");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Quota:", GUILayout.Width(50));
            quotaInput = GUILayout.TextField(quotaInput);
            if (GUILayout.Button("SET")) ExecuteCommand($"/quota {quotaInput}");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("--- ACHATS ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item:", GUILayout.Width(40));
            buyInput = GUILayout.TextField(buyInput);
            if (GUILayout.Button("BUY 1")) ExecuteCommand($"/buy {buyInput} 1");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pelle")) ExecuteCommand("/buy shovel");
            if (GUILayout.Button("Lampe")) ExecuteCommand("/buy pro");
            if (GUILayout.Button("Zap")) ExecuteCommand("/buy zap");
            if (GUILayout.Button("Shotgun")) ExecuteCommand("/buy shotgun");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("VENDRE TOUT")) ExecuteCommand("/sell");
        }

        // ==========================================
        // ONGLET 5: TROLL
        // ==========================================
        void DrawTrollAndFX()
        {
            GUILayout.Label("--- CHAT ---", labelStyle);
            chatInput = GUILayout.TextField(chatInput);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAY ")) ExecuteCommand($"/say 0 {chatInput}");
            if (GUILayout.Button("SIGNAL")) ExecuteCommand($"/signal {chatInput}");
            GUILayout.EndHorizontal();
            if (GUILayout.Button("CLEAR CHAT")) ExecuteCommand("/clear");

            GUILayout.Space(10);
            GUILayout.Label("--- ANNOY ---", labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Noise (s):", GUILayout.Width(60));
            noiseDuration = GUILayout.TextField(noiseDuration);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("NOISE SPAM (ALL)")) 
            {
                if (Helper.Players != null) 
                    foreach(var p in Helper.Players) ExecuteCommand($"/noise {p.playerUsername} {noiseDuration}");
            }
            
            if (GUILayout.Button("SPIN MAP (10s)")) ExecuteCommand("/spin 10");
            if (GUILayout.Button("STUN ENEMIES (5s)")) ExecuteCommand("/stun 5");

            GUILayout.Space(10);
            GUILayout.Label("--- CLIENT FX ---", labelStyle);
            // Ces toggles sont purement clients (MenuUI)
            fakeLag = GUILayout.Toggle(fakeLag, " Fake Lag");
            headSpin = GUILayout.Toggle(headSpin, " Head Spin (Normal)");
            rainbowScreen = GUILayout.Toggle(rainbowScreen, " Rainbow Screen");
            uiGlitch = GUILayout.Toggle(uiGlitch, " UI Glitch");
            
            GUILayout.Label($"Vitesse Spin: {headSpinSpeed:F0}");
            headSpinSpeed = GUILayout.HorizontalSlider(headSpinSpeed, 100f, 5000f);
            
            if (GUILayout.Button(extremeHeadSpin ? "STOP EXTREME SPIN" : "START EXTREME SPIN"))
            {
                extremeHeadSpin = !extremeHeadSpin;
                Notify(extremeHeadSpin ? "SPIN ACTIVÉ !" : "Spin arrêté.");
            }
        }

        // ==========================================
        // ONGLET 6: CONFIG
        // ==========================================
        void DrawSettings()
        {
            GUILayout.Label("--- MENU ---", labelStyle);
            notifications = GUILayout.Toggle(notifications, " Notifications");
            
            GUILayout.Label($"Taille UI: {uiScale:F1}");
            uiScale = GUILayout.HorizontalSlider(uiScale, 0.8f, 1.5f);

            GUILayout.Label("Thème:");
            int newTheme = GUILayout.Toolbar(themeIndex, new[] { "Dark", "Neon", "Classic", "Red" });
            if (newTheme != themeIndex)
            {
                themeIndex = newTheme;
                ApplyTheme();
            }
            
            GUILayout.Space(20);
            GUILayout.Label("--- PROTECTION ---", labelStyle);
            if (GUILayout.Button("Copier Lobby ID")) ExecuteCommand("/lobby");
            if (GUILayout.Button("Block Radar")) ExecuteCommand("/block radar");
            if (GUILayout.Button("Block Enemy Aim")) ExecuteCommand("/block enemy");

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("RESET ALL"))
                ResetConfig();
            GUI.backgroundColor = buttonColor;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        // Helper crucial pour éviter le spam : n'exécute la commande QUE si l'état change
        void DrawToggle(ref bool state, string label, string command)
        {
            bool newState = GUILayout.Toggle(state, label);
            if (newState != state)
            {
                state = newState;
                ExecuteCommand(command);
                Notify($"{command} {(state ? "ACTIVÉ" : "DÉSACTIVÉ")}");
            }
        }

        void ResetConfig()
        {
            godMode = infiniteStamina = noclip = unlimitedJump = esp = brightVision = false;
            stunClick = killClick = invis = hearAll = rapidUse = false;
            fakeLag = invertControls = headSpin = cameraShake = rainbowScreen = uiGlitch = timeJitter = fakeFreeze = notifSpam = drunkCamera = extremeHeadSpin = false;
            uiScale = speedHack = brightness = headSpinSpeed = 1f;
            Notify("Configuration réinitialisée.");
        }

        void Notify(string msg)
        {
            notif = msg;
            notifTimer = 3.0f;
        }

        void ExecuteCommand(string commandName)
        {
            // Ajoute le / automatiquement si manquant, mais Chat.ExecuteCommand gère souvent les deux
            if (!commandName.StartsWith("/")) commandName = "/" + commandName;
            Chat.ExecuteCommand(commandName);
        }

        void ApplyTheme()
        {
            switch (themeIndex)
            {
                case 0: // Dark
                    bgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                    buttonColor = new Color(0.2f, 0.2f, 0.2f);
                    textColor = Color.white;
                    accentColor = Color.cyan;
                    break;
                case 1: // Neon
                    bgColor = new Color(0.05f, 0.0f, 0.1f, 0.9f);
                    buttonColor = new Color(0.2f, 0.0f, 0.3f);
                    textColor = Color.green;
                    accentColor = Color.magenta;
                    break;
                case 2: // Classic
                    bgColor = new Color(0.8f, 0.8f, 0.8f, 0.95f);
                    buttonColor = Color.white;
                    textColor = Color.black;
                    accentColor = Color.blue;
                    break;
                case 3: // Red/Cyber
                    bgColor = new Color(0.1f, 0.0f, 0.0f, 0.95f);
                    buttonColor = new Color(0.3f, 0.0f, 0.0f);
                    textColor = Color.yellow;
                    accentColor = Color.red;
                    break;
            }
        }

        void InitStyles()
        {
            if (tabStyle != null) return;
            
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.fontStyle = FontStyle.Bold;
            windowStyle.alignment = TextAnchor.UpperCenter;

            tabStyle = new GUIStyle(GUI.skin.button);
            tabStyle.fontSize = 12;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.normal.textColor = accentColor;
            
            activeTab = new GUIStyle(tabStyle);
            activeTab.normal.textColor = accentColor;
            activeTab.fontStyle = FontStyle.Bold;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = accentColor;

            boxStyle = new GUIStyle(GUI.skin.box);
            
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.alignment = TextAnchor.MiddleCenter;
        }

        async void RunSpammWear(CancellationToken token)
        {
            string[] suits = new string[] { "armor", "hazmat", "spacesuit", "riot", "stealth" };
            try
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var suit in suits)
                    {
                        if (token.IsCancellationRequested) break;
                        ExecuteCommand($"/suit {suit}");
                        await System.Threading.Tasks.Task.Delay(100, token);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}