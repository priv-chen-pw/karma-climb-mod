using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Adds medals for Eclipse 9-16 to the Eclipse menu.
    ///
    /// Vanilla ships one <see cref="EclipseDifficultyMedalDisplay"/> object per level with a serialized
    /// <c>eclipseLevel</c>, so there are exactly eight and nothing grows the row. Each new medal is a clone
    /// of the Eclipse 8 medal with its three state sprites swapped for our art (unearned shows the
    /// Inactive medal, either earned state the Active one), so vanilla's own Refresh switches between
    /// them exactly as it does for Eclipse 1-8.
    /// </summary>
    internal static class EclipseMenuMedals
    {
        internal static void Init()
        {
            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                AddMedals(self);
            };

            // A medal's OnEnable calls Refresh during Instantiate, which for our clones happens before
            // we assign eclipseLevel - so they paint themselves as the template's level and never correct
            // themselves. Vanilla only refreshes again on a survivor preference change, which is why the
            // row looked wrong until you switched character. UpdateDisplayedSurvivor runs both when the
            // menu discovers its event system and on every survivor change, so refreshing here covers
            // menu entry and switching alike.
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
                if (parent && parent.name == EclipseMenuKarmaPanel.BadgeRowName)
                {
                    continue;
                }

                display.Refresh();
            }
        }

        private static void AddMedals(EclipseRunScreenController controller)
        {
            EclipseDifficultyMedalDisplay template = null;

            foreach (EclipseDifficultyMedalDisplay display in
                controller.GetComponentsInChildren<EclipseDifficultyMedalDisplay>(includeInactive: true))
            {
                if (display.eclipseLevel > KarmaClimbLadder.VanillaMaxLevel)
                {
                    // Already extended, nothing to do.
                    return;
                }

                if (!template || display.eclipseLevel > template.eclipseLevel)
                {
                    template = display;
                }
            }

            if (!template)
            {
                Log.Warning("Eclipse menu has no medal displays; levels 9-16 will have no medals.");
                return;
            }

            Transform secondRow = CreateSecondRow(template.transform.parent);
            int added = 0;

            for (int level = KarmaClimbLadder.VanillaMaxLevel + 1; level <= KarmaClimbLadder.MaxLevel; level++)
            {
                Sprite active = KarmaClimbMedalArt.Active(level);
                Sprite inactive = KarmaClimbMedalArt.Inactive(level);

                if (!active || !inactive)
                {
                    Log.Warning($"Missing medal art for Eclipse {level}; skipping its medal.");
                    continue;
                }

                GameObject clone = Object.Instantiate(template.gameObject, secondRow);
                clone.name = $"KarmaClimbMedal{level}";

                EclipseDifficultyMedalDisplay display = clone.GetComponent<EclipseDifficultyMedalDisplay>();
                display.eclipseLevel = level;
                display.unearnedSprite = inactive;
                display.incompleteSprite = active;
                display.completeSprite = active;

                if (display.iconImage)
                {
                    display.iconImage.color = Color.white;
                }

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
    }
}
