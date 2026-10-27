using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KarmaClimb
{
    /// <summary>
    /// Karma's changes to a stage: kill gold (Karma 1-5), a Shrine of the Mountain stack on the
    /// teleporter (Karma 3), monster credits (Karma 5) and interactable credits (Karma 7).
    /// </summary>
    internal static class KarmaDirectorEffects
    {
        private const float GoldBonusPerKarma = 0.04f;
        private const int GoldBonusMaxKarma = 5;
        private const float EclipseGoldMult = 0.8f;
        private const float MonsterCreditMult = 1.2f;
        private const float InteractableCreditMult = 1.2f;

        internal static void Init()
        {
            On.RoR2.DeathRewards.OnKilledServer += (orig, self, damageReport) =>
            {
                float bonus = GoldBonusPerKarma * Mathf.Min(KarmaRun.Level, GoldBonusMaxKarma);
                if (bonus <= 0f)
                {
                    orig(self, damageReport);
                    return;
                }

                // Eclipse 6 multiplies the reward by 0.8 inside orig. Scaling by (0.8 + bonus) / 0.8 adds the
                // bonus to that 0.8 instead, so Karma 5's +20% cancels Eclipse 6 exactly.
                uint gold = self.goldReward;
                self.goldReward = (uint)(gold * (EclipseGoldMult + bonus) / EclipseGoldMult);
                orig(self, damageReport);
                self.goldReward = gold;
            };

            On.RoR2.TeleporterInteraction.Start += (orig, self) =>
            {
                orig(self);

                // Final bosses have no teleporter, so this only ever reaches teleporter bosses.
                if (NetworkServer.active && KarmaRun.Level >= 3)
                {
                    self.AddShrineStack();
                }
            };

            SceneDirector.onPrePopulateSceneServer += director =>
            {
                int karma = KarmaRun.Level;
                if (karma >= 5)
                {
                    director.monsterCredit = (int)(director.monsterCredit * MonsterCreditMult);
                }

                if (karma >= 7)
                {
                    director.onPopulateCreditMultiplier *= InteractableCreditMult;
                }
            };

            On.RoR2.CombatDirector.Awake += (orig, self) =>
            {
                orig(self);
                if (!NetworkServer.active || KarmaRun.Level < 5 || self.moneyWaves == null)
                {
                    return;
                }

                // The teleporter boss's credit is left alone: extra bosses are Karma 3's job.
                TeleporterInteraction teleporter = self.GetComponent<TeleporterInteraction>();
                if (teleporter && teleporter.bossDirector == self)
                {
                    return;
                }

                foreach (CombatDirector.DirectorMoneyWave wave in self.moneyWaves)
                {
                    wave.multiplier *= MonsterCreditMult;
                }
            };
        }
    }
}
