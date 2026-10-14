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
            FreeHeights(controller);
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
        /// Stops the panels overlapping each other.
        ///
        /// RightPanel stacks HeaderContainer, MedalPanel and InfoPanel in a VerticalLayoutGroup, and
        /// each carries a LayoutElement with a height authored for vanilla's contents. MedalPanel was
        /// sized for one medal row, so our second row and the Karma section rendered outside its
        /// allocation - spilling up into the survivor grid and down into the info box - and the longer
        /// 9-16 description overflowed InfoPanel the same way.
        ///
        /// Releasing those fixed heights lets the layout group allocate what the contents actually need.
        /// A ContentSizeFitter then grows the column itself, which is only legal on RightPanel because
        /// its parent ("Main Panel") has no layout group of its own - putting one on a layout group's
        /// child is what makes Unity layouts overlap.
        /// </summary>
        private static void FreeHeights(EclipseRunScreenController controller)
        {
            LanguageTextMeshController description = controller.eclipseDifficultyDescription;
            if (!description)
            {
                return;
            }

            Release(description.GetComponent<LayoutElement>());

            Transform infoPanel = description.transform.parent;
            Transform rightPanel = controller.transform.Find("Main Panel/RightPanel");

            if (infoPanel)
            {
                Release(infoPanel.GetComponent<LayoutElement>());
                SizeToChildren(infoPanel.GetComponent<VerticalLayoutGroup>());

                // Added in an earlier pass; on a layout group's child it causes exactly the overlap
                // it was meant to fix.
                ContentSizeFitter strayFitter = infoPanel.GetComponent<ContentSizeFitter>();
                if (strayFitter)
                {
                    Object.Destroy(strayFitter);
                }
            }

            if (!rightPanel)
            {
                return;
            }

            foreach (Transform child in rightPanel)
            {
                // Border images are backdrops and must keep stretching behind the content.
                if (child.name.Contains("Border") || child.name.Contains("Blur"))
                {
                    continue;
                }

                Release(child.GetComponent<LayoutElement>());
                SizeToChildren(child.GetComponent<VerticalLayoutGroup>());
            }

            VerticalLayoutGroup column = rightPanel.GetComponent<VerticalLayoutGroup>();
            SizeToChildren(column);

            if (column)
            {
                // Without this the stacked panels keep their own widths, so widening the column left
                // them ragged and differently sized rather than filling it.
                column.childControlWidth = true;
                column.childForceExpandWidth = true;
            }

            PinTopEdge(rightPanel as RectTransform);

            ContentSizeFitter fitter = rightPanel.GetComponent<ContentSizeFitter>();
            if (!fitter)
            {
                fitter = rightPanel.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>
        /// Moves the pivot to the top edge without moving the panel. A ContentSizeFitter grows a rect
        /// around its pivot, so with a centred pivot the column grew upward and downward at once; pinned
        /// at the top it keeps vanilla's top edge and only grows down.
        /// </summary>
        private static void PinTopEdge(RectTransform rect)
        {
            if (!rect || Mathf.Approximately(rect.pivot.y, 1f))
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float topBefore = corners[1].y;

            rect.pivot = new Vector2(rect.pivot.x, 1f);

            rect.GetWorldCorners(corners);
            rect.position += new Vector3(0f, topBefore - corners[1].y, 0f);
        }

        private static void SizeToChildren(VerticalLayoutGroup layout)
        {
            if (!layout)
            {
                return;
            }

            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
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
