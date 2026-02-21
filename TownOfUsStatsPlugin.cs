using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using System;
using System.Reflection;

namespace TownOfUsStatsExporter;

/// <summary>
/// Main BepInEx plugin for TownOfUs Stats Exporter.
/// This is a standalone plugin that uses reflection to access TOU Mira data
/// and exports game statistics to a cloud API.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("auavengers.tou.mira", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(ReactorPlugin.Id, BepInDependency.DependencyFlags.HardDependency)]
public class TownOfUsStatsPlugin : BasePlugin
{
    /// <summary>
    /// Plugin GUID for BepInEx identification.
    /// </summary>
    public const string PluginGuid = "com.townofus.stats.exporter";

    /// <summary>
    /// Plugin display name.
    /// </summary>
    public const string PluginName = "TownOfUs Stats Exporter";

    /// <summary>
    /// Plugin version.
    /// </summary>
    public const string PluginVersion = "1.0.0";

    /// <summary>
    /// Logger instance for the plugin.
    /// </summary>
    internal static ManualLogSource Logger { get; private set; } = null!;

    /// <summary>
    /// Harmony instance for patching.
    /// </summary>
    internal static Harmony Harmony { get; private set; } = null!;

    private TownOfUsStatsExporter.Reflection.TouMiraReflectionBridge? reflectionBridge;

    /// <summary>
    /// Called when the plugin is loaded by BepInEx.
    /// </summary>
    public override void Load()
    {
        Logger = Log;
        Harmony = new Harmony(PluginGuid);

        Logger.LogInfo("========================================");
        Logger.LogInfo($"{PluginName} v{PluginVersion}");
        Logger.LogInfo("========================================");

        // Initialize reflection bridge
        reflectionBridge = new TownOfUsStatsExporter.Reflection.TouMiraReflectionBridge();

        if (!reflectionBridge.Initialize())
        {
            Logger.LogError("Failed to initialize TOU Mira reflection bridge!");
            Logger.LogError("This plugin may not be compatible with your TOU Mira version.");
            Logger.LogError("Plugin will be disabled.");
            return;
        }

        Logger.LogInfo($"Successfully connected to TOU Mira v{reflectionBridge.TouMiraVersion}");
        Logger.LogInfo($"Compatibility: {reflectionBridge.CompatibilityStatus}");

        // Store bridge in static context for patches
        ReflectionBridgeProvider.SetBridge(reflectionBridge);

        // Apply Harmony patches
        try
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo("Harmony patches applied successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply Harmony patches: {ex}");
            return;
        }

        Logger.LogInfo($"{PluginName} loaded successfully!");
        Logger.LogInfo("Stats will be exported at the end of each game.");
    }

    /// <summary>
    /// Called when the plugin is unloaded.
    /// </summary>
    /// <returns>True if unloading was successful.</returns>
    public override bool Unload()
    {
        Logger.LogInfo($"Unloading {PluginName}...");
        Harmony?.UnpatchSelf();
        return true;
    }
}

/// <summary>
/// Static provider for accessing reflection bridge from patches.
/// </summary>
internal static class ReflectionBridgeProvider
{
    private static TownOfUsStatsExporter.Reflection.TouMiraReflectionBridge? bridge;

    /// <summary>
    /// Sets the reflection bridge instance.
    /// </summary>
    /// <param name="b">The bridge instance.</param>
    public static void SetBridge(TownOfUsStatsExporter.Reflection.TouMiraReflectionBridge b) => bridge = b;

    /// <summary>
    /// Gets the reflection bridge instance.
    /// </summary>
    /// <returns>The bridge instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if bridge is not initialized.</exception>
    public static TownOfUsStatsExporter.Reflection.TouMiraReflectionBridge GetBridge() =>
        bridge ?? throw new InvalidOperationException("Bridge not initialized");
}
