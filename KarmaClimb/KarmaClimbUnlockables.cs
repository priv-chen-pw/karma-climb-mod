using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace KarmaClimb
{
    /// <summary>
    /// Creates the <c>Eclipse.&lt;survivor&gt;.9</c> through <c>.16</c> unlockables that the extended
    /// ladder is gated on, so levels 9-16 unlock by exactly the same mechanism as 1-8.
    ///
    /// <c>EclipseRun.GetEclipseLevelUnlockablesForSurvivor</c> walks upward from level 2 and stops at
    /// the first missing <c>Eclipse.&lt;survivor&gt;.&lt;n&gt;</c>, and <c>OnClientGameOver</c> grants the
    /// next one on a win. Both work unchanged once these exist.
    ///
    /// Each new def is cloned from that survivor's vanilla level-8 unlockable rather than built from
    /// scratch, so icon, hidden flag, sort score and required expansion carry over without guesswork.
    /// </summary>
    internal static class KarmaClimbUnlockables
    {
        internal static void Init()
        {
            On.RoR2.UnlockableCatalog.SetUnlockableDefs += (orig, unlockableDefs) =>
                orig(Append(unlockableDefs));
        }

        private static UnlockableDef[] Append(UnlockableDef[] vanillaDefs)
        {
            // Survivors that have a complete vanilla ladder are exactly those with a level-8 entry.
            string suffix = "." + KarmaClimbLadder.VanillaMaxLevel;
            Dictionary<string, UnlockableDef> templates = new Dictionary<string, UnlockableDef>();

            foreach (UnlockableDef def in vanillaDefs)
            {
                if (def == null)
                {
                    continue;
                }

                string name = def.cachedName;
                if (string.IsNullOrEmpty(name) || !name.StartsWith("Eclipse.") || !name.EndsWith(suffix))
                {
                    continue;
                }

                // "Eclipse.<survivor>.8" -> "<survivor>"
                string survivor = name.Substring("Eclipse.".Length, name.Length - "Eclipse.".Length - suffix.Length);
                templates[survivor] = def;
            }

            if (templates.Count == 0)
            {
                Log.Error("Found no vanilla Eclipse level-8 unlockables; levels 9-16 will be unreachable.");
                return vanillaDefs;
            }

            List<UnlockableDef> added = new List<UnlockableDef>(templates.Count * KarmaClimbLadder.AddedLevelCount);

            foreach (KeyValuePair<string, UnlockableDef> pair in templates)
            {
                for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
                {
                    UnlockableDef clone = Object.Instantiate(pair.Value);
                    clone.cachedName = KarmaClimbLadder.UnlockableName(pair.Key, level);
                    // UnlockableDef.name is obsolete-as-error; the engine name is only for debugging.
                    ((ScriptableObject)clone).name = clone.cachedName;
                    clone.nameToken = KarmaClimbLadder.UnlockableNameToken(level);
                    added.Add(clone);
                }
            }

            Log.Info($"Added {added.Count} Eclipse unlockables across {templates.Count} survivors (levels {KarmaClimbLadder.VanillaMaxLevel + 1}-{KarmaClimbLadder.MaxLevel}).");
            return vanillaDefs.Concat(added).ToArray();
        }
    }
}
