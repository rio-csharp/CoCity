namespace CoCity.Foundation
{
    public sealed record RealmState(
        string RealmName,
        TreasuryState Treasury,
        IReadOnlyList<RegionState> Regions,
        IReadOnlyList<MortalTownState> Towns,
        IReadOnlyList<SectState> Sects,
        IReadOnlyList<MinistryState> Ministries);

    public sealed record TreasuryState(
        decimal Funds,
        decimal BaselineTaxIncome);

    public sealed record RegionState(
        string Id,
        string Name,
        int SpiritualVeinStrength,
        RegionBaselineAttributes Baseline,
        IReadOnlyList<string> TownIds,
        IReadOnlyList<string> SectIds);

    public sealed record RegionBaselineAttributes(
        string Terrain,
        string Climate,
        string SettlementPattern);

    public sealed record MortalTownState(
        string Id,
        string RegionId,
        string Name,
        int Population,
        IReadOnlyList<IndustryAllocation> Industries,
        IReadOnlyList<OutputMetric> Output);

    public sealed record IndustryAllocation(
        MortalIndustryType Industry,
        int WorkforceShare);

    public enum MortalIndustryType
    {
        Agriculture,
        Handicrafts,
        Commerce
    }

    public sealed record SectState(
        string Id,
        string RegionId,
        string Name,
        decimal Funds,
        int Population,
        IReadOnlyList<OutputMetric> Output);

    public sealed record OutputMetric(
        string Label,
        decimal Amount,
        string Unit);

    public sealed record MinistryState(
        string Id,
        string Name,
        MinistryAuthorityProfile Authority,
        HandlingStandardProfile Standard,
        OfficialState Minister,
        IReadOnlyList<OfficialState> SupportingOfficials);

    public sealed record MinistryAuthorityProfile(
        string DelegationLevel,
        string EscalationRule,
        IReadOnlyList<string> Responsibilities);

    public sealed record HandlingStandardProfile(
        string Name,
        string Summary);

    public sealed record OfficialState(
        string Id,
        string Name,
        string Role,
        OfficialRatings Ratings);

    public sealed record OfficialRatings(
        int Administration,
        int Integrity,
        int Loyalty);
}
