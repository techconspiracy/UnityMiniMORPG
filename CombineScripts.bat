@echo off
setlocal enabledelayedexpansion

:: 1. Define the absolute path to your Core directory
set "BASE_DIR=C:\Users\techc\Desktop\Create with Code\UAsset_MMORPG\Assets"
set "OUTPUT=%BASE_DIR%\Scripts\Game\Core\Monolith.md"

:: 2. Create/Clear the file (ensures it is not 0 bytes if data is found)
type nul > "%OUTPUT%"

:: 3. Process each specific subfolder
for %%D in (Editor Scripts Game Core Pooling Systems UI) do (
    echo Entering: %%D...
    
    :: Move into the subdirectory temporarily
    pushd "%BASE_DIR%\%%D" 2>nul
    if !errorlevel! equ 0 (
        :: Search for .cs files in this folder and all subfolders
        for /r %%f in (*.cs) do (
            echo Adding: %%~nxf
            
            :: Header: #####FileName.cs#####
            echo #####%%~nxf##### >> "%OUTPUT%"
            
            :: Content
            type "%%f" >> "%OUTPUT%"
            
            :: Spacing
            echo. >> "%OUTPUT%"
            echo. >> "%OUTPUT%"
        )
        :: Return to the BASE_DIR
        popd
    ) else (
        echo ERROR: Could not find folder %%D inside Core.
    )
)

echo.
echo Process Complete. Check "%OUTPUT%"
pause
