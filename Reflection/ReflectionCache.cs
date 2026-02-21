using System;
using System.Reflection;

namespace TownOfUsStatsExporter.Reflection;

/// <summary>
/// Cache for reflection metadata to improve performance.
/// Reflection is ~100x slower than direct access, so caching is essential.
/// </summary>
internal class ReflectionCache
{
    /// <summary>
    /// Gets or sets the EndGamePatches type.
    /// </summary>
    public Type? EndGamePatchesType { get; set; }

    /// <summary>
    /// Gets or sets the EndGameData type.
    /// </summary>
    public Type? EndGameDataType { get; set; }

    /// <summary>
    /// Gets or sets the PlayerRecord type.
    /// </summary>
    public Type? PlayerRecordType { get; set; }

    /// <summary>
    /// Gets or sets the GameHistory type.
    /// </summary>
    public Type? GameHistoryType { get; set; }

    /// <summary>
    /// Gets or sets the PlayerRecords property.
    /// </summary>
    public PropertyInfo? PlayerRecordsProperty { get; set; }

    /// <summary>
    /// Gets or sets the PlayerStats field.
    /// </summary>
    public FieldInfo? PlayerStatsField { get; set; }

    /// <summary>
    /// Gets or sets the RoleHistory field.
    /// </summary>
    public FieldInfo? RoleHistoryField { get; set; }

    /// <summary>
    /// Gets or sets the KilledPlayers field.
    /// </summary>
    public FieldInfo? KilledPlayersField { get; set; }

    /// <summary>
    /// Gets or sets the WinningFaction field.
    /// </summary>
    public FieldInfo? WinningFactionField { get; set; }

    /// <summary>
    /// Gets or sets the GetRoleName method.
    /// </summary>
    public MethodInfo? GetRoleNameMethod { get; set; }
}
