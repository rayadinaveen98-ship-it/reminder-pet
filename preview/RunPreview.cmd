@echo off
REM Uses the installed Windows PowerShell for this one-time transparent overlay preview.
REM The policy switch applies to this process only; no system policy or registry is changed.
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File "%~dp0ShowPetPreview.ps1"
