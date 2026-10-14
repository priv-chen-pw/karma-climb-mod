using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// A standalone panel on the left of the Eclipse menu that shows the hovered Karma level's
    /// description, instead of a cursor-following tooltip.
    ///
    /// It is cloned from the menu's own "DescriptionPanel, Naked" so it arrives with the game's blur
    /// backing, corner art, padding and font already set up. It is parented to "Main Panel", which has
    /// no layout group, so it can be anchored freely without a layout fighting it.
    /// </summary>
    internal static class KarmaDescriptionPanel
    {
        internal const string PanelName = "KarmaClimbKarmaDescriptionPanel";

        private const float LeftMargin = 48f;
        private const float PanelWidth = 440f;

        /// <summary>Builds the panel and returns the text controller to drive, or null on failure.</summary>
        internal static LanguageTextMeshController Build(EclipseRunScreenController controller, out GameObject panel)
        {
            panel = null;

            Transform mainPanel = controller.transform.Find("Main Panel");
            if (!mainPanel)
            {
                Log.Warning("No 'Main Panel' in the Eclipse menu; Karma descriptions will fall back to tooltips.");
                return null;
            }

            Transform template = FindDescriptionPanelTemplate(controller.transform);
            if (!template)
            {
                Log.Warning("No description panel to clone; Karma descriptions will fall back to tooltips.");
                return null;
            }

            GameObject clone = Object.Instantiate(template.gameObject, mainPanel);
            clone.name = PanelName;
            clone.SetActive(true);

            // It came out of a VerticalLayoutGroup; here it positions itself.
            LayoutElement layoutElement = clone.GetComponent<LayoutElement>();
            if (layoutElement)
            {
                layoutElement.ignoreLayout = true;
            }

            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(LeftMargin, 0f);
            rect.sizeDelta = new Vector2(PanelWidth, rect.sizeDelta.y);

            ContentSizeFitter fitter = clone.GetComponent<ContentSizeFitter>();
            if (!fitter)
            {
                fitter = clone.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LanguageTextMeshController text = clone.GetComponentInChildren<LanguageTextMeshController>(includeInactive: true);
            if (!text)
            {
                Log.Warning("Cloned description panel has no text controller; Karma descriptions will fall back to tooltips.");
                Object.Destroy(clone);
                return null;
            }

            text.token = string.Empty;
            clone.SetActive(false);

            panel = clone;
            return text;
        }

        private static Transform FindDescriptionPanelTemplate(Transform root)
        {
            Transform direct = root.Find("Main Panel/GenericMenuButtonPanel/JuicePanel/DescriptionPanel, Naked");
            if (direct)
            {
                return direct;
            }

            // Fall back to a name search in case the menu is rearranged by a patch.
            foreach (RectTransform candidate in root.GetComponentsInChildren<RectTransform>(includeInactive: true))
            {
                if (candidate.name.StartsWith("DescriptionPanel")
                    && candidate.GetComponentInChildren<LanguageTextMeshController>(includeInactive: true))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
