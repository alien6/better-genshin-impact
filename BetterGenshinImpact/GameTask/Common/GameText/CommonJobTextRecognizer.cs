using System;
using System.Collections.Generic;
using System.Linq;
using BetterGenshinImpact.GameTask.Localization;

namespace BetterGenshinImpact.GameTask.Common.GameText;

public sealed class CommonJobTextRecognizer
{
    private readonly IGameTextMatcher _matcher;

    public CommonJobTextRecognizer(IGameTextMatcher matcher)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
    }

    public string CraftingSearchText => FirstAlias(GameTextKeys.Common.Crafting);

    public string KatheryneSearchText => FirstAlias(GameTextKeys.AdventurersGuild.Katheryne);

    public string DailyCommissionsSearchText => FirstAlias(GameTextKeys.AdventurersGuild.DailyCommissions);

    public string ExpeditionSearchText => FirstAlias(GameTextKeys.Expedition.Entry);

    public string TeapotSpiritSearchText => FirstAlias(GameTextKeys.SereniteaPot.Spirit);

    public string TrustRankSearchText => FirstAlias(GameTextKeys.SereniteaPot.TrustRank);

    public string RealmDepotSearchText => FirstAlias(GameTextKeys.SereniteaPot.RealmDepot);

    public bool IsRevive(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Common.Revive);

    public bool IsCrafting(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Common.Crafting);

    public bool IsKatheryne(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.AdventurersGuild.Katheryne);

    public bool IsDailyCommissions(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.AdventurersGuild.DailyCommissions);

    public bool IsExpedition(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Expedition.Entry);

    public bool IsClaim(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Common.Claim);

    public bool IsAll(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.Common.All);

    public bool IsClaimAll(IEnumerable<string> recognizedTexts)
    {
        ArgumentNullException.ThrowIfNull(recognizedTexts);
        var textSnapshot = recognizedTexts as IReadOnlyCollection<string> ?? recognizedTexts.ToArray();
        return _matcher.IsCombinedMatch(textSnapshot, GameTextKeys.Common.Claim)
               && _matcher.IsCombinedMatch(textSnapshot, GameTextKeys.Common.All);
    }

    public bool IsDailyRewardClaimed(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.AdventureHandbook.DailyRewardClaimed);

    public bool IsTeapotSpirit(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.SereniteaPot.Spirit);

    public bool IsTrustRank(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.SereniteaPot.TrustRank);

    public bool IsRealmDepot(string recognizedText) =>
        _matcher.IsMatch(recognizedText, GameTextKeys.SereniteaPot.RealmDepot);

    private string FirstAlias(string key) => _matcher.GetAliases(key)[0];
}
