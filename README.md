# capslockviewer

a tiny caps lock indicator that sits in your windows system tray. cyan "A" when caps is on, white outline when it's off. uses about 10 mb of ram.

i built this because every other caps lock app i found was weirdly heavy — traystatus eats ~197 mb to show one keyboard bit. this does the same thing in ~10.

![ON state](preview/on-128.png) ![OFF state](preview/off-128.png)

made by remiel shirazi. mit licensed, source is all here.

## run it from source

needs the [.net 8 sdk](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet run --project CapsLockViewer.csproj -c Release
```

## build a standalone exe

```powershell
dotnet publish CapsLockViewer.csproj -c Release
```

drops a self-contained exe at `bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/CapsLockViewer.exe` (~74 mb, no runtime needed on the target machine).

## build the msix (store / sideload)

needs [visual studio 2022](https://visualstudio.microsoft.com/) or the build tools with the **windows application packaging** workload.

```powershell
msbuild Package\CapsLockViewer.Package.wapproj /p:Configuration=Release /p:Platform=x64 /restore
```

output lands at `Package/AppPackages/CapsLockViewer.Package_1.0.0.0_x64.msixbundle`.

before submitting to the store, swap `Identity Name` and `Publisher` in `Package/Package.appxmanifest` for the values from your partner center reservation, then rebuild.

## regenerate the icons / store tiles

```powershell
dotnet run --project CapsLockViewer.csproj -c Release -- --export-preview preview
dotnet run --project CapsLockViewer.csproj -c Release -- --export-store-assets Package\Assets
```

## how it actually works

- one file — `Program.cs`. `TrayContext` runs the tray icon, `IconFactory` draws the icons in memory.
- caps state is polled with `user32!GetKeyState(VK_CAPITAL)` on a 150 ms timer. no keyboard hook.
- only one copy runs at a time (named mutex). extra launches just exit.
- auto-start uses the msix `startupTask` when packaged, or `HKCU\...\Run` when it's a plain exe.
- settings live in `LocalSettings` (packaged) or `HKCU\Software\CapsLockViewer` (unpackaged).
- calls `EmptyWorkingSet` now and then to keep memory down near 10 mb.

## layout

```
CapsLockViewer.csproj             the .net 8 winforms project
Program.cs                        all the code
app.manifest                      win32 manifest (asInvoker, longPathAware)
Package/                          msix packaging project + store tiles
preview/                          tray icon pngs for this readme
```

## contributing

keep it small. the whole thing is one source file and i'd like it to stay that way. open an issue before anything bigger than a bug fix.

## license

mit — see [LICENSE](LICENSE). © 2026 remiel shirazi.
