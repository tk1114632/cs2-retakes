using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

namespace RetakesPlugin.Utils;

public static class LocalizationExtensions
{
    public static string Translate(this RetakesPlugin plugin, CCSPlayerController? player,
        string key, params object[] args) => plugin.Localizer.ForPlayer(player, key, args);

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
