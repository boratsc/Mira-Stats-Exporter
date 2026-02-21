using HarmonyLib;
using System;
using System.Threading.Tasks;

namespace TownOfUsStatsExporter.Patches;

/// <summary>
/// Patch on AmongUsClient.OnGameEnd to trigger stats export.
/// Uses Low priority to execute AFTER TOU Mira's BuildEndGameData() patch.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class EndGameExportPatch
{
    /// <summary>
    /// Postfix patch - runs after TOU Mira's BuildEndGameData() has populated EndGameData.PlayerRecords.
    /// </summary>
    /// <param name="__instance">The AmongUsClient instance.</param>
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    public static void Postfix(AmongUsClient __instance)
    {
        try
        {
            TownOfUsStatsPlugin.Logger.LogInfo("=== End Game Export Patch Triggered ===");

            // Check if this is Hide & Seek mode (skip export)
            if (GameOptionsManager.Instance?.CurrentGameOptions?.GameMode == AmongUs.GameOptions.GameModes.HideNSeek)
            {
                TownOfUsStatsPlugin.Logger.LogInfo("Hide & Seek mode detected - skipping export");
                return;
            }

            // Fire-and-forget async export (don't block UI)
            _ = Task.Run(async () =>
            {
                try
                {
                    await Export.StatsExporter.ExportGameStatsAsync();
                }
                catch (Exception ex)
                {
                    TownOfUsStatsPlugin.Logger.LogError($"Unhandled error in stats export: {ex}");
                }
            });

            TownOfUsStatsPlugin.Logger.LogInfo("Stats export task started in background");
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error in EndGameExportPatch: {ex}");
        }
    }
}
