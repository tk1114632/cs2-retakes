using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using RetakesPlugin.Utils;
using RetakesPlugin.Models;
using RetakesPlugin.Services;
using RetakesPlugin.Managers;
using RetakesPluginShared.Enums;

namespace RetakesPlugin.Commands.SpawnEditor;

public class AddSpawnCommand
{
    private readonly RetakesPlugin _plugin;
    private readonly ShowSpawnsCommand _showSpawnsCommand;

    public AddSpawnCommand(RetakesPlugin plugin, ShowSpawnsCommand showSpawnsCommand)
    {
        _plugin = plugin;
        _showSpawnsCommand = showSpawnsCommand;
    }

    public void OnCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player))
        {
            return;
        }

        var commandName = commandInfo.GetArg(0);
        var requiredPermission = PlayerHelper.GetCommandPermission(_plugin.Config, commandName, "SpawnEditor");
        if (!AdminManager.PlayerHasPermissions(player, requiredPermission))
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.no_permissions")}");
            return;
        }

        if (_showSpawnsCommand.ShowingSpawnsForBombsite == null)
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.spawn.show_before_add")}");
            return;
        }

        if (!PlayerHelper.HasAlivePawn(player))
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.spawn.alive_required")}");
            return;
        }

        if (commandInfo.ArgCount < 2)
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.command.usage", "!add [T/CT] [Y/N can be planter]")}");
            return;
        }

        var team = commandInfo.GetArg(1).ToUpper();
        if (team != "T" && team != "CT")
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.command.team_required")}");
            return;
        }

        var canBePlanterInput = commandInfo.GetArg(2).ToUpper();
        if (!string.IsNullOrWhiteSpace(canBePlanterInput) && canBePlanterInput != "Y" && canBePlanterInput != "N")
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.spawn.planter_invalid")}");
            return;
        }

        if (_plugin.SpawnManager == null || _plugin.MapConfigService == null)
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.command.not_initialized")}");
            return;
        }

        var spawns = _plugin.SpawnManager.GetSpawns((Bombsite)_showSpawnsCommand.ShowingSpawnsForBombsite);
        var closestDistance = 9999.9;

        foreach (var spawn in spawns)
        {
            var distance = GameRulesHelper.GetDistanceBetweenVectors(spawn.Vector, player!.PlayerPawn.Value!.AbsOrigin!);

            if (distance > 128.0 || distance > closestDistance)
            {
                continue;
            }

            closestDistance = distance;
        }

        if (closestDistance <= 72)
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.spawn.too_close")}");
            return;
        }

        var newSpawn = new Spawn(
            vector: player!.PlayerPawn.Value!.AbsOrigin!,
            qAngle: player!.PlayerPawn.Value!.AbsRotation!
        )
        {
            Team = team == "T" ? CsTeam.Terrorist : CsTeam.CounterTerrorist,
            CanBePlanter = team == "T" && !string.IsNullOrWhiteSpace(canBePlanterInput) ? canBePlanterInput == "Y" : player.PlayerPawn.Value.InBombZoneTrigger,
            Bombsite = (Bombsite)_showSpawnsCommand.ShowingSpawnsForBombsite
        };

        SpawnService.ShowSpawn(newSpawn);

        var didAddSpawn = _plugin.MapConfigService.AddSpawn(newSpawn);
        if (didAddSpawn)
        {
            _plugin.SpawnManager.CalculateMapSpawns();
        }

        commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, didAddSpawn ? "retakes.spawn.added" : "retakes.spawn.add_failed")}");

        if (didAddSpawn)
        {
            Logger.LogInfo("Commands", $"{player.PlayerName} added spawn at bombsite {_showSpawnsCommand.ShowingSpawnsForBombsite}");
        }
    }
}