using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

using RetakesPlugin.Configs;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Commands.Player;

public class VoicesCommand
{
    private readonly HashSet<CCSPlayerController> _hasMutedVoices;
    private readonly BaseConfigs _config;
    private readonly RetakesPlugin _plugin;

    public VoicesCommand(RetakesPlugin plugin, BaseConfigs config, HashSet<CCSPlayerController> hasMutedVoices)
    {
        _plugin = plugin;
        _config = config;
        _hasMutedVoices = hasMutedVoices;
    }

    public void OnCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.command.player_only")}");
            return;
        }

        if (!_config.MapConfig.EnableBombsiteAnnouncementVoices)
        {
            commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.voices.server_disabled")}");
            return;
        }

        var didMute = false;
        if (!_hasMutedVoices.Contains(player))
        {
            didMute = true;
            _hasMutedVoices.Add(player);
        }
        else
        {
            _hasMutedVoices.Remove(player);
        }

        var statusText = didMute ? $"{_plugin.Translate(player, "retakes.enabled")}" : $"{_plugin.Translate(player, "retakes.disabled")}";

        commandInfo.ReplyToCommand($"{_plugin.Translate(player, "retakes.prefix")} {_plugin.Translate(player, "retakes.voices.toggle", statusText)}");

        Logger.LogInfo("Commands", $"{player.PlayerName} {(didMute ? "muted" : "unmuted")} voice announcements");
    }
}