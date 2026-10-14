# Karma Climb

Eclipse doesn't stop at 8 anymore! Climb all the way to **Eclipse 16**, where every new level stacks Mithrix's **Karma** on top of the full Eclipse 8 challenge.

## Changes from Vanilla

* **Eclipse now goes up to 16**
  * Eclipse 9–16 keep **all eight Eclipse modifiers** and add a **Karma** level:
    * Eclipse 9 = Karma 1, Eclipse 10 = Karma 2, … all the way to Eclipse 16 = Karma 8
  * **Karma is locked for the whole run.** It starts at the level's Karma and never goes up (Karma triggers give nothing during Eclipse).
  * Unlocked just like vanilla Eclipse: beat Eclipse 8 with a survivor to unlock Eclipse 9 for them, beat 9 to unlock 10, and so on.
* **Eclipse 1–8 no longer include Karma**
  * In vanilla, Eclipse 3 also forces Karma 3, and so on. Here you climb Karma once, on Eclipse 9–16, instead of twice.
* **Eclipse menu**
  * A second row of medals for Eclipse 9–16: greyed out until earned, Karma blue once you've beaten them.
  * New **Karma** box: hover a Karma badge to see exactly what that level does to Mithrix.
  * The menu is a bit wider, and the difficulty description grows to fit the longer Eclipse 9–16 text.
  * **Escape** takes you back out of the menu.
* Eclipse 9–16 show their own badge in character select and during the run.
* The new levels stay out of the normal lobby's difficulty picker, just like vanilla Eclipse.

## What does Karma do?

Karma makes **Mithrix** nastier. Every level keeps everything from the levels before it:

* **Every level:** Mithrix deals +1% damage per Karma level
* **Karma 2+:** Mithrix moves and attacks 30% faster
* **Karma 6+:** Mithrix's skill cooldowns are halved
* **Karma 8:** Mithrix has 50% more health
* On top of that, almost every level gives him new attacks and arena hazards. Hover the Karma badges in the Eclipse menu for the full list!

## Installation

Install with **r2modman** or the **Thunderstore Mod Manager**. Everything it needs is installed automatically.

Installing by hand? Drop the `KarmaClimb` folder into `BepInEx\plugins\`. You'll also need R2API (Core, Difficulty and Language) and HookGenPatcher.

## Compatibility

* Works alongside mods that add their own difficulties.
* Mods that also change the Eclipse ladder (more levels, different unlocks) will most likely conflict.
* Multiplayer hasn't been tested yet.

## Configuration

In `BepInEx\config\priv-chen-pw.KarmaClimb.cfg`:

* `RightPanelExtraWidth` (default `260`): how much wider the Eclipse menu's right-hand column is. Set it to `0` for the vanilla width.

## Credits

* Made by priv-chen-pw
* Karma level details from the [Risk of Rain 2 Wiki](https://riskofrain2.wiki.gg/wiki/Karma)

## Bugs & Feedback

Found a bug or have an idea? Open an issue on [GitHub](https://github.com/priv-chen-pw/karma-climb-mod/issues).
