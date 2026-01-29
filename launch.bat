@echo off
setlocal enabledelayedexpansion
title Omni Injector Launcher
cls
echo check des mises a jours omni injector...

:: 1. On recupere les infos de GitHub
git fetch origin main >nul 2>&1

:: Verification si git fetch a fonctionne (internet, acces...)
if %ERRORLEVEL% NEQ 0 (
    echo [ERREUR] Impossible de contacter GitHub. Verifiez votre connexion.
    echo Lancement de la version locale...
    goto :lancement
)

:: 2. On recupere les Hash (ID unique des versions)
for /f "delims=" %%i in ('git rev-parse HEAD') do set LOCAL=%%i
for /f "delims=" %%i in ('git rev-parse origin/main') do set REMOTE=%%i

:: 3. Comparaison
if "!LOCAL!" == "!REMOTE!" (
    echo Vous etes a jour.
) else (
    echo [!] Mise a jour omni injector detectee !
    echo Installation en cours...
    git pull --rebase --autostash origin main
    echo Mise a jour terminee avec succes.
)

:lancement
echo.
echo Lancement de Omni Injector...
timeout /t 2 >nul
call launch-dev.bat

:: Si jamais launch-dev.bat plante, ceci gardera la fenetre ouverte
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [CRASH] Le programme s'est ferme avec une erreur.
    pause
)
