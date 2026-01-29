@echo off
cls
echo check des mises a jours omni injector...

:: On recupere les dernieres infos de GitHub sans telecharger les fichiers
git fetch origin main >nul 2>&1

:: On recupere l'identifiant (le hash) du dernier commit local
for /f %%i in ('git rev-parse HEAD') do set LOCAL=%%i
:: On recupere l'identifiant du dernier commit sur GitHub
for /f %%i in ('git rev-parse origin/main') do set REMOTE=%%i

if "%LOCAL%" == "%REMOTE%" (
    echo Vous etes a jour
) else (
    echo Mise a jour omni injector...
    :: On fait le pull silencieusement
    git pull --rebase --autostash origin main >nul 2>&1
    echo Mise a jour terminee.
)

:: Lancement du programme
call launch-dev.bat
