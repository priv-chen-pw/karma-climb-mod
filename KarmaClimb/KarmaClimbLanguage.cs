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

            // The one thing that actually changes across 9-16.
            sb.Append("\n<mspace=0.5em>(").Append(level).Append(")</mspace> Starting Karma: <style=cIsHealth>")
              .Append(karma).Append("</style></style>");

            return sb.ToString();
        }
    }
}
