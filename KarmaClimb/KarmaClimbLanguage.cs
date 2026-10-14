using System.Text;
using R2API;

namespace KarmaClimb
{
    /// <summary>
    /// Language tokens for the appended Eclipse levels, written to match the vanilla
    /// Eclipse.json wording and markup so the selection screen reads consistently.
    /// </summary>
    internal static class KarmaClimbLanguage
    {
        /// <summary>The eight inherited Eclipse modifiers, worded exactly as ECLIPSE_8_DESCRIPTION.</summary>
        private static readonly string[] InheritedEffects =
        {
            "Ally Starting Health: <style=cIsHealth>-50%</style>",
            "Teleporter Radius: <style=cIsHealth>-50%</style>",
            "Ally Fall Damage: <style=cIsHealth>+100% and lethal</style>",
            "Enemy Speed: <style=cIsHealth>+40%</style>",
            "Ally Healing: <style=cIsHealth>-50%</style>",
            "Enemy Gold Drops: <style=cIsHealth>-20%</style>",
            "Enemy Cooldowns: <style=cIsHealth>-50%</style>",
            "Allies recieve <style=cIsHealth>permanent damage</style>",
        };

        internal static void Register()
        {
            // The Eclipse menu's left paragraph. Vanilla says "a maximum of 8 times".
            LanguageAPI.Add(
                "ECLIPSE_GAMEMODE_DESCRIPTION",
                "<style=cIsUtility>Eclipse</style> is a gamemode that adds stacking challenge modifiers to your run.\n\n" +
                "Each time you <style=cIsUtility>beat the game</style>, your Eclipse level <style=cIsUtility>permanently increases</style> " +
                $"with that Survivor, up to a maximum of <style=cIsUtility>{KarmaClimbLadder.MaxLevel}</style> times.\n\n" +
                $"Levels <style=cIsUtility>{KarmaClimbLadder.VanillaMaxLevel + 1}-{KarmaClimbLadder.MaxLevel}</style> keep every " +
                $"Eclipse {KarmaClimbLadder.VanillaMaxLevel} modifier and add <style=cIsUtility>Karma</style>, " +
                "which stays <style=cIsUtility>locked at its starting level</style> for the whole run.");

            for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
            {
                int karma = KarmaClimbLadder.KarmaLevelFor(level);

                LanguageAPI.Add(KarmaClimbLadder.NameToken(level), $"Eclipse ({level})");
                LanguageAPI.Add(KarmaClimbLadder.DescriptionToken(level), BuildDescription(level, karma));
                LanguageAPI.Add(KarmaClimbLadder.UnlockableNameToken(level), $"Eclipse ({level})");
            }
        }

        private static string BuildDescription(int level, int karma)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Starts at baseline Monsoon difficulty. <style=cSub>\n");

            for (int i = 0; i < InheritedEffects.Length; i++)
            {
                sb.Append("\n<mspace=0.5em>(").Append(i + 1).Append(")</mspace> ").Append(InheritedEffects[i]).Append(' ');
            }

            // The one thing that actually changes across 9-16. Karma cannot rise during an Eclipse run
            // (see EclipseKarmaLock), so this is the Karma for the whole run, not just its start.
            sb.Append("\n<mspace=0.5em>(").Append(level).Append(")</mspace> Karma: <style=cIsHealth>locked at ")
              .Append(karma).Append("</style></style>");

            return sb.ToString();
        }
    }
}
