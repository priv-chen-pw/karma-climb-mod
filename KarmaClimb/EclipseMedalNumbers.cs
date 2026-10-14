using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Draws the level number on the Eclipse 9-16 medals.
    ///
    /// The vanilla medal numerals are baked into the sprite and there is no art above 8, so cloning
    /// Eclipse 1-8 produced medals numbered 1-8. Rather than try to paint over the baked numeral, these
    /// medals drop the ring art entirely for a plain filled circle that we tint and label ourselves.
    /// That also makes them read as a distinct Karma tier rather than as duplicate Eclipse medals.
    /// </summary>
    internal static class EclipseMedalNumbers
    {
        private const string CircleAddress = "RoR2/Base/Common/texCircle256Aliased.png";
        private const string LabelName = "KarmaClimbLevelNumber";

        private static Sprite circleSprite;
        private static bool circleLoadAttempted;

        internal static Sprite CircleSprite
        {
            get
            {
                if (!circleLoadAttempted)
                {
                    circleLoadAttempted = true;
                    try
                    {
                        circleSprite = Addressables.LoadAssetAsync<Sprite>(CircleAddress).WaitForCompletion();
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"Could not load {CircleAddress} ({e.Message}); medals 9-16 keep the Eclipse artwork.");
                    }
                }

                return circleSprite;
            }
        }

        /// <summary>Adds the number label to a medal, reusing a menu text for its font.</summary>
        internal static void AddLabel(GameObject medal, int eclipseLevel, LanguageTextMeshController fontSource)
        {
            if (!fontSource || medal.transform.Find(LabelName))
            {
                return;
            }

            GameObject label = Object.Instantiate(fontSource.gameObject, medal.transform);
            label.name = LabelName;
            label.SetActive(true);

            // Otherwise it would keep resolving its old token over our text.
            LanguageTextMeshController languageController = label.GetComponent<LanguageTextMeshController>();
            if (languageController)
            {
                languageController.enabled = false;
                Object.Destroy(languageController);
            }

            foreach (LayoutElement layoutElement in label.GetComponents<LayoutElement>())
            {
                layoutElement.ignoreLayout = true;
            }

            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            TMP_Text text = label.GetComponent<TMP_Text>();
            if (!text)
            {
                Object.Destroy(label);
                return;
            }

            text.text = eclipseLevel.ToString();
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = 8f;
            text.fontSizeMax = 32f;
            text.margin = Vector4.zero;
        }

        internal static TMP_Text FindLabel(GameObject medal)
        {
            Transform label = medal.transform.Find(LabelName);
            return label ? label.GetComponent<TMP_Text>() : null;
        }
    }
}
