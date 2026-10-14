using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Builds the "Karma" block in the Eclipse menu: a heading, a hint line, and one badge per Karma
    /// level that shows what that level adds when hovered.
    ///
    /// Everything is cloned from objects already in the menu - the difficulty name text for the heading,
    /// a medal for the badges - so fonts, materials and sizing come from the menu itself instead of being
    /// built from scratch, which would need font assets we do not ship.
    /// </summary>
    internal static class EclipseMenuKarmaSection
    {
        internal static void Init()
        {
            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                Build(self);
            };
        }

        private static void Build(EclipseRunScreenController controller)
        {
            if (controller.transform.Find("KarmaClimbKarmaSection"))
            {
                return;
            }

            EclipseDifficultyMedalDisplay medal =
                controller.GetComponentInChildren<EclipseDifficultyMedalDisplay>(includeInactive: true);

            if (!medal)
            {
                Log.Warning("Eclipse menu has no medals to anchor the Karma section to; skipping it.");
                return;
            }

            Transform medalRow = medal.transform.parent;
            if (!medalRow)
            {
                return;
            }

            // Sit as the next sibling of the medal row, so in a vertical layout we land under the medals.
            GameObject section = new GameObject("KarmaClimbKarmaSection", typeof(RectTransform));
            section.transform.SetParent(medalRow.parent, worldPositionStays: false);
            section.transform.SetSiblingIndex(medalRow.GetSiblingIndex() + 1);

            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4f;

            AddText(controller.eclipseDifficultyName, section.transform, "KarmaHeading", KarmaDescriptions.HeadingToken);
            AddText(controller.eclipseDifficultyDescription, section.transform, "KarmaHint", KarmaDescriptions.HintToken);
            AddBadgeRow(section.transform, medal);

            Log.Info("Built the Karma section in the Eclipse menu.");
        }

        private static void AddText(LanguageTextMeshController template, Transform parent, string name, string token)
        {
            if (!template)
            {
                Log.Warning($"No text template available for {name}; it will be missing from the Karma section.");
                return;
            }

            LanguageTextMeshController clone = Object.Instantiate(template, parent);
            clone.gameObject.name = name;
            clone.token = token;
        }

        private static void AddBadgeRow(Transform parent, EclipseDifficultyMedalDisplay medalTemplate)
        {
            GameObject row = new GameObject("KarmaBadgeRow", typeof(RectTransform));
            row.transform.SetParent(parent, worldPositionStays: false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 4f;

            for (int karma = 1; karma <= KarmaClimbLadder.AddedLevelCount; karma++)
            {
                GameObject badge = Object.Instantiate(medalTemplate.gameObject, row.transform);
                badge.name = $"KarmaBadge{karma}";

                // The template may have been an inactive medal, and a clone of an inactive object
                // stays inactive.
                badge.SetActive(true);

                // It is a static informational badge, not a progress medal. Disabling it first stops
                // it repainting our sprite before the deferred destroy lands.
                EclipseDifficultyMedalDisplay progressDisplay = badge.GetComponent<EclipseDifficultyMedalDisplay>();
                if (progressDisplay)
                {
                    progressDisplay.enabled = false;
                    Object.Destroy(progressDisplay);
                }

                Image image = badge.GetComponentInChildren<Image>(includeInactive: true);
                if (image)
                {
                    Sprite karmaSprite = FindKarmaSprite(karma);
                    if (karmaSprite)
                    {
                        image.sprite = karmaSprite;
                    }

                    image.color = Color.white;
                    image.raycastTarget = true;
                }

                TooltipProvider tooltip = badge.GetComponent<TooltipProvider>();
                if (!tooltip)
                {
                    tooltip = badge.AddComponent<TooltipProvider>();
                }

                tooltip.titleToken = KarmaDescriptions.TitleToken(karma);
                tooltip.titleColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem);
                tooltip.bodyToken = KarmaDescriptions.BodyToken(karma);
            }
        }

        /// <summary>Reuses the Karma icons vanilla already assigns to the Karma.InitialLevel rule choices.</summary>
        private static Sprite FindKarmaSprite(int karmaLevel)
        {
            RuleChoiceDef choice = RuleCatalog.FindChoiceDef("Karma.InitialLevel." + karmaLevel);
            if (choice == null)
            {
                Log.Warning($"No Karma.InitialLevel.{karmaLevel} rule choice; the badge keeps the medal artwork.");
                return null;
            }

            return choice.sprite;
        }
    }
}
