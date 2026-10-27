using R2API;
using RoR2;

namespace KarmaClimb
{
    /// <summary>
    /// Karma's stat changes to enemies: +10% damage at Karma 1, +15% attack speed at Karma 2, and +30%
    /// health for final bosses at Karma 7.
    ///
    /// Vanilla gives Mithrix alone +1% damage per Karma level and +30% attack speed at Karma 2
    /// (<c>CharacterBody.RecalculateStats</c>). Karma Climb replaces both with the all-enemy bonuses, so
    /// they are divided back out here. That is exact because R2API's total multipliers and vanilla's
    /// factors are plain multiplications with no clamp in between. Mithrix keeps vanilla's +30% movement.
    /// </summary>
    internal static class KarmaStatEffects
    {
        private const float DamageMult = 1.1f;
        private const float AttackSpeedMult = 1.15f;
        private const float FinalBossHealthMult = 1.3f;

        internal static void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += (body, args) =>
            {
                int karma = KarmaRun.Level;
                if (karma <= 0 || !body.teamComponent ||
                    body.teamComponent.teamIndex is not (TeamIndex.Monster or TeamIndex.Void or TeamIndex.Lunar))
                {
                    return;
                }

                args.damageTotalMult *= DamageMult;
                if (karma >= 2)
                {
                    args.attackSpeedTotalMult *= AttackSpeedMult;
                }

                // Same check vanilla uses, so this undoes exactly what vanilla applied.
                bool mithrix = BrotherKarmaUtils.IsBrother(body);
                if (mithrix)
                {
                    args.damageTotalMult /= 1f + 0.01f * karma;
                    if (karma >= 2)
                    {
                        args.attackSpeedTotalMult /= 1.3f;
                    }
                }

                // From Karma 8 Mithrix has vanilla's +50% health instead.
                if (karma >= 7 && KarmaRun.IsFinalBoss(body) && !(mithrix && karma >= KarmaManager.maxKarma))
                {
                    args.healthTotalMult *= FinalBossHealthMult;
                }
            };
        }
    }
}
