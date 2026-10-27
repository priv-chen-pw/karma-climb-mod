using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace KarmaClimb
{
    /// <summary>
    /// Karma 4: after each 33% of maximum health lost, a final boss takes 80% less damage for 5 seconds.
    ///
    /// The trigger is vanilla's <c>BrotherSuperArmorController</c>, the component behind Mithrix's own
    /// Karma shell. It counts damage taken outside the shell and resets each time it fires. Mithrix keeps his
    /// vanilla shell (hits reduced to 1 damage) and gets no second one.
    /// </summary>
    internal static class KarmaBossShell
    {
        private const int RequiredKarma = 4;
        private const float HealthThreshold = 0.33f;
        private const float Duration = 5f;
        private const float DamageTakenMult = 0.2f;

        /// <summary>Kinetic Dampener's overlay at reduced opacity, fading out as the shell ends.</summary>
        private static readonly AnimationCurve OverlayAlpha = new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(0.8f, 0.5f), new Keyframe(1f, 0f));

        private static BuffDef shellBuff;

        internal static void Init()
        {
            shellBuff = ScriptableObject.CreateInstance<BuffDef>();
            shellBuff.name = "bdKarmaClimbShell";
            shellBuff.canStack = false;
            shellBuff.isDebuff = false;
            ContentAddition.AddBuffDef(shellBuff);

            CharacterBody.onBodyStartGlobal += AddShell;

            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                if (self.body && self.body.HasBuff(shellBuff))
                {
                    damageInfo.damage *= DamageTakenMult;
                }

                orig(self, damageInfo);
            };

            // Runs on the server and, through the buff sync, on every client, so each machine draws its own overlay.
            On.RoR2.CharacterBody.OnBuffFirstStackGained += (orig, self, buffDef) =>
            {
                orig(self, buffDef);
                if (buffDef == shellBuff)
                {
                    ShowOverlay(self);
                }
            };
        }

        private static void AddShell(CharacterBody body)
        {
            if (!NetworkServer.active || KarmaRun.Level < RequiredKarma || !KarmaRun.IsFinalBoss(body) ||
                BrotherKarmaUtils.IsBrother(body))
            {
                return;
            }

            // Borrow Mithrix's burst effect and buff icon when his prefab carries them.
            GameObject mithrixPrefab = BodyCatalog.FindBodyPrefab("BrotherBody");
            BrotherSuperArmorController mithrixShell = mithrixPrefab ? mithrixPrefab.GetComponent<BrotherSuperArmorController>() : null;
            if (!shellBuff.iconSprite && mithrixShell && mithrixShell.buffToApply)
            {
                shellBuff.iconSprite = mithrixShell.buffToApply.iconSprite;
            }

            BrotherSuperArmorController shell = body.gameObject.AddComponent<BrotherSuperArmorController>();
            shell.healthThreshold = HealthThreshold;
            shell.buffToApply = shellBuff;
            shell.buffDuration = Duration;
            shell.requiredKarma = RequiredKarma;
            shell.applicationEffectPrefab = mithrixShell ? mithrixShell.applicationEffectPrefab : null;
        }

        private static void ShowOverlay(CharacterBody body)
        {
            Transform modelTransform = body.modelLocator ? body.modelLocator.modelTransform : null;
            CharacterModel model = modelTransform ? modelTransform.GetComponent<CharacterModel>() : null;
            if (!model || !CharacterModel.shieldBoosterOverlay)
            {
                return;
            }

            TemporaryOverlayInstance overlay = TemporaryOverlayManager.AddOverlay(body.gameObject);
            overlay.duration = Duration;
            overlay.animateShaderAlpha = true;
            overlay.alphaCurve = OverlayAlpha;
            overlay.destroyComponentOnEnd = true;
            overlay.originalMaterial = CharacterModel.shieldBoosterOverlay;
            overlay.AddToCharacterModel(model);
        }
    }
}
