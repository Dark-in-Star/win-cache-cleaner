@echo off
title Windows Cache & Temp Cleaner
color 0A

echo ==========================================
echo   Cleaning Windows Cache and Temp Files
echo ==========================================
echo.

:: Run as admin check
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Please run this BAT file as Administrator.
    pause
    exit
)

echo [1/16] Cleaning User Temp...
del /s /f /q "%temp%\*" >nul 2>&1
for /d %%x in ("%temp%\*") do rd /s /q "%%x" >nul 2>&1

echo [2/16] Cleaning Windows Temp...
del /s /f /q "C:\Windows\Temp\*" >nul 2>&1
for /d %%x in ("C:\Windows\Temp\*") do rd /s /q "%%x" >nul 2>&1

echo [3/16] Cleaning Prefetch...
del /s /f /q "C:\Windows\Prefetch\*" >nul 2>&1

echo [4/16] Cleaning Software Distribution Download Cache...
del /s /f /q "C:\Windows\SoftwareDistribution\Download\*" >nul 2>&1
for /d %%x in ("C:\Windows\SoftwareDistribution\Download\*") do rd /s /q "%%x" >nul 2>&1

echo [5/16] Cleaning Recycle Bin...
PowerShell.exe -NoProfile -Command "Clear-RecycleBin -Force" >nul 2>&1

echo [6/16] Flushing DNS Cache...
ipconfig /flushdns >nul

echo [7/16] Cleaning Thumbnail Cache...
del /s /f /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*" >nul 2>&1

echo [8/16] Cleaning Recent Files...
del /s /f /q "%APPDATA%\Microsoft\Windows\Recent\*" >nul 2>&1

echo [9/16] Cleaning DirectX Shader Cache...
del /s /f /q "%LocalAppData%\D3DSCache\*" >nul 2>&1

echo [10/16] Cleaning Windows Error Reports...
del /s /f /q "C:\ProgramData\Microsoft\Windows\WER\ReportArchive\*" >nul 2>&1
for /d %%x in ("C:\ProgramData\Microsoft\Windows\WER\ReportArchive\*") do rd /s /q "%%x" >nul 2>&1
del /s /f /q "C:\ProgramData\Microsoft\Windows\WER\ReportQueue\*" >nul 2>&1
for /d %%x in ("C:\ProgramData\Microsoft\Windows\WER\ReportQueue\*") do rd /s /q "%%x" >nul 2>&1

echo [11/16] Cleaning Memory Dumps...
del /s /f /q "C:\Windows\Minidump\*" >nul 2>&1
del /s /f /q "%LocalAppData%\CrashDumps\*" >nul 2>&1

echo [12/16] Cleaning CBS Logs...
del /s /f /q "C:\Windows\Logs\CBS\*" >nul 2>&1

echo [13/16] Cleaning DISM Logs...
del /s /f /q "C:\Windows\Logs\DISM\*" >nul 2>&1

echo [14/16] Cleaning Delivery Optimization Cache...
del /s /f /q "C:\Windows\SoftwareDistribution\DeliveryOptimization\*" >nul 2>&1
for /d %%x in ("C:\Windows\SoftwareDistribution\DeliveryOptimization\*") do rd /s /q "%%x" >nul 2>&1

echo [15/16] Cleaning Icon Cache...
del /s /f /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*" >nul 2>&1

echo [16/16] Running Disk Cleanup...
cleanmgr /sagerun:1

echo.
echo ==========================================
echo      Cleanup Completed Successfully
echo ==========================================
echo.

pause
