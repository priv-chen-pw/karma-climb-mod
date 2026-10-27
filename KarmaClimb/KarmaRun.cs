using System.Collections.Generic;
using RoR2;

namespace KarmaClimb
{
    /// <summary>
    /// What every Karma Climb effect asks before it applies: the run's Karma level, and whether a body
    /// is one of the final bosses.
    /// </summary>
    internal static class KarmaRun
    {
        /// <summary>
        /// Final boss bodies, every phase included. Solus Heart and Providence are secret final bosses and
        /// stay vanilla; <c>FalseSonBody</c> is the playable survivor, not the boss.
        /// </summary>
        private static readonly string[] FinalBossBodyNames =
        {
            "BrotherBody",
            "BrotherHurtBody",
            "MiniVoidRaidCrabBodyPhase1",
            "MiniVoidRaidCrabBodyPhase2",
            "MiniVoidRaidCrabBodyPhase3",
            "FalseSonBossBody",
            "FalseSonBossBodyLunarShard",
            "FalseSonBossBodyBrokenLunarShard",
            "SolusWingBody",
        };

        private static readonly HashSet<BodyIndex> finalBosses = new HashSet<BodyIndex>();

        /// <summary>
        /// The run's Karma, but only during Eclipse: vanilla's own Karma mode elsewhere keeps vanilla rules.
        /// Karma is synced to clients, so this reads the same on every machine.
        /// </summary>
        internal static int Level =>
            Run.instance is EclipseRun && KarmaManager.instance ? KarmaManager.instance.GetEffectiveKarma() : 0;

        internal static void Init()
        {
            RoR2Application.onLoad += () =>
            {
                foreach (string name in FinalBossBodyNames)
                {
                    BodyIndex index = BodyCatalog.FindBodyIndex(name);
                    if (index == BodyIndex.None)
                    {
                        Log.Warning($"Final boss body {name} not found; Karma will not treat it as a final boss.");
                        continue;
                    }

                    finalBosses.Add(index);
                }
            };
        }

        internal static bool IsFinalBoss(CharacterBody body) => finalBosses.Contains(body.bodyIndex);
    }
}
