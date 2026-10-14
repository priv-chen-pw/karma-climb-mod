using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Widens the Eclipse menu's right-hand column and lets the difficulty description box grow with
    /// its text. The vanilla panel is sized for eight lines of Eclipse 8 description; levels 9-16 add a
    /// Karma line, and the extra medal row and Karma section need the width too.
    /// </summary>
    internal static class EclipseMenuLayout
    {
        private static float extraWidth;

        internal static void Init(float rightPanelExtraWidth)
        {
            extraWidth = rightPanelExtraWidth;

            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                Apply(self);
            };
        }

        private static void Apply(EclipseRunScreenController controller)
        {
            Widen(controller);
            FreeDescriptionHeight(controller);
        }

        /// <summary>Grows the right column leftwards, keeping its right edge where it is.</summary>
        private static void Widen(EclipseRunScreenController controller)
        {
            if (extraWidth <= 0f)
            {
                return;
            }

            Transform rightPanel = controller.transform.Find("Main Panel/RightPanel");
            if (!rightPanel)
            {
                Log.Warning("No 'Main Panel/RightPanel'; leaving the Eclipse menu width alone.");
                return;
            }

            RectTransform rect = rightPanel.GetComponent<RectTransform>();
            if (!rect)
            {
                return;
            }

            Vector2 size = rect.sizeDelta;
            size.x += extraWidth;
            rect.sizeDelta = size;

            // Shifting by (1 - pivot.x) * extra keeps the right edge fixed for any pivot.
            Vector2 position = rect.anchoredPosition;
            position.x -= (1f - rect.pivot.x) * extraWidth;
            rect.anchoredPosition = position;

            Log.Debug($"Widened the Eclipse right panel by {extraWidth}.");
        }

        /// <summary>
        /// Clears the fixed heights on the description and on the InfoPanel holding it, so both size to
        /// the text. Freeing only the description was not enough: its container stayed fixed and the
        /// text simply overflowed the border.
        /// </summary>
        private static void FreeDescriptionHeight(EclipseRunScreenController controller)
        {
            LanguageTextMeshController description = controller.eclipseDifficultyDescription;
            if (!description)
            {
                return;
            }

            Release(description.GetComponent<LayoutElement>());

            Transform infoPanel = description.transform.parent;
            if (!infoPanel)
            {
                return;
            }

            Release(infoPanel.GetComponent<LayoutElement>());

            VerticalLayoutGroup layout = infoPanel.GetComponent<VerticalLayoutGroup>();
            if (layout)
            {
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;
            }

            ContentSizeFitter fitter = infoPanel.GetComponent<ContentSizeFitter>();
            if (!fitter)
            {
                fitter = infoPanel.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>-1 means "no override", handing sizing back to the content.</summary>
        private static void Release(LayoutElement layoutElement)
        {
            if (!layoutElement)
            {
                return;
            }

            layoutElement.preferredHeight = -1f;
            layoutElement.minHeight = -1f;
            layoutElement.flexibleHeight = -1f;
        }
    }
}
