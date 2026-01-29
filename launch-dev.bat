@echo off
setlocal enabledelayedexpansion
title Omni Injector - Dev Builder

:: Indispensable : Force le script à travailler dans son propre dossier
cd /d "%~dp0"

:begin
cls
color 0B
echo ========================================================
echo              OMNI INJECTOR - DEV BUILDER
echo ========================================================
echo.

echo [1/3] Recherche du fichier projet...

:: -- CONFIGURATION DU CHEMIN CIBLE --
:: On cherche d'abord a l'endroit precis
set "target=omni-injector\omni-injector.csproj"

if exist "!target!" (
    set "project_file=!target!"
) else (
    :: Securite : Recherche recursive
    echo [INFO] Pas trouve dans omni-injector\, recherche globale...
    for /f "delims=" %%f in ('dir /s /b omni-injector.csproj 2^>nul') do set "project_file=%%f"
)

:: Si toujours rien, on cherche l'ancien nom
if not defined project_file (
    for /f "delims=" %%f in ('dir /s /b lc-hax.csproj 2^>nul') do set "project_file=%%f"
)

if not defined project_file (
    color 0C
    echo.
    echo [ERREUR CRITIQUE] Fichier projet introuvable !
    echo Chemin cible tente : !target!
    echo.
    echo Fichiers presents ici :
    dir /b
    pause
    goto begin
)

echo [OK] Fichier trouve : !project_file!
echo.

echo [2/3] Compilation...
echo ---------------------------------------

:: Nettoyage
if exist "bin\omni-injector.dll" del "bin\omni-injector.dll" >nul 2>&1
if exist "bin\lc-hax.dll" del "bin\lc-hax.dll" >nul 2>&1

:: Compilation
dotnet build "!project_file!" -c Release -restoreProperty:RestoreLockedMode=true

if %ERRORLEVEL% NEQ 0 (
    color 0C
    echo.
    echo [ERREUR] La compilation a echoue.
    pause
    goto begin
)

echo.
echo [3/3] Injection dans Lethal Company...
echo ---------------------------------------

:: Detection du DLL
set "dll_to_inject=bin\omni-injector.dll"

if not exist "!dll_to_inject!" (
    :: Fallback sur lc-hax.dll
    if exist "bin\lc-hax.dll" set "dll_to_inject=bin\lc-hax.dll"
)

if not exist "!dll_to_inject!" (
    :: Recherche de secours
    for /f "delims=" %%f in ('dir /s /b omni-injector.dll 2^>nul') do set "dll_to_inject=%%f"
)

if not exist "!dll_to_inject!" (
    color 0C
    echo [ERREUR] DLL introuvable apres compilation.
    pause
    goto begin
)

echo [OK] DLL pret : !dll_to_inject!

:: Verification Injecteur
set "injector=submodules\SharpMonoInjectorCore\dist\SharpMonoInjector.exe"
if not exist "!injector!" (
    echo [INFO] Compilation de l'injecteur...
    dotnet build "submodules\SharpMonoInjectorCore\SharpMonoInjector.csproj" -c Release -o "submodules\SharpMonoInjectorCore\dist" >nul 2>&1
)

color 0A
start /wait /b "" "!injector!" inject -p "Lethal Company" -a "!dll_to_inject!" -n Hax -c Loader -m Load

echo.
echo [INFO] Cycle termine. Appuyez sur une touche pour recompiler.
pause >nul
goto begin