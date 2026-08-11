using CounterStrikeSharp.API.Core;

using RetakesPlugin.Utils;

namespace RetakesPlugin.Modules;

/// <summary>
/// Lets a server enable friendly fire for utility only.
///
/// The game offers no middle ground of its own: mp_friendlyfire 0 zeroes teammate damage in an
/// early return that runs before the ff_damage_reduction_* multipliers, so turning it off to stop
/// teammates shooting each other on a public server also removes HE, molotov and fire damage
/// between teammates. Running mp_friendlyfire 1 and dropping only DMG_BULLET here restores the
/// competitive behaviour of utility while keeping guns harmless.
///
/// Everything this feature owns lives in this file. The rest of the plugin only holds three
/// one-line anchors, so that merging upstream changes stays cheap:
///   RetakesPlugin.Load                  -> Register(this)
///   PlayerEventHandlers.OnPlayerDeath   -> IsTeamKill(...)
///   ServerHelper's retakes.cfg template -> ConfigConVars
///
/// Requires CounterStrikeSharp 1.0.367, which is why RetakesPlugin declares MinimumApiVersion 367:
/// the damage listeners landed in 1.0.352 and the CBaseEntity_TakeDamageOld linux signature they
/// rely on was only fixed in 1.0.367.
/// </summary>
public static class FriendlyFire
{
    /// <summary>
    /// Convar block appended to the generated retakes.cfg. mp_friendlyfire itself stays at 0 in the
    /// template, so flipping that single convar is the whole opt-in.
    ///
    /// The reduction values are the Valve competitive defaults. ff_damage_bullet_penetration is
    /// deliberately left alone: the game only applies its "teammates block bullets" special case
    /// when ff_damage_reduction_bullets is exactly 0, so keeping 0.33 preserves normal competitive
    /// penetration through teammates even though this module makes those bullets harmless.
    /// </summary>
    public const string ConfigConVars =
"""
                // Set mp_friendlyfire to 1 above to enable friendly fire for utility
                // (HE, molotov, knife, zeus). Bullet damage between teammates is blocked
                // by the plugin, so guns stay harmless either way.
                // These are the Valve competitive damage reduction values.
                ff_damage_reduction_bullets 0.33
                ff_damage_reduction_grenade 0.85
                ff_damage_reduction_grenade_self 1
                ff_damage_reduction_other 0.4
                mp_tkpunish 0
""";

    public static void Register(BasePlugin plugin)
    {
        plugin.RegisterListener<Listeners.OnPlayerTakeDamagePre>(OnPlayerTakeDamagePre);
    }

    /// <summary>
    /// True when the attacker hit someone on their own team. Utility can kill teammates once
    /// mp_friendlyfire is 1, and those kills must not count towards the retakes scoreboard or the
    /// queue ranking. Self inflicted deaths are not team kills, so they keep their existing
    /// behaviour.
    /// </summary>
    public static bool IsTeamKill(CCSPlayerController attacker, CCSPlayerController? victim)
    {
        return PlayerHelper.IsValid(victim) && attacker.Handle != victim.Handle && attacker.Team == victim.Team;
    }

    // Only DMG_BULLET is filtered - the knife is DMG_SLASH, the zeus is DMG_SHOCK, grenades are
    // DMG_BLAST and fire is DMG_BURN, so all of those keep flowing through the game's own
    // ff_damage_reduction_* scaling. No-op while mp_friendlyfire is 0.
    private static HookResult OnPlayerTakeDamagePre(CCSPlayerPawn playerPawn, CTakeDamageInfo info)
    {
        if ((info.BitsDamageType & DamageTypes_t.DMG_BULLET) == 0)
        {
            return HookResult.Continue;
        }

        var attacker = info.Attacker.Value;

        if (attacker == null || attacker.Handle == playerPawn.Handle || attacker.TeamNum != playerPawn.TeamNum)
        {
            return HookResult.Continue;
        }

        // Handled skips the whole damage application, which also avoids the armour loss, the
        // tagging and the "attacked a teammate" chat spam that a 0 damage value leaves behind.
        return HookResult.Handled;
    }
}
