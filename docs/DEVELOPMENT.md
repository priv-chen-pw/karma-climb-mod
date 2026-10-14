# Karma Climb — development notes

How the mod is built and how it works. The player-facing description is in the
[README](../README.md).

Karma Climb extends vanilla Eclipse from 8 levels to 16. Levels 9-16 carry the full Eclipse 8
modifier set plus a Karma level of 1-8 that is locked for the whole run (Karma triggers grant nothing
during Eclipse). Levels 1-8 force Karma **disabled**, so Karma is climbed once across the ladder instead
of twice (vanilla forces Karma = Eclipse level on 1-8).

## Environment this is built against

| Thing | Version |
| --- | --- |
| Game build | 25475991 (Unity 2021.3.45f2, DLC1-4 installed) |
| `RiskOfRain2.GameLibs` | `1.5.0-r.1` (verified type-for-type against the local install) |
| `MMHOOK.RoR2` | `2026.10.8` |
| BepInEx | 5.4.21 |
| R2API | Core 5.3.0 |
| Dev profile | r2modman profile `KarmaModDev` |

## Building

```
dotnet build KarmaClimb/KarmaClimb.csproj
```

The build drops `KarmaClimb.dll` straight into
`%APPDATA%\r2modmanPlus-local\RiskOfRain2\profiles\KarmaModDev\BepInEx\plugins\KarmaClimb`,
so launching that profile from r2modman picks up the latest build. The copy step is
skipped if the profile folder is missing, so the project still builds elsewhere.

### Releasing

```
dotnet build KarmaClimb.sln -c Release
```

A Release build also writes the upload-ready Thunderstore package to `artifacts/KarmaClimb-<version>.zip`:
the dll, `manifest.json`, `icon.png`, `README.md` and `CHANGELOG.md`, flat at the zip root. It fails if
`Version` in the csproj, `version_number` in the manifest and `PluginVersion` in the code disagree, since
Thunderstore never accepts the same version twice. Bump all three together.

Importing that zip into r2modman (Settings → Import local mod) installs the mod but **not** its
dependencies — r2modman only resolves dependencies for mods installed from Thunderstore — so install
R2API_Difficulty and R2API_Language from the Online tab first.

`global.json` pins the .NET 8 SDK to match the MSBuild shipped with Visual Studio
2022 17.10 and to keep `dotnet new sln` producing `.sln` rather than `.slnx`.

## How it works

The "Eclipse 8 as a baseline" part is free. Every Eclipse modifier in the game is gated with
`Run.instance.selectedDifficulty >= DifficultyIndex.EclipseN`, so a difficulty registered after
`Eclipse8` inherits all eight without touching a single modifier site. `DifficultyAPI.AddDifficulty`
with `preferPositive: true` appends exactly there.

| File | Does |
| --- | --- |
| [KarmaClimbLadder.cs](../KarmaClimb/KarmaClimbLadder.cs) | Ladder shape and token naming, in one place |
| [KarmaClimbDifficulties.cs](../KarmaClimb/KarmaClimbDifficulties.cs) | Registers the 8 difficulties, owns level ⇄ `DifficultyIndex` |
| [EclipseKarmaLock.cs](../KarmaClimb/EclipseKarmaLock.cs) | Locks Karma at its starting level for the whole Eclipse run |
| [KarmaClimbUnlockables.cs](../KarmaClimb/KarmaClimbUnlockables.cs) | Injects `Eclipse.<survivor>.9`-`.16`, cloned from each survivor's level-8 def |
| [EclipseLadderHooks.cs](../KarmaClimb/EclipseLadderHooks.cs) | Bridges vanilla's name-based rule lookups to R2API's indices |
| [KarmaClimbLanguage.cs](../KarmaClimb/KarmaClimbLanguage.cs) | Tokens matching vanilla `Eclipse.json` wording |
| [EclipseMenuMedals.cs](../KarmaClimb/EclipseMenuMedals.cs) | Second medal row for 9-16, using the generated art |
| [EclipseMenuKarmaPanel.cs](../KarmaClimb/EclipseMenuKarmaPanel.cs) | Karma box in the left column: heading, badges with tooltip popups, hint |
| [KarmaPanelPlacer.cs](../KarmaClimb/KarmaPanelPlacer.cs) | Keeps that box under the gamemode paragraph's rendered text |
| [KarmaDescriptions.cs](../KarmaClimb/KarmaDescriptions.cs) | Karma level tooltip text |
| [KarmaClimbMedalArt.cs](../KarmaClimb/KarmaClimbMedalArt.cs) | Loads the embedded 9-16 medal art (Active / Inactive) |
| [EclipseMenuLayout.cs](../KarmaClimb/EclipseMenuLayout.cs) | Widens the right column, frees the description height |
| [EclipseMenuEscape.cs](../KarmaClimb/EclipseMenuEscape.cs) | Escape leaves the Eclipse menu |
| [EclipseMenuDump.cs](../KarmaClimb/EclipseMenuDump.cs) | Optional one-shot dump of the Eclipse menu tree |

