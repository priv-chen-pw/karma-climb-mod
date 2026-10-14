using TMPro;
using UnityEngine;

namespace KarmaClimb
{
    /// <summary>
    /// Keeps the Karma box directly below the Eclipse gamemode paragraph.
    ///
    /// The paragraph's RectTransform is only 50px tall while its text renders over several lines, so
    /// anchoring to the rect would put the box on top of the text. Instead this follows the text's
    /// actual rendered bounds, which also copes with other languages and with our longer description.
    /// It runs in LateUpdate because the text only resolves and lays out after the menu is enabled, and
    /// it keeps tracking because the left column slides in on open.
    /// </summary>
    internal class KarmaPanelPlacer : MonoBehaviour
    {
        internal RectTransform anchor;
        internal float gap;

        private readonly Vector3[] corners = new Vector3[4];
        private TMP_Text anchorText;

        private void Start()
        {
            anchorText = anchor ? anchor.GetComponent<TMP_Text>() : null;
        }

        private void LateUpdate()
        {
            if (!anchor)
            {
                return;
            }

            anchor.GetWorldCorners(corners);
            float left = corners[1].x;
            float bottom = corners[0].y;

            // Prefer the rendered text's lowest point over the rect's bottom edge.
            if (anchorText && anchorText.textBounds.size.y > 0f)
            {
                bottom = Mathf.Min(bottom, anchor.TransformPoint(new Vector3(0f, anchorText.textBounds.min.y, 0f)).y);
            }

            Vector3 target = new Vector3(left, bottom - gap * anchor.lossyScale.y, transform.position.z);

            // Moving a UI transform dirties its canvas, so only move when it is actually off.
            if ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = target;
            }
        }
    }
}
