---
name: maui
description: Conventions, gotchas and workflow for PokeJuegos.Mobile, the .NET MAUI app (.NET 10, Android + Windows/WinUI). Use whenever adding or changing pages, view models, controls, styles, images/SVGs, platform code, local SQLite data or API calls in the mobile app, or when debugging a crash, freeze or Release-only bug in it.
---

# PokeJuegos.Mobile (.NET MAUI)

Targets `net10.0-android` and `net10.0-windows10.0.19041.0` (unpackaged, `WindowsPackageType=None`). Android is the
real target; Windows is for fast iteration. Portrait only on Android; status bar hidden, navigation bar visible.

## Structure

```
Views/           <Name>Page.xaml (+ .xaml.cs: InitializeComponent + BindingContext = injected view model)
ViewModels/      <Name>ViewModel.cs
Controls/        Reusable ContentViews (FormEntry, SearchBox, OptionPicker, ModalView, ConfirmModalView,
                 ErrorView, PokemonCard) and GraphicsView drawables (BurstBackground, FuseBar, ConfettiView)
Services/        Interface + implementation (HttpAuthService, SessionService, LocalDatabase, FavoritesService,
                 TopScoresService, PokeApiService, ShellNavigationService...)
Models/          App models, SQLite tables, AuthResult, AuthJson (System.Text.Json source-gen context)
Platforms/       Android (MainActivity, WebAuthenticator callback) / Windows (LoopbackOAuthBrowser)
Resources/       Styles/Colors.xaml + Styles.xaml, Images/ (SVG icons, Sprites/*.webp), Fonts/, Raw/ (Pokémon JSON)
scripts/         Python generators for Resources/Raw/*.json and sprites (gen I–III, 386 Pokémon)
Routes.cs        Route name constants;  AppShell.xaml.cs registers them;  MauiProgram.cs registers DI
```

## Code conventions

- **Tabs** for indentation (unlike the backend). **Explicit types**, no `var`. XML `<summary>` on public members.
- **MVVM with CommunityToolkit.Mvvm**: `public partial class XViewModel(IDep dep, ...) : ObservableObject`,
  properties as `[ObservableProperty] public partial string Email { get; set; } = string.Empty;`, commands with
  `[RelayCommand] private async Task SendAsync()` (bind to `SendCommand`; `SendCommand.IsRunning` drives spinners).
  Use `partial void OnXChanged(...)` for reactions, `[NotifyPropertyChangedFor]` for derived properties.
- **Compiled bindings everywhere**: `x:DataType` on every page, `DataTemplate` and control. Non-compilable bindings
  and trim-unsafe reflection are build errors (`WarningsAsErrors` in the `.csproj`) so Release (trimmed) can't diverge
  from Debug. JSON goes through the source-generated context in `Models/AuthJson.cs`; add new DTOs there.
- **Navigation** only via `INavigationService` + `Routes` constants. Register new pages and view models as
  `AddTransient` in `MauiProgram.cs`, services as `AddSingleton`, route in `AppShell.xaml.cs`. `//general` resets
  the stack. Platform-specific implementations with `#if ANDROID / #elif WINDOWS` in `MauiProgram`.
