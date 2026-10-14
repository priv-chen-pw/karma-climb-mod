using System.Collections.Generic;
using R2API;
using RoR2;

namespace KarmaClimb
{
    /// <summary>
    /// Registers the Eclipse 9-16 difficulties and owns the mapping between Eclipse level and
    /// the <see cref="DifficultyIndex"/> R2API handed back.
    ///
    /// The indices are deliberately *not* hardcoded. R2API assigns <c>11 + difficultyDefinitions.Count</c>
    /// for positive difficulties, so with nothing else installed our levels land on 11-18, which is
    /// exactly what vanilla's <c>GetEclipseDifficultyIndex</c> formula produces. But any other mod
    /// registering a difficulty first shifts that, so we store what we are given and translate.
    /// </summary>
    internal static class KarmaClimbDifficulties
    {
        private static readonly Dictionary<int, DifficultyIndex> LevelToIndex = new Dictionary<int, DifficultyIndex>();
        private static readonly Dictionary<DifficultyIndex, int> IndexToLevel = new Dictionary<DifficultyIndex, int>();

        /// <summary>Must run before the difficulty catalog initialises, i.e. during plugin Awake.</summary>
        internal static void Register()
        {
            for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
            {
                DifficultyDef def = new DifficultyDef(
                    // Identical to Monsoon and every Eclipse level. The added difficulty comes from
                    // Karma and the inherited Eclipse modifiers, not from faster scaling.
                    scalingValue: 3f,
                    nameToken: KarmaClimbLadder.NameToken(level),
                    iconPath: "Textures/DifficultyIcons/texDifficultyEclipse8Icon",
                    descriptionToken: KarmaClimbLadder.DescriptionToken(level),
                    color: ColorCatalog.GetColor(ColorCatalog.ColorIndex.HardDifficulty),
                    serverTag: "mn",
                    countsAsHardMode: true);

                // preferPositive is the whole trick: it appends after Eclipse8 so that every
                // "selectedDifficulty >= EclipseN" check in the game passes.
                DifficultyIndex index = DifficultyAPI.AddDifficulty(def, preferPositive: true);

                if (index == DifficultyIndex.Invalid)
                {
                    Log.Error($"R2API refused the difficulty for Eclipse {level}; the ladder will stop at {level - 1}.");
                    return;
                }

                LevelToIndex[level] = index;
                IndexToLevel[index] = level;
                Log.Debug($"Eclipse {level} registered as DifficultyIndex {(int)index} (Karma {KarmaClimbLadder.KarmaLevelFor(level)}).");
            }
        }

        internal static bool TryGetDifficultyIndex(int eclipseLevel, out DifficultyIndex index)
            => LevelToIndex.TryGetValue(eclipseLevel, out index);

        internal static bool TryGetEclipseLevel(DifficultyIndex index, out int eclipseLevel)
            => IndexToLevel.TryGetValue(index, out eclipseLevel);

        internal static IEnumerable<KeyValuePair<int, DifficultyIndex>> RegisteredLevels => LevelToIndex;
    }
}
