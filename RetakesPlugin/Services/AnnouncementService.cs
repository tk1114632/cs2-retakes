using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using RetakesPlugin.Utils;
using RetakesPluginShared.Enums;

namespace RetakesPlugin.Services;

public class AnnouncementService
{
    private readonly RetakesPlugin _plugin;
    private readonly Random _random;
    private readonly HashSet<CCSPlayerController> _hasMutedVoices;
    private readonly bool _voicesEnabled;
    private readonly bool _centerEnabled;

    private static readonly string[] BombsiteAnnouncers =
    [
        "balkan_epic",
        "leet_epic",
        "professional_epic",
        "professional_fem",
        "seal_epic",
        "swat_epic",
        "swat_fem"
    ];

    public AnnouncementService(RetakesPlugin plugin, Random random, HashSet<CCSPlayerController> hasMutedVoices, bool voicesEnabled, bool centerEnabled)
    {
        _plugin = plugin;
        _random = random;
        _hasMutedVoices = hasMutedVoices;
        _voicesEnabled = voicesEnabled;
        _centerEnabled = centerEnabled;
    }

    public void AnnounceBombsite(Bombsite bombsite, bool onlyCenter = false)
    {
        var numTerrorist = PlayerHelper.GetPlayerCount(CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist);
        var numCounterTerrorist = PlayerHelper.GetPlayerCount(CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist);


        foreach (var player in Utilities.GetPlayers())
        {
            if (!onlyCenter)
            {
                _plugin.PrintLocalizedChat(player, "retakes.bombsite.announcement", bombsite.ToString(), numTerrorist, numCounterTerrorist);

                if (_voicesEnabled && !_hasMutedVoices.Contains(player))
                {
                    var bombsiteAnnouncer = BombsiteAnnouncers[_random.Next(BombsiteAnnouncers.Length)];
                    player.ExecuteClientCommand($"play sounds/vo/agents/{bombsiteAnnouncer}/loc_{bombsite.ToString().ToLower()}_01");
                }

                continue;
            }

            if (!_centerEnabled)
            {
                continue;
            }

            if (player.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist)
            {
                player.PrintToCenter(_plugin.Translate(player, "retakes.center.bombsite.announcement", bombsite.ToString(), numTerrorist, numCounterTerrorist));
            }
        }

        Logger.LogInfo("Announcement", $"Announced bombsite {bombsite} ({numTerrorist}T vs {numCounterTerrorist}CT)");
    }
}