using RoR2.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KarmaClimb
{
    /// <summary>
    /// Drives <see cref="KarmaDescriptionPanel"/> while a Karma badge is hovered. Works through the
    /// menu canvas's existing GraphicRaycaster, so the badge only needs a raycastable Image.
    /// </summary>
    internal class KarmaBadgeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal int karmaLevel;
        internal LanguageTextMeshController target;
        internal GameObject panel;

        public void OnPointerEnter(PointerEventData eventData) => SetShown(true);

        public void OnPointerExit(PointerEventData eventData) => SetShown(false);

        // Leaving the menu while hovered would otherwise strand the panel open.
        private void OnDisable() => SetShown(false);

        private void SetShown(bool shown)
        {
            if (!target)
            {
                return;
            }

            target.token = shown ? KarmaDescriptions.PanelToken(karmaLevel) : string.Empty;

            if (panel)
            {
                panel.SetActive(shown);
            }
        }
    }
}
