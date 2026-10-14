using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaClimb
{
    /// <summary>
    /// Makes Escape leave the Eclipse menu, which otherwise has no keyboard way back.
    /// It clicks the menu's own Back button rather than driving the scene change itself, so whatever
    /// that button does stays the single source of truth.
    /// </summary>
    internal class EclipseMenuEscape : MonoBehaviour
    {
        private Button backButton;

        internal static void Init()
        {
            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);

                if (!self.GetComponent<EclipseMenuEscape>())
                {
                    self.gameObject.AddComponent<EclipseMenuEscape>();
                }
            };
        }

        private void Start()
        {
            Transform back = transform.Find("Main Panel/BackButton");

            if (!back)
            {
                // Fall back to a name search in case a patch moves it.
                foreach (Button candidate in GetComponentsInChildren<Button>(includeInactive: true))
                {
                    if (candidate.name.Contains("BackButton"))
                    {
                        back = candidate.transform;
                        break;
                    }
                }
            }

            if (!back)
            {
                Log.Warning("No Back button in the Eclipse menu; Escape will not leave it.");
                return;
            }

            backButton = back.GetComponent<Button>();
        }

        private void Update()
        {
            if (!backButton || !Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (backButton.interactable)
            {
                backButton.onClick.Invoke();
            }
        }
    }
}
