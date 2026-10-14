using System.Collections.Generic;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>Not yet reached. Dark, matching the unearned Eclipse medals.</summary>
        private static readonly Color UnearnedColor = new Color32(26, 36, 48, 255);

        private static readonly Color EarnedNumberColor = new Color32(12, 28, 38, 255);
        private static readonly Color UnearnedNumberColor = new Color32(86, 102, 118, 255);

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

            // A medal's OnEnable calls Refresh during Instantiate, which for our clones happens before
            // we assign eclipseLevel - so they paint themselves as levels 1-8 and never correct
            // themselves. Vanilla only refreshes again on a survivor preference change, which is why
            // the row looked wrong until you switched character. UpdateDisplayedSurvivor runs both when
            // the menu discovers its event system and on every survivor change, so refreshing here
            // covers menu entry and switching alike.
            On.RoR2.UI.EclipseRunScreenController.UpdateDisplayedSurvivor += (orig, self) =>
            {
                orig(self);
                RefreshAll(self);
            };
        }

        private static void RefreshAll(EclipseRunScreenController controller)
        {
            EclipseDifficultyMedalDisplay[] displays =
                controller.GetComponentsInChildren<EclipseDifficultyMedalDisplay>(includeInactive: true);

            foreach (EclipseDifficultyMedalDisplay display in displays)
            {
                // The Karma badges are clones of a medal whose display component is destroyed, but the
                // destroy is deferred a frame - don't let a refresh repaint them in the meantime.
                Transform parent = display.transform.parent;
                if (parent && parent.name == EclipseMenuKarmaSection.BadgeRowName)
                {
                    continue;
                }

                display.Refresh();
            }
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
            Transform secondRow = null;

            for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
            {
                int karma = KarmaClimbLadder.KarmaLevelFor(level);

                if (!vanillaByLevel.TryGetValue(karma, out EclipseDifficultyMedalDisplay template))
                {
                    Log.Warning($"No vanilla medal for Eclipse {karma}; skipping the medal for Eclipse {level}.");
                    continue;
                }

                if (!secondRow)
                {
                    secondRow = CreateSecondRow(template.transform.parent);
                }

                GameObject clone = Object.Instantiate(template.gameObject, secondRow);
                clone.name = $"KarmaClimbMedal{level}";
                clone.GetComponent<EclipseDifficultyMedalDisplay>().eclipseLevel = level;
                EclipseMedalNumbers.AddLabel(clone, level, controller.eclipseDifficultyName);
                added++;
            }

            Log.Info($"Added {added} Eclipse medals to the menu (levels {KarmaClimbLadder.VanillaMaxLevel + 1}-{KarmaClimbLadder.MaxLevel}).");
        }

        /// <summary>
        /// Sixteen medals in the vanilla single row would be squeezed, so 9-16 get their own row
        /// directly beneath it. The layout settings are copied from the vanilla row rather than
        /// cloning it, which would drag along eight medal children we would then have to destroy.
        /// </summary>
        private static Transform CreateSecondRow(Transform vanillaRow)
        {
            GameObject row = new GameObject("KarmaClimbMedalRow", typeof(RectTransform));
            row.transform.SetParent(vanillaRow.parent, worldPositionStays: false);
            row.transform.SetSiblingIndex(vanillaRow.GetSiblingIndex() + 1);

            HorizontalLayoutGroup source = vanillaRow.GetComponent<HorizontalLayoutGroup>();
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();

            if (source)
            {
                layout.padding = source.padding;
                layout.spacing = source.spacing;
                layout.childAlignment = source.childAlignment;
                layout.childControlWidth = source.childControlWidth;
                layout.childControlHeight = source.childControlHeight;
                layout.childForceExpandWidth = source.childForceExpandWidth;
                layout.childForceExpandHeight = source.childForceExpandHeight;
                layout.childScaleWidth = source.childScaleWidth;
                layout.childScaleHeight = source.childScaleHeight;
                layout.reverseArrangement = source.reverseArrangement;
            }

            return row.transform;
        }

        private static void Recolour(EclipseDifficultyMedalDisplay display)
        {
            if (display.eclipseLevel <= KarmaClimbLadder.VanillaMaxLevel)
            {
                return;
            }

            Image image = display.iconImage;
            if (!image)
            {
                return;
            }

            // Refresh has just assigned one of the three sprites, so read the state before replacing it.
            bool unearned = image.sprite == display.unearnedSprite;
            bool earnedByEveryone = image.sprite == display.completeSprite;

            Sprite circle = EclipseMedalNumbers.CircleSprite;
            if (circle)
            {
                image.sprite = circle;
            }

            image.color = unearned ? UnearnedColor : earnedByEveryone ? CompleteColor : IncompleteColor;

            TMP_Text label = EclipseMedalNumbers.FindLabel(display.gameObject);
            if (label)
            {
                label.color = unearned ? UnearnedNumberColor : EarnedNumberColor;
            }
        }
    }
}
