# Run the app

`PokeJuegos.Mobile` runs on Windows and on Android. Start the API (and the web, for the email links) first: from
Visual Studio, with `dotnet run` or with [Docker](DOCKER.md).

## Windows (fastest way to try it, no emulator needed)

```bash
dotnet build PokeJuegos.Mobile -t:Run -f net10.0-windows10.0.19041.0
```

In Visual Studio: open the dropdown next to the Run button, go to **Framework**, pick `net10.0-windows10.0.19041.0`,
then run **Windows Machine**.

## Android emulator

```bash
dotnet build PokeJuegos.Mobile -t:Run -f net10.0-android
```

The app calls the API at `http://localhost:5224` on every platform. On Android, Debug deploys run
`adb reverse tcp:5224 tcp:5224` automatically (an MSBuild target in the `.csproj`), so the emulator's `localhost`
reaches your machine. Google OAuth only accepts `localhost` as a development redirect, which is why the app doesn't
use `10.0.2.2`. If the emulator or adb restarts while the app is running, run that command again (`adb` is in
`<Android SDK>\platform-tools`).

## Everything at once in Visual Studio

The solution includes two shared launch profiles (`PokeJuegos.slnLaunch`). Pick one in the startup dropdown next to
the Run button:

| Profile | Starts |
|---|---|
| **API + WEB + WinUI** | API, web, and the app as a Windows desktop app |
| **API + WEB + MOBILE** | API, web, and the app on the Android emulator |

The Android profile targets an emulator named *Pixel 7 - API 36.0 (Android 16.0 - API 36)*. If yours has a different
name, pick your device in *Configure Startup Projects* for that profile.
