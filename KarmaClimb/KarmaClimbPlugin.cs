using BepInEx;
using R2API.Utils;

namespace KarmaClimb
{
    /// <summary>
    /// Extends Eclipse from 8 levels to 16. Levels 9-16 carry the full Eclipse 8 modifier set plus
    /// a forced starting Karma of 1-8; levels 1-8 are forced to Karma disabled, so Karma is climbed
    /// exactly once across the ladder rather than twice.
    /// </summary>
    // Every player needs this mod, on the same version: it adds difficulty choices and unlockables, and
    // the game sends both over the network as indices, so a mismatch would silently desync the lobby.
    // R2API turns this into the game's mod list check, which keeps mismatched players apart.
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.DifficultyAPI.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class KarmaClimbPlugin : BaseUnityPlugin
    {
        public const string PluginAuthor = "chen-ror2";
        public const string PluginName = "KarmaClimb";
        public const string PluginGuid = PluginAuthor + "." + PluginName;
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);

            // Order matters: difficulties have to be registered before the difficulty catalog
            // initialises, and their language tokens before R2API reads nameToken to build the
            // difficulty rule choices.
            KarmaClimbLanguage.Register();
            KarmaDescriptions.Register();
            KarmaClimbDifficulties.Register();
            KarmaClimbUnlockables.Init();
            EclipseLadderHooks.Init();
            EclipseKarmaLock.Init();

            // Eclipse menu presentation.
            EclipseMenuMedals.Init();
            EclipseMenuKarmaPanel.Init();
            EclipseMenuLayout.Init(Config.Bind(
                "Eclipse menu",
                "RightPanelExtraWidth",
                260f,
                "Pixels to widen the Eclipse menu's right-hand column by, growing leftwards. 0 disables.").Value);
            EclipseMenuEscape.Init();
            EclipseMenuDump.Init(Config.Bind(
                "Debug",
                "LogEclipseMenuHierarchy",
                false,
                "Logs the Eclipse menu's transform tree once, to position the Karma section against the real hierarchy. Turn off when the layout is settled.").Value);

            Log.Info($"{PluginName} {PluginVersion} ready: Eclipse now climbs to {KarmaClimbLadder.MaxLevel}.");
        }
    }
}
