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
        int FoodConsumptionPerCapita,
        int FoodProduction,
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

    // ─────────────────────────────────────────────────────────────────────────
    // Simulation model (mutable turn-by-turn state)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Immutable snapshot of all mortal-realm simulation state for a single turn.</summary>
    public sealed record MortalRealmState(
        IReadOnlyList<MortalTownSimulationState> Towns,
        IReadOnlyList<SectRecruitmentSimulationState> Sects,
        int TurnNumber);

    public sealed record SectRecruitmentSimulationState(
        string SectId,
        string SectName,
        string RegionId,
        int RecruitablesFromRegion);

    /// <summary>Per-town simulation state.</summary>
    public sealed record MortalTownSimulationState(
        string TownId,
        string TownName,
        int CurrentPopulation,
        int FoodBalance,
        int RecruitmentPool,
        int PopulationChange,
        int RecruitsLostLastTurn,
        string ChangeReason);

    /// <summary>Reports every side effect produced by a turn step.</summary>
    public sealed record TurnReport(
        int TurnNumber,
        IReadOnlyList<TownTurnEvent> TownEvents,
        IReadOnlyList<SectRecruitmentEvent> RecruitmentEvents);

    public sealed record TownTurnEvent(
        string TownId,
        string TownName,
        int PreviousPopulation,
        int CurrentPopulation,
        int FoodBalance,
        int RecruitsLost,
        string ChangeReason);

    public sealed record SectRecruitmentEvent(
        string SectId,
        string SectName,
        string RegionId,
        int RecruitsGathered);
}
