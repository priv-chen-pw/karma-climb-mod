using RoR2;

namespace KarmaClimb
{
    /// <summary>
    /// Holds Karma at its starting level for the whole of an Eclipse run.
    ///
    /// Karma is set once when the run starts (<c>KarmaManager.Start</c>, from the Karma.InitialLevel
    /// rule EclipseRun forces) and afterwards only ever rises through <c>KarmaManager.ModifyKarma</c>,
    /// which the Karma triggers call as they complete. Nothing else writes it - <c>SyncKarma</c> only
    /// mirrors the server to clients - so declining ModifyKarma during Eclipse runs fixes Karma at its
    /// starting level. Every other game mode keeps vanilla Karma.
    /// </summary>
    internal static class EclipseKarmaLock
    {
        internal static void Init()
        {
            On.RoR2.KarmaManager.ModifyKarma += (orig, self, amount) =>
            {
                if (Run.instance is EclipseRun)
                {
                    return;
                }

                orig(self, amount);
            };
        }
    }
}
