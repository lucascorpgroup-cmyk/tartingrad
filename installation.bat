@echo off
setlocal enabledelayedexpansion

:: ==========================================
:: CONFIGURATION ESTHETIQUE
:: ==========================================
title OMNI INJECTOR BUILDER
:: Couleur Cyan sur fond noir (0B) pour un look moderne
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
echo                 [ OMNI INJECTOR BUILDER ]
echo  ============================================================
echo.

:: ==========================================
:: ETAPE 1 : GIT SUBMODULES
:: ==========================================
echo  [*] Verification des sous-modules Git...
echo  ----------------------------------------

git submodule update --init

:: Gestion d'erreur personnalisée pour Git
if %errorlevel% neq 0 (
    echo.
    color 0C
    echo  [!] ERREUR CRITIQUE : Impossible de mettre a jour les sous-modules.
    echo      Verifiez que Git est installe et accessible.
    echo.
    pause
    exit /b
)

echo.
echo  [+] Sous-modules synchronises avec succes.
echo.

:: ==========================================
:: ETAPE 2 : DOTNET PUBLISH
:: ==========================================
echo  [*] Demarrage de la compilation (SharpMonoInjectorCore)...
echo      Veuillez patienter, cela peut prendre quelques minutes...
echo  ----------------------------------------
echo.

dotnet publish submodules/SharpMonoInjectorCore

:: Gestion d'erreur personnalisée pour Dotnet
if %errorlevel% neq 0 (
    echo.
    color 0C
    echo  [!] ERREUR DE COMPILATION.
    echo      Verifiez les logs ci-dessus pour plus de details.
    echo.
    pause
    exit /b
)

:: ==========================================
:: FIN
:: ==========================================
echo.
echo  ============================================================
echo  [+] BUILD TERMINE AVEC SUCCES !
echo  ============================================================
echo.
color 0A
pause