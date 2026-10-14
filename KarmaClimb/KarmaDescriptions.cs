using R2API;

namespace KarmaClimb
{
    /// <summary>
    /// Tooltip text for each Karma level, shown on the Karma badges in the Eclipse menu.
    ///
    /// The mechanics are sourced from the Risk of Rain 2 wiki (https://riskofrain2.wiki.gg/wiki/Karma)
    /// and summarised here in our own wording rather than copied, since the wiki's prose is licensed.
    /// The stat numbers were verified against <c>CharacterBody.RecalculateStats</c>, where they apply
    /// only to Mithrix (<c>BrotherKarmaUtils.IsBrother</c>).
    /// </summary>
    internal static class KarmaDescriptions
    {
        internal const string HeadingToken = "KARMACLIMB_MENU_KARMA_HEADING";
        internal const string HintToken = "KARMACLIMB_MENU_KARMA_HINT";

        internal static string TitleToken(int karmaLevel) => $"KARMACLIMB_KARMA_{karmaLevel}_TITLE";

        internal static string BodyToken(int karmaLevel) => $"KARMACLIMB_KARMA_{karmaLevel}_BODY";

        /// <summary>Indexed by Karma level, so index 0 is unused.</summary>
        private static readonly string[] Bodies =
        {
            null,

            // 1
            "Mithrix gains a crystal cluster on his hammer, and one further crystal piece at each " +
            "Karma level up to 7.\n\nIn phases 1, 3 and 4, once he has taken damage equal to <style=cIsDamage>10%</style> " +
            "of his maximum health he gains a <style=cIsUtility>5 second shell</style> that reduces every hit he takes to 1 damage.",

            // 2
            "<style=cIsUtility>Movement and attack speed +30%</style>.\n\nSky Leap drops four bombs at the cardinal points " +
            "that detonate once he lands, each releasing a ring of shockwaves for <style=cIsDamage>400% damage</style> " +
            "that <style=cIsUtility>Cripple</style>. A gravity well also forms above his landing point and pulls players in while he is airborne.\n\n" +
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
            "<style=cIsUtility>Skill cooldowns halved</style>.\n\nAfter Big Spinny and Forbidden Pizza he resummons spires, up to six.\n\n" +
            "In phase 3, Shock Wave adds pairs of crawling waves that apply <style=cIsDamage>Lunar Ruin</style>, and his dash launches a homing bomb.\n\n" +
            "Monsters from the director in phase 3 gain the <style=cIsDamage>Perfected</style> affix, and the " +
            "<style=cIsDamage>Twisted</style> affix if Seekers of the Storm is enabled.\n\n" +
            "In phase 4, at 50% health or less, he gains a mortar volley, Lunar Chimera Golems and Lunar Chimera Wisps.",

            // 7
            "No new mechanics at this level \u2014 only the cumulative damage increase.",

            // 8
            "<style=cIsHealing>Maximum health +50%</style>.\n\nThe sky above the arena changes, and he uses different spawn lines, " +
            "including separate lines when a player is the Heretic.",
        };

        internal static void Register()
        {
            LanguageAPI.Add(HeadingToken, "Karma");
            LanguageAPI.Add(HintToken, "<style=cSub>Hover over a Karma level to see what it adds.</style>");

            for (int karma = 1; karma <= KarmaClimbLadder.AddedLevelCount; karma++)
            {
                LanguageAPI.Add(TitleToken(karma), $"Karma {karma}");
                LanguageAPI.Add(BodyToken(karma), BuildBody(karma));
            }
        }

        private static string BuildBody(int karmaLevel)
        {
            // Every level carries the cumulative damage bonus, so state it once up front.
            string cumulative = $"<style=cSub>Cumulative: Mithrix damage <style=cIsDamage>+{karmaLevel}%</style>.</style>\n\n";
            return cumulative + Bodies[karmaLevel];
        }
    }
}