- **Reuse `PokeJuegos.DTO`** for API requests/responses. `ApiSettings.BaseUrl` must stay `http://localhost:5224/`.
- **Texts are never literals.** English in `Resources/Strings/AppResources.resx`, Spanish (neutral, "tú") in
  `AppResources.es.resx`; XAML uses `{x:Static strings:AppResources.Key}`, C# `AppResources.Key`. Add the key to both
  `.resx` files and to `AppResources.Designer.cs` (committed; Visual Studio regenerates it when the `.resx` is saved;
  MSBuild-time generation doesn't work because WinUI pre-compiles XAML first). API errors arrive as codes:
  `AuthResult` translates them with `ErrorText.For` (`Error_<CODE>`); react to specific ones with `ErrorCode`.
  Changing the language rebuilds the `AppShell` (pages read their texts when built), see `UserConfigService`.
- Styles: use the existing keys in `Styles.xaml` (`TitleLabel`, `SubtitleLabel`, `BodyLabel`, `ErrorLabel`,
  `PrimaryButton`, `SecondaryButton`, `TertiaryButton`, `OutlineButton`, `ModalCard`...). Palette in `Colors.xaml`
  (Primary `#F22539`, Secondary `#C00E20`). Fonts: Lato; Press Start 2P only for GAME OVER.
- Controls have no `Base` prefix and must not clash with native names (`SearchBox`, not `SearchBar`). Each control
  names in a comment the Vue component it ports.

## Design fidelity

Screens port the Vue web design (`referencia/poke-juegos-front`, local only) as is: don't add data or decorations
the web doesn't have — propose improvements instead of slipping them in. Take sizes from the CSS, not screenshots:
`html { font-size: 62.5% }` → 1rem = 10dp; the content column is `w-[31.5rem]` = 315dp. Known, chosen deviations:
All/Favorites tabs keep their own style, the back arrow is always visible, score in gold with white outline.

## Gotchas already solved (don't rediscover them)

1. **`ContentView` with its own `[ContentProperty]`** must declare its root with an explicit `<ContentView.Content>`.
   Otherwise the attribute applies to its own XAML, the control ends up inside itself and the UI thread loops forever
   (ANR on Android, "not responding" on Windows). This froze the Pokédex.
2. **SVGs are rasterized at build time** by Resizetizer (Svg.Skia), which ignores `mask-type:alpha` and uses the mask
   fill as opacity: paint masks `#FFFFFF`. `oklch()` colors aren't supported: use hex. Verify the generated PNG's alpha
   in `obj/.../resizetizer/`. Size icons in the SVG at the size they're shown, or they're scaled blurry.
3. Large bitmaps and the sprites use `Resize="false"` (already set in the `.csproj` for `Sprites/*.webp` and
   `bg_pokedex_screen.png`). Sprite files are `pokemon_<id>.webp`.
4. **Android stretches a `*` row containing a `ScrollView`** to the full offered height (WinUI doesn't): use `Auto`.
5. `Picker.Title` on Windows renders as a header above the combo: `OptionPicker` draws its own placeholder. Native
   Entry/Picker chrome is removed in `MauiProgram` (`RemoveNativeEntryChrome`, `RemoveNativePickerChrome`).
6. `WebAuthenticator` doesn't work on WinUI: Windows uses `LoopbackOAuthBrowser` (`http://127.0.0.1:<port>/callback`),
   Android uses `WebAuthenticator` + `pokejuegos://oauth`.
7. Android emulator reaches the API through `adb reverse tcp:5224 tcp:5224` (not `10.0.2.2`: Google OAuth only
   accepts `localhost`). The `ReverseApiPort` target recreates it on every Debug deploy. "Can't reach the server" on
   the emulator → check the reverse and that `BaseUrl` wasn't changed.
8. Packages pinned on purpose: CommunityToolkit.Maui 13.0.0 (14+ needs MAUI ≥ 10.0.30), sqlite-net-pcl without
   `bundle_green` (CVE).
9. Release signing: keystore in `%USERPROFILE%\.pokejuegos\`, store and key passwords in **two separate files**
   (apksigner reads them as consecutive lines from one file → `java.exe` exit code 2). After a failed signing the
   `-Signed.apk` stays unsigned; check with `apksigner verify`.

## Debugging

- Freeze/ANR: `adb` is at `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe` (not in PATH).
  `adb shell top -H -b -n 1 -p <pid>`: the thread with TID = PID at ~100% CPU means a UI-thread loop. Reproduce on
  Windows running `bin/Debug/net10.0-windows10.0.19041.0/win-x64/PokeJuegos.Mobile.exe`, drive it with UI Automation
  (for `Entry` use `SetFocus` + `SendKeys`; `ValuePattern.SetValue` doesn't trigger MAUI bindings) and get the managed
  stack with `dnx -y dotnet-stack report -p <pid>`.
- To inspect what's deployed on the emulator: `adb exec-out run-as com.pokejuegos.mobile cat files/.__override__/x86_64/PokeJuegos.Mobile.dll`.

## Workflow

- Verify with `dotnet build PokeJuegos.Mobile -f net10.0-windows10.0.19041.0` (and `-f net10.0-android` when touching
  Android code): 0 errors, 0 warnings. If the Android build fails with "file is being used by another process",
  Visual Studio is running the app: build Windows only and say so.
- The user runs the app and checks visuals themselves (Visual Studio, emulator or Windows) and sends screenshots.
  Don't run automated screenshots, `dotnet publish` Release or long build experiments without proposing it first.
  Reproducing a reported bug (adb, UI Automation, dotnet-stack) is fine.
- Stop any app/API process you started when done.
- Do not commit; the user handles git.
