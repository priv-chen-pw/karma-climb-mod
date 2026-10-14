namespace KarmaClimb
{
    /// <summary>
    /// Shape of the extended Eclipse ladder.
    ///
    /// Vanilla Eclipse is levels 1-8, and every Eclipse modifier in the game is gated with
    /// <c>Run.instance.selectedDifficulty &gt;= DifficultyIndex.EclipseN</c>. That means any
    /// difficulty registered *after* Eclipse8 inherits all eight modifiers for free, which is
    /// what makes "Eclipse 8 as a baseline" possible without touching a single modifier site.
    ///
    /// Levels 9-16 therefore carry the full Eclipse 8 modifier set plus a starting Karma level
    /// of 1-8. Levels 1-8 are forced to Karma disabled, so Karma is climbed exactly once across
    /// the ladder instead of twice.
    /// </summary>
    internal static class KarmaClimbLadder
    {
        /// <summary>Highest Eclipse level vanilla ships (<c>EclipseRun.maxEclipseLevel</c>).</summary>
        internal const int VanillaMaxLevel = 8;

        /// <summary>Highest Eclipse level after this mod.</summary>
        internal const int MaxLevel = 16;

        /// <summary>How many levels this mod appends. Equals <c>KarmaManager.maxKarma</c>.</summary>
        internal const int AddedLevelCount = MaxLevel - VanillaMaxLevel;

        internal static bool IsKarmaClimbLevel(int eclipseLevel)
            => eclipseLevel > VanillaMaxLevel && eclipseLevel <= MaxLevel;

        /// <summary>Starting Karma level a given Eclipse level should force.</summary>
        /// <returns>1-8 for Karma Climb levels, 0 for base Eclipse (which disables Karma).</returns>
        internal static int KarmaLevelFor(int eclipseLevel)
            => IsKarmaClimbLevel(eclipseLevel) ? eclipseLevel - VanillaMaxLevel : 0;

        internal static string NameToken(int eclipseLevel) => $"KARMACLIMB_ECLIPSE_{eclipseLevel}_NAME";

        internal static string DescriptionToken(int eclipseLevel) => $"KARMACLIMB_ECLIPSE_{eclipseLevel}_DESCRIPTION";

        internal static string UnlockableName(string survivorCachedName, int eclipseLevel)
            => $"Eclipse.{survivorCachedName}.{eclipseLevel}";

        internal static string UnlockableNameToken(int eclipseLevel)
            => $"KARMACLIMB_UNLOCKABLE_ECLIPSE_{eclipseLevel}_NAME";
    }
}
