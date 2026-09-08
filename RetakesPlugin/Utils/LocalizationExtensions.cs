using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

namespace RetakesPlugin.Utils;

public static class LocalizationExtensions
{
    // GOTV proxies, bots and connections that are not authorized yet have a SteamID of 0, which
    // ForPlayer cannot convert. Those get the server language instead of throwing.
    public static string Translate(this RetakesPlugin plugin, CCSPlayerController? player,
        string key, params object[] args) => player is { IsValid: true, SteamID: not 0 }
        ? plugin.Localizer.ForPlayer(player, key, args)
        : plugin.Localizer[key, args].Value;

    public static void PrintLocalizedChat(this RetakesPlugin plugin, CCSPlayerController player,
        string key, params object[] args)
    {
        player.PrintToChat($"{plugin.Translate(player, "retakes.prefix")} {plugin.Translate(player, key, args)}");
    }

    public static void PrintLocalizedChatAll(this RetakesPlugin plugin, string key, params object[] args)
    {
        foreach (var player in Utilities.GetPlayers().Where(PlayerHelper.IsValid))
        {
            plugin.PrintLocalizedChat(player, key, args);
        }
    }
}
