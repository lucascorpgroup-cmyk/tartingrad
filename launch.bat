@echo off
setlocal enabledelayedexpansion
title Omni Injector - Auto Update
cls

echo [1/3] Verification de la connexion au serveur GitHub...
git fetch origin main >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo [ERREUR] Impossible de joindre GitHub.
    echo Le programme va se lancer avec la version actuelle.
    goto :lancement
)

:: Récupération des identifiants de version (Hash)
for /f "delims=" %%i in ('git rev-parse HEAD') do set LOCAL=%%i
for /f "delims=" %%i in ('git rev-parse origin/main') do set REMOTE=%%i

if "!LOCAL!" == "!REMOTE!" (
    echo [2/3] Aucune mise a jour trouvee. Votre version est a jour.
) else (
    echo [2/3] Une nouvelle version est disponible !
    echo        Mise a jour en cours...
    
    :: Cette commande met tes modifs de côté, met à jour, et remet tes modifs
    git pull --rebase --autostash origin main
    
    echo [OK] Mise a jour terminee avec succes.
)

:lancement
echo.
echo [3/3] Lancement de Omni Injector...
echo.
timeout /t 2 >nul
call launch-dev.bat

pause
