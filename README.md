# PvZ Windows Phone — Decompilation + Desktop/Mobile Port

![build](https://github.com/IsraelDXPP/PvZ-Windows-Phone-decomp/actions/workflows/build.yml/badge.svg)

Decompilation of the **Plants vs. Zombies — Windows Phone 7/8 (XNA)** release
(`LAWN.dll`, managed .NET — no obfuscation), re-targeted to modern .NET 8
with MonoGame so it builds and runs on **Windows, Linux, Android** (and iOS
project files included).

> **Disclaimer:** all game code, art and audio belong to PopCap/EA.
> This repo contains the decompiled/ported *code* for research and
> preservation purposes. You must own the game (the WP8 `.xap`) to supply
> the assets. Not affiliated with PopCap/EA.

## Status

| Platform | Project | State |
|---|---|---|
| Windows (x64) | `LAWN.Desktop.csproj` | ✅ Builds, runs windowed 800×480 |
| Linux (x64) | `LAWN.Desktop.csproj` (`-r linux-x64`) | ✅ Publishes (self-contained) |
| macOS (arm64) | `LAWN.Desktop.csproj` (`-r osx-arm64`) | ✅ Publishes via CI (untested locally) |
| Android | `LAWN.Android.csproj` | ✅ Builds APK (needs device test) |
| iOS | `LAWN.iOS.csproj` | ✅ Device IPA via CI (ad-hoc signed, Full AOT — install on jailbroken phones with TrollStore/Sideloadly; local build needs a Mac + Xcode 26) |

Every push/PR runs all six builds on GitHub Actions
(`.github/workflows/build.yml`: Windows, Linux, macOS, Android, iOS
simulator + an asset-pipeline check). Release artifacts (game folders,
APK, iOS `.app`) are uploaded per run. macOS runners bill at 10× minutes,
so avoid re-running CI gratuitously.

Port fixes vs. the original binary:
- Windowed mode (was fullscreen-only), resizable window, mouse emulated as touch.
- Content loading moved to the main thread (OpenGL can't create textures on a worker thread — the game used to hang on the PopCap logo).
- Music: original songs are WMA, which MonoGame/DesktopGL can't decode → converted to OGG Vorbis (same filenames, see below).
- Achievements work offline (original wall `Draw` reconstructed 1:1 from the binary IL).
- Leaderboards work offline: local best scores persisted in `localboards.dat`.
- WP7-only APIs (`Microsoft.Phone.*`, Xbox Live `GamerServices`) replaced by no-op stubs in `Compat/Stubs.cs`.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (10 works too)
- Your own `*.xap` of PvZ WP8 v1.4.x
- `ffmpeg` (only to convert the music once)
- Android: JDK 17+, Android SDK + `.NET android` workload
- iOS: a Mac with Xcode

## Game assets

The `Content/` folder, `resources.xml`, `todresources.xml` and
`LawnStrings_*.txt` are **not** in this repo (copyrighted). Get them from
the game bundle — manually from your `.xap` (it's a ZIP), or automatically
with the included script (downloads the bundle from Google Drive):

```powershell
python tools/fetch_assets.py --dest .
```

It extracts `Content/`, the XML/TXT files and converts the music to OGG
(requires `ffmpeg` on PATH). CI does exactly this on every run.

```
LAWN.Desktop.csproj
Content/            <- from the .xap
resources.xml       <- from the .xap
todresources.xml    <- from the .xap
LawnStrings_*.txt   <- from the .xap
```

Then convert the music (MonoGame/DesktopGL only plays OGG for songs):

```powershell
ffmpeg -y -i Content\music\crazydave.wma -c:a libvorbis -q:a 4 Content\music\crazydave.ogg
Move-Item Content\music\crazydave.ogg Content\music\crazydave.wma -Force
# repeat for every *.wma in Content\music (keep a backup!)
```

(The `.xnb` song files only store the file name, so keeping the `.wma`
filename with OGG data inside just works.)

## Build & run

```powershell
# Windows
dotnet run --project LAWN.Desktop.csproj

# Linux (self-contained)
dotnet publish LAWN.Desktop.csproj -c Release -r linux-x64 --self-contained
./bin/Release/net8.0/linux-x64/publish/LAWN

# Android (APK in bin/Release/.../com.israeldxpp.pvzdecomp-Signed.apk)
dotnet build LAWN.Android.csproj -c Release

# iOS device IPA, ad-hoc signed for jailbreak install (TrollStore/Sideloadly).
# Needs a Mac; CI builds it automatically (see Actions artifacts).
dotnet publish LAWN.iOS.csproj -c Release -f net9.0-ios -r ios-arm64 -p:BuildIpa=true -p:CodesignKey=-
```

Or open `LAWN.sln` in Visual Studio / Rider.

## Controls

- Mouse = touch (click / drag).
- Get a `pvz_load.log` next to the exe with loading diagnostics.

## Project structure

```
Lawn/            game logic (Board, Plant, Zombie, UI screens...)
Sexy/            PopCap framework (widgets, graphics, audio...)
Sexy.TodLib/     Tod engine (reanim, particles, trails, resources)
Compat/Stubs.cs  desktop/mobile stubs for Phone + Xbox Live APIs
Platforms/       Android (MainActivity) / iOS (AppDelegate) entry points
```

## Known issues

- No music/SFX volume on first launch until a song plays (inherited).
- Android/iOS have not been tested on device — touch uses the native
  `TouchPanel` path there; reports welcome.
