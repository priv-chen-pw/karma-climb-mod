using System.Text;
using RoR2.UI;
using UnityEngine;

namespace KarmaClimb
{
    /// <summary>
    /// Logs the Eclipse menu's transform tree once, so the Karma section and the extra medals can be
    /// positioned against the real hierarchy instead of guessed at. Off unless enabled in the config.
    /// </summary>
    internal static class EclipseMenuDump
    {
        private static bool enabled;
        private static bool logged;

        internal static void Init(bool logHierarchy)
        {
            enabled = logHierarchy;
            if (!enabled)
            {
                return;
            }

            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                Dump(self);
            };
        }

        private static void Dump(EclipseRunScreenController controller)
        {
            if (logged)
            {
                return;
            }

            logged = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("Eclipse menu hierarchy:\n");
            Append(sb, controller.transform, 0);
            Log.Info(sb.ToString());
        }

        private static void Append(StringBuilder sb, Transform transform, int depth)
        {
            sb.Append(' ', depth * 2).Append("- ").Append(transform.name);

            // Sizes and layout overrides, so layout problems can be reasoned about from the log.
            RectTransform rect = transform as RectTransform;
            if (rect)
            {
                sb.Append("  rect=").Append(Mathf.Round(rect.rect.width)).Append('x').Append(Mathf.Round(rect.rect.height));
            }

            UnityEngine.UI.LayoutElement layoutElement = transform.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement)
            {
                sb.Append("  LE(min=").Append(layoutElement.minHeight)
                  .Append(",pref=").Append(layoutElement.preferredHeight)
                  .Append(",flex=").Append(layoutElement.flexibleHeight)
                  .Append(",ignore=").Append(layoutElement.ignoreLayout).Append(')');
            }

            Component[] components = transform.GetComponents<Component>();
            if (components.Length > 0)
            {
                sb.Append("  [");
                for (int i = 0; i < components.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(components[i] ? components[i].GetType().Name : "null");
                }

                sb.Append(']');
            }

            sb.Append('\n');

            for (int i = 0; i < transform.childCount; i++)
            {
                Append(sb, transform.GetChild(i), depth + 1);
            }
        }
    }
}