Two details worth knowing before changing this code:

- **Difficulty indices are never hardcoded.** R2API assigns `11 + difficultyDefinitions.Count`, which
  happens to equal vanilla's `GetEclipseDifficultyIndex(9..16)` — but only when no other mod registers
  a difficulty first. We store what R2API returns and translate both ways.
- **`EclipseRun.OverrideRuleChoices` forces choices by name**, and the names it builds for levels 9-16
  (`"Difficulty.Count"`, `"Difficulty.12"`, `"Karma.InitialLevel.9"`) do not exist. Instead of
  reimplementing the method — which would drift from vanilla on the next patch — we scope a
  `RuleCatalog.FindChoiceDef` interception to that single call and translate the names.

## Eclipse menu notes

- **Layout.** The relevant tree is `EclipseRunMenu/Main Panel/RightPanel/MedalPanel`, a
  `VerticalLayoutGroup` whose `HorizontalLayout` child holds the eight vanilla medals. We append a second
  `KarmaClimbMedalRow` for 9-16 (sixteen in one row is too cramped). Enable the `LogEclipseMenuHierarchy`
  config option to re-dump the tree if the menu changes in a game patch.
- **Medal refresh timing.** `EclipseDifficultyMedalDisplay.OnEnable` calls `Refresh()` during
  `Instantiate`, i.e. before a clone's `eclipseLevel` is assigned, and vanilla then only refreshes on a
  survivor preference change. We refresh every medal from `EclipseRunScreenController.UpdateDisplayedSurvivor`,
  which fires both on menu entry and on each survivor change.
- **Two columns.** The right column holds survivors, both medal rows and the difficulty description,
  pinned at its top edge so it only grows downward. The Karma box sits in the left column under the
  gamemode paragraph. Cloned texts must have their `LayoutElement` heights cleared: the vanilla
  difficulty description reserves 400px, and a one-line clone inheriting that once pushed the whole
  right column off-screen. The right column's width is the `RightPanelExtraWidth` config option.
- **Medal art for 9-16.** Vanilla medal numerals are painted into the sprites and stop at 8, so
  [KarmaClimb/Assets/Medals](../KarmaClimb/Assets/Medals) holds generated art: the Eclipse 8 medal with
  its numeral erased, the new number drawn in Bombardier (RoR2's UI font) in the vanilla three-layer
  style, and, for the earned state, the ring moved into the Karma icon blue with its glow. Two states per
  level, as Karma icons have: `Active` and `Inactive`. The medals reuse vanilla's state switching by
  filling the display's sprite fields, and `Active` doubles as the difficulty icon in character select
  and the HUD. The art is derived from the game's own textures and font, generated by a script kept
  outside the repo; only the finished PNGs are committed.

## Known gaps

- **Difficulty carousel ordering.** R2API sorts difficulty rule choices by `scalingValue`, and every
  Eclipse level (plus Monsoon) uses `3f`, so ties sort unpredictably. Cosmetic only: the new levels are
  hidden from the normal lobby, and Eclipse runs force the difficulty rather than picking it.
- **Karma unlock requirements.** `Karma.InitialLevel.N` choices carry a `requiredUnlockable`
  (`Karma.StartingKarma.N`). Forcing bypasses the mask, and vanilla Eclipse already forces these same
  choices, but it is worth confirming on a profile that has not unlocked Karma yet.
