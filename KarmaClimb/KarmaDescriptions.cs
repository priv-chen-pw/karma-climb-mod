using R2API;

namespace KarmaClimb
{
    /// <summary>
    /// Tooltip text for each Karma level, shown on the Karma badges in the Eclipse menu: Karma Climb's
    /// effects first, then Mithrix's own Karma.
    ///
    /// Mithrix's mechanics are sourced from the Risk of Rain 2 wiki (https://riskofrain2.wiki.gg/wiki/Karma)
    /// and summarised here in our own wording rather than copied, since the wiki's prose is licensed. His stat
    /// numbers were verified against <c>CharacterBody.RecalculateStats</c>; where Karma Climb replaces one
    /// (see <see cref="KarmaStatEffects"/>), his text says what he actually gets.
    /// </summary>
    internal static class KarmaDescriptions
    {
        internal const string HeadingToken = "KARMACLIMB_MENU_KARMA_HEADING";
        internal const string HintToken = "KARMACLIMB_MENU_KARMA_HINT";

        internal static string TitleToken(int karmaLevel) => $"KARMACLIMB_KARMA_{karmaLevel}_TITLE";

        internal static string BodyToken(int karmaLevel) => $"KARMACLIMB_KARMA_{karmaLevel}_BODY";

        /// <summary>Karma Climb's effects for the whole run, indexed by Karma level, so index 0 is unused.</summary>
        private static readonly string[] General =
        {
            null,

            // 1
            "All enemies and bosses deal <style=cIsDamage>+10% damage</style>.\n\n" +
            "Kill gold <style=cIsUtility>+4%</style>.",

            // 2
            "All enemies and bosses gain <style=cIsDamage>+15% attack speed</style>.\n\n" +
            "Kill gold <style=cIsUtility>+4%</style> (<style=cIsUtility>+8%</style> total).",

            // 3
            "The teleporter starts with a <style=cIsUtility>Shrine of the Mountain</style> stack: one extra boss, " +
            "and one extra item when the bosses are beaten.\n\n" +
            "Kill gold <style=cIsUtility>+4%</style> (<style=cIsUtility>+12%</style> total).",

            // 4
            "Voidling, False Son and Solus Wing gain a <style=cIsUtility>shell</style> each time they lose " +
            "<style=cIsHealth>33%</style> of their maximum health, taking <style=cIsUtility>80% less damage for 5 seconds</style>.\n\n" +
            "Kill gold <style=cIsUtility>+4%</style> (<style=cIsUtility>+16%</style> total).",

            // 5
            "Monster credits <style=cIsDamage>+20%</style> on every stage.\n\n" +
            "Kill gold <style=cIsUtility>+4%</style> (<style=cIsUtility>+20%</style> total), which cancels " +
            "Eclipse's <style=cIsHealth>-20%</style> Enemy Gold Drops.",

            // 6
            "<style=cIsDamage>Voidtouched</style> and <style=cIsDamage>Perfected</style> elites can spawn on every stage, " +
            "at the cost of Malachite-tier elites.",

            // 7
            "Final bosses (Mithrix, Voidling, False Son, Solus Wing) gain <style=cIsHealing>+30% maximum health</style>.\n\n" +
            "Interactable credits <style=cIsUtility>+20%</style>: more chests, shrines and drones.",

            // 8
            "Final bosses spawn with a random <style=cIsDamage>elite aspect</style>: Blazing, Overloading, Glacial or Mending, " +
            "the same one for every phase of the fight.",
        };

        /// <summary>Mithrix's own Karma, indexed by Karma level, so index 0 is unused.</summary>
        private static readonly string[] Mithrix =
        {
            null,

            // 1
            "Gains a crystal cluster on his hammer, and one further crystal piece at each Karma level up to 7.\n\n" +
            "In phases 1, 3 and 4, once he has taken damage equal to <style=cIsDamage>10%</style> of his maximum health " +
            "he gains a <style=cIsUtility>5 second shell</style> that reduces every hit he takes to 1 damage.\n\n" +
            "His damage bonus is the <style=cIsDamage>+10%</style> above, replacing vanilla's +1% per Karma level.",

            // 2
            "<style=cIsUtility>Movement speed +30%</style>. His attack speed bonus is the <style=cIsDamage>+15%</style> above, " +
            "replacing vanilla's +30%.\n\n" +
            "Sky Leap drops four bombs at the cardinal points that detonate once he lands, each releasing a ring of shockwaves " +
            "for <style=cIsDamage>400% damage</style> that <style=cIsUtility>Cripple</style>. A gravity well also forms above " +
            "his landing point and pulls players in while he is airborne.\n\n" +
            "His dash leaves three crystal spikes dealing <style=cIsDamage>200% damage</style> and applying <style=cIsDamage>Lunar Ruin</style>.\n\n" +
            "In phase 2, the throne grants items to monsters on his team every <style=cIsUtility>25 seconds</style>.",

            // 3
            "Hammer Smash adds crawling waves and a ring of crystal spikes.\n\nHammer Swing releases a fan of five crystal spike trails.\n\n" +
            "Needle Barrage and Needle Burst travel faster and leave <style=cIsDamage>Lunar Ruin</style> spikes where they strike terrain.\n\n" +
            "In phase 2, monsters that die erupt into lines of flame.",

            // 4
            "His subtitle becomes <style=cIsUtility>Apocalypse Aspirant</style>.\n\nHis Sky Leap landing calls down a meteor storm " +
            "that damages every character in the arena.\n\nHis dash grants <style=cIsUtility>knockback immunity for 5 seconds</style>.\n\n" +
            "In phase 4, Lunar Orbs also send out spiralling crystal spike trails.",

            // 5
            "<style=cIsDamage>Karmic Spires</style>: at the start of phases 1 and 3 he summons up to six spires roughly 75m away.\n\n" +
            "In phase 2, the throne periodically fires a blast at a player.",

            // 6
            "After Big Spinny and Forbidden Pizza he resummons spires, up to six.\n\n" +
            "In phase 3, Shock Wave adds pairs of crawling waves that apply <style=cIsDamage>Lunar Ruin</style>, and his dash launches a homing bomb.\n\n" +
            "Monsters from the director in phase 3 gain the <style=cIsDamage>Perfected</style> affix, and the " +
            "<style=cIsDamage>Twisted</style> affix if Seekers of the Storm is enabled.\n\n" +
            "In phase 4, at 50% health or less, he gains a mortar volley, Lunar Chimera Golems and Lunar Chimera Wisps.\n\n" +
            "<style=cSub>Vanilla also halves his cooldowns here, but Eclipse already halves every enemy's cooldowns, " +
            "and the two do not stack.</style>",

            // 7
            "No new mechanics at this level.",

            // 8
            "<style=cIsHealing>Maximum health +50%</style>, replacing the +30% from Karma 7.\n\n" +
            "The sky above the arena changes, and he uses different spawn lines, including separate lines when a player is the Heretic.",
        };

        internal static void Register()
        {
            LanguageAPI.Add(HeadingToken, "Karma");
            LanguageAPI.Add(HintToken, "<style=cSub>Hover over a Karma level to see what it adds.</style>");

            for (int karma = 1; karma <= KarmaClimbLadder.AddedLevelCount; karma++)
            {
                LanguageAPI.Add(TitleToken(karma), $"Karma {karma}");
                LanguageAPI.Add(BodyToken(karma), General[karma] + "\n\n<style=cSub>— Mithrix —</style>\n\n" + Mithrix[karma]);
            }
        }
    }
}
