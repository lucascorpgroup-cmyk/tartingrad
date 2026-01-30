@echo off
setlocal enabledelayedexpansion
title Omni Injector - Updater
mode con: cols=85 lines=20
color 0B

:: --- CORRECTIF CRITIQUE ---
:: Force le script a s'executer dans le dossier ou il se trouve
cd /d "%~dp0"
:: --------------------------

echo.
echo  =============================================================
echo     OMNI INJECTOR - MISE A JOUR UNIQUEMENT
echo  =============================================================
echo.

:: 1. Verification si c'est bien un dossier Git
if not exist ".git" (
    color 0C
    echo  [ERREUR] Ce dossier n'est pas un depot Git valide.
    echo  Le dossier ".git" est introuvable.
    echo.
    echo  Appuyez sur une touche pour quitter...
    pause >nul
    exit /b
)

:: 2. Connexion et Fetch
echo  [1/3] Connexion au serveur GitHub...
git fetch origin main >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    color 0C
    echo.
    echo  [ERREUR] Impossible de contacter GitHub.
    echo  Verifiez votre connexion internet.
    echo.
    echo  Appuyez sur une touche pour quitter...
    pause >nul
    exit /b
)

:: 3. Verification des versions
for /f "delims=" %%i in ('git rev-parse HEAD') do set LOCAL=%%i
for /f "delims=" %%i in ('git rev-parse origin/main') do set REMOTE=%%i

echo  [2/3] Verification des versions...

if "!LOCAL!" == "!REMOTE!" (
    color 0A
    echo.
    echo  [OK] Aucune mise a jour necessaire.
    echo       Votre version est a jour.
) else (
    color 0E
    echo.
    echo  [UPDATE] Une nouvelle version est disponible !
    echo           Installation en cours...
    echo.
    
    :: Mise a jour
    git pull origin main
    
    echo.
    echo  [OK] Mise a jour terminee.
)

echo.
echo  =============================================================
echo  Fin de l'operation. Appuyez sur une touche pour fermer.
pause >nul