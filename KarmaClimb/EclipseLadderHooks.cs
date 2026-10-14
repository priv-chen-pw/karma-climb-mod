using System.Collections.Generic;
using RoR2;

namespace KarmaClimb
{
    /// <summary>
    /// Teaches vanilla's Eclipse plumbing about levels 9-16.
    ///
    /// The awkward part is that <c>EclipseRun.OverrideRuleChoices</c> forces rule choices by *name*:
    /// it builds <c>"Difficulty." + GetEclipseDifficultyIndex(level).ToString()</c>, which for our
    /// appended indices stringifies to things like "Count" or "12" because they fall outside the
    /// <see cref="DifficultyIndex"/> enum, while R2API names its choices after the localized
    /// difficulty name. It also forces <c>"Karma.InitialLevel." + level</c>, and no such choice
    /// exists above 8.
    ///
    /// Rather than reimplement the method and risk drifting from vanilla on the next game patch, we
    /// scope a <c>RuleCatalog.FindChoiceDef</c> interception to the duration of that one call and
    /// translate the names it asks for. Everything else in vanilla's body runs untouched.
    /// </summary>
    internal static class EclipseLadderHooks
    {
        private const string DifficultyPrefix = "Difficulty.";
        private const string KarmaPrefix = "Karma.InitialLevel.";
        private const string KarmaDisabledChoice = KarmaPrefix + "KarmaDisabled";

        private static bool inEclipseOverride;

        internal static void Init()
        {
            // Extend the ladder itself.
            EclipseRun.maxEclipseLevel = KarmaClimbLadder.MaxLevel;

            // Level <-> DifficultyIndex, in both directions.
            On.RoR2.EclipseRun.GetEclipseDifficultyIndex += (orig, eclipseLevel) =>
                KarmaClimbDifficulties.TryGetDifficultyIndex(eclipseLevel, out DifficultyIndex index)
                    ? index
                    : orig(eclipseLevel);

            On.RoR2.EclipseRun.GetEclipseLevelFromRuleBook += (orig, ruleBook) =>
                KarmaClimbDifficulties.TryGetEclipseLevel(ruleBook.FindDifficulty(), out int level)
                    ? level
                    : orig(ruleBook);

            On.RoR2.EclipseRun.OverrideRuleChoices += (orig, self, mustInclude, mustExclude, runSeed) =>
            {
                inEclipseOverride = true;
                try
                {
                    orig(self, mustInclude, mustExclude, runSeed);
                }
                finally
                {
                    inEclipseOverride = false;
                }
            };

            On.RoR2.RuleCatalog.FindChoiceDef += (orig, globalName) =>
            {
                if (inEclipseOverride && TryRemap(orig, globalName, out RuleChoiceDef remapped))
                {
                    return remapped;
                }

                return orig(globalName);
            };
        }

        private static bool TryRemap(
            On.RoR2.RuleCatalog.orig_FindChoiceDef orig,
            string globalName,
            out RuleChoiceDef remapped)
        {
            remapped = null;

            if (string.IsNullOrEmpty(globalName))
            {
                return false;
            }

            if (globalName.StartsWith(DifficultyPrefix))
            {
                string wanted = globalName.Substring(DifficultyPrefix.Length);

                foreach (KeyValuePair<int, DifficultyIndex> pair in KarmaClimbDifficulties.RegisteredLevels)
                {
                    if (pair.Value.ToString() != wanted)
                    {
                        continue;
                    }

                    remapped = FindDifficultyChoice(pair.Value);
                    if (remapped == null)
                    {
                        Log.Error($"No rule choice for DifficultyIndex {(int)pair.Value} (Eclipse {pair.Key}).");
                    }

                    return remapped != null;
                }

                return false;
            }

            if (globalName.StartsWith(KarmaPrefix))
            {
                // Vanilla passes the Eclipse level here, not a Karma level.
                if (!int.TryParse(globalName.Substring(KarmaPrefix.Length), out int eclipseLevel))
                {
                    return false;
                }

                int karma = KarmaClimbLadder.KarmaLevelFor(eclipseLevel);

                // Base Eclipse is karma-free; Karma is climbed once, over levels 9-16.
                remapped = orig(karma > 0 ? KarmaPrefix + karma : KarmaDisabledChoice);
                return remapped != null;
            }

            return false;
        }

        private static RuleChoiceDef FindDifficultyChoice(DifficultyIndex index)
        {
            RuleDef difficultyRule = RuleCatalog.FindRuleDef("Difficulty");
            if (difficultyRule == null)
            {
                return null;
            }

            foreach (RuleChoiceDef choice in difficultyRule.choices)
            {
                if (choice.difficultyIndex == index)
                {
                    return choice;
                }
            }

            return null;
        }
    }
}
