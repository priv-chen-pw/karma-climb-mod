using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Builds the Karma box in the Eclipse menu's left column, under the gamemode description: a
    /// "Karma" heading, one badge per Karma level, and a hint line. Hovering a badge opens the game's
    /// standard tooltip popup, the same one character select uses for skills.
    ///
    /// It lives on the left because the right column is already full (survivors, two medal rows and
    /// the difficulty description), while the left column has several hundred pixels free between the
    /// paragraph and the Start button. Its backdrop is copied from the right column so the two boxes
    /// match, and its text is cloned from the menu's own so fonts come from the game.
    /// </summary>
    internal static class EclipseMenuKarmaPanel
    {
        internal const string PanelName = "KarmaClimbKarmaPanel";
        internal const string BadgeRowName = "KarmaBadgeRow";

        /// <summary>Space between the gamemode paragraph and the box.</summary>
        private const float GapBelowParagraph = 32f;

        private const float BadgeSpacing = 6f;

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
            Transform leftColumn = controller.transform.Find("Main Panel/GenericMenuButtonPanel");
            RectTransform paragraph = leftColumn ? leftColumn.Find("Description") as RectTransform : null;
            EclipseDifficultyMedalDisplay medal =
                controller.GetComponentInChildren<EclipseDifficultyMedalDisplay>(includeInactive: true);

            if (!leftColumn || !paragraph || !medal)
            {
                Log.Warning("Eclipse menu is missing the left column, its description or a medal; skipping the Karma box.");
                return;
            }

            if (leftColumn.Find(PanelName))
            {
                return;
            }

            // Parented to the left column so it fades in with it (it carries the menu's UIJuice).
            GameObject panel = new GameObject(PanelName, typeof(RectTransform));
            panel.transform.SetParent(leftColumn, worldPositionStays: false);

            RectTransform rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            // Top-left pivot: the box grows downward when a long description is shown, so the badges
            // never slide out from under the cursor.
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(paragraph.rect.width, 0f);

            Transform rightPanel = controller.transform.Find("Main Panel/RightPanel");
            CopyBackdrop(rightPanel, panel);

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            VerticalLayoutGroup rightLayout = rightPanel ? rightPanel.GetComponent<VerticalLayoutGroup>() : null;
            layout.padding = rightLayout
                ? new RectOffset(rightLayout.padding.left, rightLayout.padding.right, rightLayout.padding.top, rightLayout.padding.bottom)
                : new RectOffset(16, 16, 16, 16);
            layout.spacing = rightLayout ? rightLayout.spacing : 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Legal here: the left column has no layout group of its own to fight it.
            ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddText(controller.eclipseDifficultyName, panel.transform, "KarmaHeading", KarmaDescriptions.HeadingToken, sizeToText: false);
            AddBadgeRow(panel.transform, medal);
            AddText(controller.eclipseDifficultyDescription, panel.transform, "KarmaHint", KarmaDescriptions.HintToken, sizeToText: true);

            KarmaPanelPlacer placer = panel.AddComponent<KarmaPanelPlacer>();
            placer.anchor = paragraph;
            placer.gap = GapBelowParagraph;

            Log.Info("Built the Karma box in the Eclipse menu's left column.");
        }

        /// <summary>Gives the box the same background, blur and border as the right column.</summary>
        private static void CopyBackdrop(Transform rightPanel, GameObject panel)
        {
            if (!rightPanel)
            {
                return;
            }

            Image source = rightPanel.GetComponent<Image>();
            if (source)
            {
                Image background = panel.AddComponent<Image>();
                background.sprite = source.sprite;
                background.type = source.type;
                background.color = source.color;
                background.material = source.material;
                background.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
                background.raycastTarget = false;
            }

            // Backdrop children, drawn behind the content because they come first.
            foreach (string partName in new[] { "BlurPanel", "BorderImage" })
            {
                Transform part = rightPanel.Find(partName);
                if (!part)
                {
                    continue;
                }

                GameObject clone = Object.Instantiate(part.gameObject, panel.transform);
                clone.name = partName;

                RectTransform cloneRect = (RectTransform)clone.transform;
                cloneRect.anchorMin = Vector2.zero;
                cloneRect.anchorMax = Vector2.one;
                cloneRect.offsetMin = Vector2.zero;
                cloneRect.offsetMax = Vector2.zero;

                LayoutElement layoutElement = clone.GetComponent<LayoutElement>();
                if (!layoutElement)
                {
                    layoutElement = clone.AddComponent<LayoutElement>();
                }

                layoutElement.ignoreLayout = true;
            }
        }

        /// <param name="sizeToText">
        /// Clear the template's fixed height so the clone sizes to its own text. The Eclipse description
        /// reserves 400px, and inheriting that is what pushed the right column off-screen previously.
        /// </param>
        private static LanguageTextMeshController AddText(
            LanguageTextMeshController template, Transform parent, string name, string token, bool sizeToText)
        {
            if (!template)
            {
                Log.Warning($"No text template for {name}; it will be missing from the Karma box.");
                return null;
            }

            LanguageTextMeshController clone = Object.Instantiate(template, parent);
            clone.gameObject.name = name;
            clone.token = token;

            if (sizeToText)
            {
                foreach (LayoutElement layoutElement in clone.GetComponents<LayoutElement>())
                {
                    layoutElement.minHeight = -1f;
                    layoutElement.preferredHeight = -1f;
                    layoutElement.flexibleHeight = -1f;
                }
            }

            return clone;
        }

        private static void AddBadgeRow(Transform parent, EclipseDifficultyMedalDisplay medalTemplate)
        {
            GameObject row = new GameObject(BadgeRowName, typeof(RectTransform));
            row.transform.SetParent(parent, worldPositionStays: false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = BadgeSpacing;

            RectTransform medalRect = medalTemplate.GetComponent<RectTransform>();
            float badgeHeight = medalRect && medalRect.rect.height > 0f ? medalRect.rect.height : 64f;

            LayoutElement rowSize = row.AddComponent<LayoutElement>();
            rowSize.minHeight = badgeHeight;
            rowSize.preferredHeight = badgeHeight;

            for (int karma = 1; karma <= KarmaClimbLadder.AddedLevelCount; karma++)
            {
                GameObject badge = Object.Instantiate(medalTemplate.gameObject, row.transform);
                badge.name = $"KarmaBadge{karma}";

                // The template may have been inactive, and a clone of an inactive object stays inactive.
                badge.SetActive(true);

                // A static informational badge, not a progress medal. Disabled first so it cannot repaint
                // our sprite before the deferred destroy lands.
                EclipseDifficultyMedalDisplay progressDisplay = badge.GetComponent<EclipseDifficultyMedalDisplay>();
                if (progressDisplay)
                {
                    progressDisplay.enabled = false;
                    Object.Destroy(progressDisplay);
                }

                Image image = badge.GetComponent<Image>();
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

                // Same colour vanilla uses for Karma levels in its own rule tooltips.
                tooltip.titleToken = KarmaDescriptions.TitleToken(karma);
                tooltip.titleColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarCoin);
                tooltip.bodyToken = KarmaDescriptions.BodyToken(karma);
            }
        }

        /// <summary>Reuses the Karma icons vanilla assigns to the Karma.InitialLevel rule choices.</summary>
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
