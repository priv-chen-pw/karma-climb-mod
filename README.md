# Karma Climb

A Risk of Rain 2 mod adding **Karma Climb**, a separate entry in the alternate game
modes list, structured like Eclipse Climb but driven by the Hallowed Concepts (DLC4)
Karma system.

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

`global.json` pins the .NET 8 SDK to match the MSBuild shipped with Visual Studio
2022 17.10 and to keep `dotnet new sln` producing `.sln` rather than `.slnx`.

## How it works

Eclipse runs 1-16. Levels 9-16 carry the full Eclipse 8 modifier set plus a forced starting
Karma of 1-8; levels 1-8 force Karma **disabled**, so Karma is climbed once across the ladder
instead of twice (vanilla forces Karma = Eclipse level on 1-8).

The "Eclipse 8 as a baseline" part is free. Every Eclipse modifier in the game is gated with
`Run.instance.selectedDifficulty >= DifficultyIndex.EclipseN`, so a difficulty registered after
`Eclipse8` inherits all eight without touching a single modifier site. `DifficultyAPI.AddDifficulty`
with `preferPositive: true` appends exactly there.

| File | Does |
| --- | --- |
| [KarmaClimbLadder.cs](KarmaClimb/KarmaClimbLadder.cs) | Ladder shape and token naming, in one place |
| [KarmaClimbDifficulties.cs](KarmaClimb/KarmaClimbDifficulties.cs) | Registers the 8 difficulties, owns level ⇄ `DifficultyIndex` |
| [KarmaClimbUnlockables.cs](KarmaClimb/KarmaClimbUnlockables.cs) | Injects `Eclipse.<survivor>.9`-`.16`, cloned from each survivor's level-8 def |
| [EclipseLadderHooks.cs](KarmaClimb/EclipseLadderHooks.cs) | Bridges vanilla's name-based rule lookups to R2API's indices |
| [KarmaClimbLanguage.cs](KarmaClimb/KarmaClimbLanguage.cs) | Tokens matching vanilla `Eclipse.json` wording |
| [EclipseMenuMedals.cs](KarmaClimb/EclipseMenuMedals.cs) | Medals for 9-16, cloned from Eclipse 1-8 art and tinted cyan |
| [EclipseMenuKarmaSection.cs](KarmaClimb/EclipseMenuKarmaSection.cs) | "Karma" heading, hint line and hoverable per-level badges |
| [KarmaDescriptions.cs](KarmaClimb/KarmaDescriptions.cs) | Karma level tooltip text |
| [KarmaDescriptionPanel.cs](KarmaClimb/KarmaDescriptionPanel.cs) | Left-hand description panel, cloned from the menu's own |
| [KarmaBadgeHover.cs](KarmaClimb/KarmaBadgeHover.cs) | Drives that panel while a badge is hovered |
| [EclipseMedalNumbers.cs](KarmaClimb/EclipseMedalNumbers.cs) | Draws 9-16 on the new medals, whose numerals are not in any sprite |
| [EclipseMenuLayout.cs](KarmaClimb/EclipseMenuLayout.cs) | Widens the right column, frees the description height |
| [EclipseMenuEscape.cs](KarmaClimb/EclipseMenuEscape.cs) | Escape leaves the Eclipse menu |
| [EclipseMenuDump.cs](KarmaClimb/EclipseMenuDump.cs) | Optional one-shot dump of the Eclipse menu tree |

Two details worth knowing before changing this code:

- **Difficulty indices are never hardcoded.** R2API assigns `11 + difficultyDefinitions.Count`, which
  happens to equal vanilla's `GetEclipseDifficultyIndex(9..16)` — but only when no other mod registers
  a difficulty first. We store what R2API returns and translate both ways.
- **`EclipseRun.OverrideRuleChoices` forces choices by name**, and the names it builds for levels 9-16
  (`"Difficulty.Count"`, `"Difficulty.12"`, `"Karma.InitialLevel.9"`) do not exist. Instead of
  reimplementing the method — which would drift from vanilla on the next patch — we scope a
  `RuleCatalog.FindChoiceDef` interception to that single call and translate the names.

## Known gaps

- **Eclipse menu layout.** The relevant tree is
  `EclipseRunMenu/Main Panel/RightPanel/MedalPanel`, a `VerticalLayoutGroup` whose `HorizontalLayout`
  child holds the eight vanilla medals. We append a second `KarmaClimbMedalRow` for 9-16 (sixteen in one
  row is too cramped) and then the `KarmaClimbKarmaSection` as the last child. Enable the
  `LogEclipseMenuHierarchy` config option to re-dump the tree if the menu changes in a game patch.
- **Difficulty carousel ordering.** R2API sorts difficulty rule choices by `scalingValue`, and every
  Eclipse level (plus Monsoon) uses `3f`, so ties sort unpredictably. Cosmetic only: Eclipse runs force
  the difficulty rather than picking it from the carousel.
- **Karma unlock requirements.** `Karma.InitialLevel.N` choices carry a `requiredUnlockable`
  (`Karma.StartingKarma.N`). Forcing bypasses the mask, and vanilla Eclipse already forces these same
  choices, but it is worth confirming on a profile that has not unlocked Karma yet.

## Eclipse menu notes

- **Medal refresh timing.** `EclipseDifficultyMedalDisplay.OnEnable` calls `Refresh()` during
  `Instantiate`, i.e. before a clone's `eclipseLevel` is assigned, and vanilla then only refreshes on a
  survivor preference change. We refresh every medal from `EclipseRunScreenController.UpdateDisplayedSurvivor`,
  which fires both on menu entry and on each survivor change.
- **Tuning the description panel.** `KarmaDescriptionPanel.LeftMargin` and `PanelWidth` set its position
  and width. The right column's width is the `RightPanelExtraWidth` config option.
- **Medal numerals are baked into the sprites**, and there is no vanilla art above Eclipse 8. Medals 9-16
  therefore drop the ring art for a plain tinted circle (`texCircle256Aliased`) with the number drawn on
  top, which also keeps them visually distinct from the Eclipse medals.
