# Sideload-installs the signed MSIX locally for testing.
# Must be run as Administrator (the dev cert has to land in Trusted Root).
# After Store submission, end users don't need any of this — the Store-signed package installs without trust steps.

#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "Importing dev cert into LocalMachine\Root..." -ForegroundColor Cyan
Import-Certificate -FilePath ".\CapsLockViewer.Package_Dev.cer" -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null

Write-Host "Installing MSIX bundle..." -ForegroundColor Cyan
$msix = Resolve-Path ".\AppPackages\CapsLockViewer.Package_1.0.0.0_Test\CapsLockViewer.Package_1.0.0.0_x64.msixbundle"
Add-AppxPackage -Path $msix

Write-Host "Installed. Look for CapsLockViewer in the Start menu (or just launch via tray)." -ForegroundColor Green
Write-Host "Uninstall later with: Get-AppxPackage *CapsLockViewer* | Remove-AppxPackage"
