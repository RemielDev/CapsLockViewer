# CapsLockViewer

A tiny Caps Lock indicator for the Windows system tray. Uses ~10 MB of RAM.

![ON state](preview/on-128.png) ![OFF state](preview/off-128.png)

## Run from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet run --project CapsLockViewer.csproj -c Release
```

## Build standalone exe

```powershell
dotnet publish CapsLockViewer.csproj -c Release
```

Output: `bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/CapsLockViewer.exe` (self-contained, ~74 MB, no .NET runtime required on the target machine).

## Build MSIX (for Microsoft Store / sideload)

Requires [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community is fine) or [VS Build Tools 2022](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022) with the **Windows Application Packaging** workload.

```powershell
msbuild Package\CapsLockViewer.Package.wapproj /p:Configuration=Release /p:Platform=x64 /restore
```

Output: `Package/AppPackages/CapsLockViewer.Package_1.0.0.0_x64.msixbundle`.

For Store submission, replace `Identity Name` and `Publisher` in `Package/Package.appxmanifest` with the values from your Partner Center reservation.

## Regenerate icon previews / Store tiles

```powershell
dotnet run --project CapsLockViewer.csproj -c Release -- --export-preview preview
dotnet run --project CapsLockViewer.csproj -c Release -- --export-store-assets Package\Assets
```

## How it works

- Single `Program.cs` — `TrayContext` (ApplicationContext + NotifyIcon) and `IconFactory` (renders multi-resolution ICOs in memory).
- Caps Lock state polled via `user32!GetKeyState(VK_CAPITAL)` on a 150 ms `WinForms.Timer`.
- Single-instance enforced by a named `Mutex`.
- Auto-start uses `windows.startupTask` when packaged (MSIX) and `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` when run as a plain exe. `Program.IsPackaged` decides at startup.
- Settings stored in `ApplicationData.Current.LocalSettings` (packaged) or `HKCU\Software\CapsLockViewer` (unpackaged).
- `psapi!EmptyWorkingSet` is called after launch and periodically to keep the working set near 10 MB.

## Project layout

```
CapsLockViewer.csproj             standalone .NET 8 WinForms project
Program.cs                        all source
app.manifest                      win32 manifest (asInvoker, longPathAware)
Package/
  CapsLockViewer.Package.wapproj  MSIX packaging project (referenced by VS / msbuild)
  Package.appxmanifest            MSIX manifest, declares the startupTask
  Assets/                         Store tile PNGs (regenerated from source)
preview/                          tray icon PNGs for the README
```

## Contributing

PRs welcome. Keep it tiny — the whole app fits in one source file and that's the bar. Open an issue first for anything beyond a bug fix.

## License

MIT. See [LICENSE](LICENSE).
