# Microsoft Store listing — copy/paste reference

Everything Partner Center will ask for during submission. Replace placeholders in `< >`.

## Reservation

**App name:** `CapsLockViewer`

## Product identity (auto-assigned by Store after reservation)

After clicking "Reserve product name" in Partner Center, copy these three values from **Apps and games → CapsLockViewer → Product management → Product identity**:

- Package/Identity/Name — e.g. `12345SomeID.CapsLockViewer`
- Package/Identity/Publisher — e.g. `CN=ABCD1234-...`
- Package/Properties/PublisherDisplayName — your Partner Center seller name

Paste them into `Package/Package.appxmanifest`, replacing:

```xml
Name="CapsLockViewer"
Publisher="CN=CapsLockViewer-Dev"
<PublisherDisplayName>CapsLockViewer</PublisherDisplayName>
```

Then rebuild with the msbuild command in the project README.

## Pricing and availability

- **Price:** Free
- **Markets:** All markets
- **Visibility:** Public
- **Discoverability:** Make this product available and discoverable in the Store
- **Schedule:** As soon as possible

## Properties

- **Category:** Productivity
- **Subcategory:** Personal finance → *no, pick* **Other**
- **Support contact info:** `remielshirazi@gmail.com`
- **Privacy policy URL:** `https://remieldev.github.io/CapsLockViewer/privacy-policy.html`
- **Website (optional):** `https://github.com/RemielDev/CapsLockViewer`
- **System requirements:** none beyond Windows 10 1809+
- **Product features (one-line bullets, up to 20):**
  - Solid cyan tray icon when Caps Lock is on, white outline when off
  - Uses ~10 MB of RAM (TrayStatus uses ~197 MB)
  - One right-click menu — Run at startup, Hide when off, Exit
  - No window, no console, no telemetry, no network access
  - Single-instance (extra launches exit silently)
  - Open source under MIT — see GitHub for the full source

## Age ratings

Click **Take questionnaire**. Answer:
- *Does your app or game contain any of the following...?* — **No** to every category.
- *Does your app share, collect, or transmit personal information?* — **No**
- *Does your app contain advertising?* — **No**
- *Does your app allow user-generated content?* — **No**

End result: ESRB **E (Everyone)**, PEGI **3**, USK **All ages**, etc.

## Store listing

### Description (short, ~200 chars max — used in search results)

```
A tiny system-tray Caps Lock indicator. Open source, ~10 MB RAM, no telemetry, no network access. Replacement for TrayStatus.
```

### Description (long, up to 10,000 chars)

```
CapsLockViewer puts a small icon in your system tray that turns solid cyan when Caps Lock is on and white when it's off. That's it. That's the whole app.

WHY
Most Caps Lock indicator apps for Windows are surprisingly heavy. TrayStatus is the popular one and uses ~197 MB of RAM to show one keyboard state. CapsLockViewer does the same job in about 10 MB.

FEATURES
• Solid cyan tray icon when Caps Lock is on
• Outlined white tray icon when Caps Lock is off (or hidden, if you prefer)
• Right-click menu: Run at startup, Hide icon when off, Exit
• Single-instance — extra launches exit silently
• Starts in milliseconds, lives in your tray, polls the Caps Lock state every 150 ms
• Zero windows, zero consoles, zero notifications, zero telemetry, zero network access

OPEN SOURCE
The full source is on GitHub under MIT license: github.com/RemielDev/CapsLockViewer
Every behavior is auditable. The whole app is one C# source file.

NO DATA COLLECTION
Nothing about you or your keystrokes leaves your machine. The app reads exactly one bit of state (Caps Lock on/off) using the standard Windows GetKeyState API. It does not hook the keyboard. See the privacy policy linked from this listing.

REQUIREMENTS
Windows 10 version 1809 or newer, Windows 11. x64 only.
```

### What's new (release notes)

```
Initial release.
- Tray icon for Caps Lock state (cyan = on, white = off)
- Run at startup via Windows StartupTask
- Hide-when-off option
- ~10 MB RAM
```

### Search terms (up to 7)

```
caps lock indicator
caps lock tray
caps lock viewer
tray status
keyboard indicator
caps lock notifier
caps lock light
```

### Copyright and trademark info

```
© 2026 Remiel Shirazi. Released under the MIT license.
```

### Additional license terms

```
https://github.com/RemielDev/CapsLockViewer/blob/main/LICENSE
```

## Screenshots required

Need ≥1 desktop screenshot. Required size: 1366×768 minimum, 3840×2160 maximum. PNG or JPG.

Suggested set (after sideloading or publishing the unsigned MSIX privately):

1. **Tray with icon visible.** Crop the Windows 11 taskbar so the cyan "A" icon shows. Add a label like "Caps Lock ON."
2. **Right-click menu open.** Same view with the context menu showing the three options.
3. **Side-by-side comparison.** Task Manager showing CapsLockViewer at ~10 MB next to TrayStatus at ~197 MB. Optional but compelling.

Take screenshots with Snipping Tool (Win+Shift+S). Save as PNG at native resolution.

## Store logo

Already generated in `Package/Assets/StoreLogo.png` (50×50). The Partner Center submission also pulls Square150x150Logo, Square71x71Logo, Square310x310Logo, Wide310x150Logo, and SplashScreen from the appxmanifest at upload time — no separate upload needed.

## Submission options

- **Mandatory update:** No
- **Notes for certification:** *(paste this so the reviewer doesn't get confused)*
  ```
  Tray-only app. No main window. After install, launch from the Start menu — the only UI is a small "A" icon in the system tray (you may need to drag it from the overflow flyout). Right-click the icon for the menu.
  ```
- **Restricted capabilities declared:** `runFullTrust` — required because this is a packaged Win32 app rather than a UWP. Explanation field:
  ```
  This is a Windows desktop application packaged with MSIX. It uses runFullTrust because GetKeyState is a Win32 user32.dll API not exposed to UWP. The app does not access network, files outside its install location, or any user data.
  ```

## Final checklist before clicking Submit

- [ ] `Package.appxmanifest` Identity Name + Publisher updated with Partner Center values
- [ ] Rebuilt MSIX bundle after updating manifest
- [ ] At least 1 desktop screenshot uploaded
- [ ] Privacy policy URL is live (check https://remieldev.github.io/CapsLockViewer/privacy-policy.html responds 200)
- [ ] Age rating questionnaire completed
- [ ] Description copy pasted
- [ ] Notes for certification filled in (otherwise reviewer will fail it for "no UI")
