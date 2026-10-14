using System.Collections.Generic;
using RoR2.UI;
using UnityEngine;

namespace KarmaClimb
{
    /// <summary>
    /// Adds medals for Eclipse 9-16 to the Eclipse menu and recolours them.
    ///
    /// Vanilla ships one <see cref="EclipseDifficultyMedalDisplay"/> object per level with a serialized
    /// <c>eclipseLevel</c>, so there are exactly eight and nothing grows the row automatically. Each new
    /// medal is cloned from the vanilla medal whose level equals its Karma level, so Eclipse 9 reuses the
    /// Eclipse 1 artwork, 10 reuses 2, and so on - the row visually mirrors the Karma climb.
    ///
    /// The filled states are then tinted cyan to separate them from the gold Eclipse medals. Tinting is
    /// applied to the non-gold sprite: multiplying cyan into the gold artwork would cancel its red
    /// channel and come out muddy.
    /// </summary>
    internal static class EclipseMenuMedals
    {
        /// <summary>Earned by every survivor. Cyan, in the tone of the Karma icon.</summary>
        private static readonly Color CompleteColor = new Color32(125, 240, 255, 255);

        /// <summary>Earned by this survivor but not all of them.</summary>
        private static readonly Color IncompleteColor = new Color32(62, 136, 156, 255);

        internal static void Init()
        {
            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                AddMedals(self);
            };

            On.EclipseDifficultyMedalDisplay.Refresh += (orig, self) =>
            {
                orig(self);
                Recolour(self);
            };
        }

        private static void AddMedals(EclipseRunScreenController controller)
        {
            EclipseDifficultyMedalDisplay[] existing =
                controller.GetComponentsInChildren<EclipseDifficultyMedalDisplay>(includeInactive: true);

            Dictionary<int, EclipseDifficultyMedalDisplay> vanillaByLevel =
                new Dictionary<int, EclipseDifficultyMedalDisplay>();

            foreach (EclipseDifficultyMedalDisplay display in existing)
            {
                if (display.eclipseLevel > KarmaClimbLadder.VanillaMaxLevel)
                {
                    // Already extended, nothing to do.
                    return;
                }

                if (display.eclipseLevel >= 1)
                {
                    vanillaByLevel[display.eclipseLevel] = display;
                }
            }

            if (vanillaByLevel.Count == 0)
            {
                Log.Warning("Eclipse menu has no medal displays; levels 9-16 will have no medals.");
                return;
            }

            int added = 0;

            for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
            {
                int karma = KarmaClimbLadder.KarmaLevelFor(level);

                if (!vanillaByLevel.TryGetValue(karma, out EclipseDifficultyMedalDisplay template))
                {
                    Log.Warning($"No vanilla medal for Eclipse {karma}; skipping the medal for Eclipse {level}.");
                    continue;
                }

                GameObject clone = Object.Instantiate(template.gameObject, template.transform.parent);
                clone.name = $"KarmaClimbMedal{level}";
                clone.GetComponent<EclipseDifficultyMedalDisplay>().eclipseLevel = level;
                added++;
            }

            Log.Info($"Added {added} Eclipse medals to the menu (levels {KarmaClimbLadder.VanillaMaxLevel + 1}-{KarmaClimbLadder.MaxLevel}).");
        }

        private static void Recolour(EclipseDifficultyMedalDisplay display)
        {
            if (display.eclipseLevel <= KarmaClimbLadder.VanillaMaxLevel)
            {
                return;
            }

            if (!display.iconImage)
            {
                return;
            }

            // Refresh has just assigned one of the three sprites, so this reads the current state.
            if (display.iconImage.sprite == display.unearnedSprite)
            {
                display.iconImage.color = Color.white;
                return;
            }

            bool earnedByEveryone = display.iconImage.sprite == display.completeSprite;

            display.iconImage.sprite = display.incompleteSprite;
            display.iconImage.color = earnedByEveryone ? CompleteColor : IncompleteColor;
        }
    }
}
