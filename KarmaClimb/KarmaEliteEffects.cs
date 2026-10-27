using System;
using System.Linq;
using R2API;
using RoR2;
using RoR2.Artifacts;

namespace KarmaClimb
{
    /// <summary>
    /// Karma's elite changes: Voidtouched and Perfected elites on every stage at Karma 6, and an elite
    /// aspect on every final boss at Karma 8.
    /// </summary>
    internal static class KarmaEliteEffects
    {
        /// <summary>
        /// Voidtouched and Perfected at Malachite's cost. Vanilla only spawns Perfected from Lunar spawn
        /// cards, and Voidtouched only from Void Infestors.
        /// </summary>
        private static readonly CombatDirector.EliteTierDef voidAndPerfectedTier = new CombatDirector.EliteTierDef
        {
            costMultiplier = CombatDirector.baseEliteCostMultiplier * 6f,
            eliteTypes = Array.Empty<EliteDef>(),
            isAvailable = rules => rules == SpawnCard.EliteRules.Default && KarmaRun.Level >= 6,
            canSelectWithoutAvailableEliteDef = false,
        };

        private static EliteDef fightAspect;
        private static Stage fightAspectStage;

        internal static void Init()
        {
            EliteAPI.AppendCustomEliteTier(voidAndPerfectedTier);

            // Every director wakes before it picks its first elite, which a stage-begin event does not guarantee.
            On.RoR2.CombatDirector.Awake += (orig, self) =>
            {
                RefreshVoidAndPerfectedTier();
                orig(self);
            };

            On.RoR2.Artifacts.EliteOnlyArtifactManager.PromoteIfHonor += PromoteFinalBoss;
        }

        private static void RefreshVoidAndPerfectedTier()
        {
            // The director picks the last affordable tier, so after a loop this tier would hide vanilla's
            // equally priced Tier 2 (Malachite, Celestine). Carrying Tier 2's elites once they unlock keeps
            // them in the pool.
            CombatDirector.EliteTierDef tier2 = EliteAPI.GetVanillaEliteTierDef(VanillaEliteTier.Tier2);
            EliteDef[] karmaElites = { RoR2Content.Elites.Lunar, DLC1Content.Elites.Void };

            voidAndPerfectedTier.eliteTypes = Run.instance && Run.instance.loopClearCount > 0 && tier2 != null
                ? karmaElites.Concat(tier2.eliteTypes).ToArray()
                : karmaElites;
        }

        /// <summary>
        /// Every final boss fight spawns its bosses through <c>ScriptedCombatEncounter</c>, which calls this
        /// for Artifact of Honor. Like Honor, it gives only the aspect, not the elite health and damage boost.
        /// </summary>
        private static void PromoteFinalBoss(
            On.RoR2.Artifacts.EliteOnlyArtifactManager.orig_PromoteIfHonor orig,
            CharacterMaster master,
            Xoroshiro128Plus rng,
            EliteDef[] eliteDefs)
        {
            CharacterBody body = master ? master.GetBody() : null;
            if (KarmaRun.Level < KarmaManager.maxKarma || !body || !KarmaRun.IsFinalBoss(body))
            {
                orig(master, rng, eliteDefs);
                return;
            }

            // One roll per fight: each stage holds one final boss fight, so every phase reuses this roll.
            if (fightAspectStage != Stage.instance)
            {
                EliteDef[] available = EliteOnlyArtifactManager.eliteDefs.Where(elite => elite.IsAvailable()).ToArray();
                fightAspect = available.Length > 0 ? available[rng.RangeInt(0, available.Length)] : null;
                fightAspectStage = Stage.instance;
            }

            if (fightAspect)
            {
                master.inventory.SetEquipmentIndex(fightAspect.eliteEquipmentDef.equipmentIndex, master.inventory.activeEquipmentLocation);
            }
        }
    }
}
