@echo off
setlocal enabledelayedexpansion

:: ==========================================
:: CONFIGURATION ESTHETIQUE
:: ==========================================
title OMNI INJECTOR - AUTO UPDATE
:: Couleur Cyan (0B) pour rester cohérent avec le Builder
color 0B
cls

:: ==========================================
:: BANNIERE ASCII
:: ==========================================
echo.
echo  ============================================================
echo   ___  __  __ _   _ ___   ___ _   _     _  _____ _____ ___  ____  
echo  / _ \^|  \/  ^| \ ^| ^|_ _^| ^|_ _^| \ ^| ^|   ^| ^|^| ____^|_   _/ _ \^|  _ \ 
echo ^| ^| ^| ^| ^|\/^| ^|  \^| ^|^| ^|    ^| ^| ^|  \^| ^|_  ^| ^|^|  _^|   ^| ^|^| ^| ^| ^| ^|_) ^|
echo ^| ^|_^| ^| ^|  ^| ^| ^|\  ^|^| ^|    ^| ^| ^| ^|\  ^| ^|_^| ^|^| ^|___  ^| ^|^| ^|_^| ^|  _ ^< 
echo  \___/^|_^|  ^|_^|_^| \_^|___^|  ^|___^|_^| \_^|\___/^|_____^| ^|_^| \___/^|_^| \_\
echo.
echo                 [ SYSTEME DE MISE A JOUR ]
echo  ============================================================
echo.

:: ==========================================
:: ETAPE 1 : CONNEXION SERVEUR
:: ==========================================
echo  [*] Verification de la connexion au serveur GitHub...
git fetch origin main >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo.
    color 0C
    echo  [!] ERREUR DE CONNEXION : Impossible de joindre GitHub.
    echo      Le programme va demarrer en mode HORS-LIGNE.
    timeout /t 3 >nul
    color 0B
    goto :lancement
)

:: ==========================================
:: ETAPE 2 : VERIFICATION DE VERSION
:: ==========================================
:: Récupération des identifiants de version (Hash)
for /f "delims=" %%i in ('git rev-parse HEAD') do set LOCAL=%%i
for /f "delims=" %%i in ('git rev-parse origin/main') do set REMOTE=%%i

echo  [*] Analyse des versions locale et distante...

if "!LOCAL!" == "!REMOTE!" (
    echo.
    color 0A
    echo  [+] SYSTEME A JOUR. Aucune action requise.
    echo      Hash: !LOCAL:~0,7!
    timeout /t 1 >nul
    color 0B
) else (
    echo.
    echo  [!] NOUVELLE VERSION DETECTEE !
    echo      Locale  : !LOCAL:~0,7!
    echo      Distante: !REMOTE:~0,7!
    echo.
    echo  [*] Telechargement et installation de la mise a jour...
    echo  -------------------------------------------------------
    
    :: Mise à jour avec autostash pour protéger tes modifs locales
    git pull --rebase --autostash origin main
    
    echo.
    echo  [+] Mise a jour installee avec succes.
    echo  -------------------------------------------------------
)

:: ==========================================
:: ETAPE 3 : LANCEMENT
:: ==========================================
:lancement
echo.
echo  ============================================================
echo  [*] INITIALISATION DE OMNI INJECTOR...
echo  ============================================================
echo.

timeout /t 2 >nul

:: Appel du script principal
call launch-dev.bat

:: Si launch-dev.bat se termine, on garde la fenêtre ouverte
pause