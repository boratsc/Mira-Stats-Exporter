namespace TownOfUsStatsExporter.Models;

/// <summary>
/// DTO for player record data extracted via reflection from TOU Mira.
/// </summary>
public class PlayerRecordData
{
    /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role string representation.
    /// </summary>
    public string RoleString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the player won.
    /// </summary>
    public bool Winner { get; set; }

    /// <summary>
    /// Gets or sets the player ID.
    /// </summary>
    public byte PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the team string representation.
    /// </summary>
    public string TeamString { get; set; } = string.Empty;
}

/// <summary>
/// DTO for player stats data extracted via reflection from TOU Mira.
/// </summary>
public class PlayerStatsData
{
    /// <summary>
    /// Gets or sets the number of correct kills.
    /// </summary>
    public int CorrectKills { get; set; }

    /// <summary>
    /// Gets or sets the number of incorrect kills.
    /// </summary>
    public int IncorrectKills { get; set; }

    /// <summary>
    /// Gets or sets the number of correct assassin kills.
    /// </summary>
    public int CorrectAssassinKills { get; set; }

    /// <summary>
    /// Gets or sets the number of incorrect assassin kills.
    /// </summary>
    public int IncorrectAssassinKills { get; set; }
}

/// <summary>
/// DTO for killed player data.
/// </summary>
public class KilledPlayerData
{
    /// <summary>
    /// Gets or sets the killer's player ID.
    /// </summary>
    public byte KillerId { get; set; }

    /// <summary>
    /// Gets or sets the victim's player ID.
    /// </summary>
    public byte VictimId { get; set; }
}
