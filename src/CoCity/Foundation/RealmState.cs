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
        IReadOnlyList<OutputMetric> Output,
        IndustryBaseOutputRates BaseOutputPerWorker);

    public sealed record IndustryAllocation(
        MortalIndustryType Industry,
        int WorkforceShare);

    public enum MortalIndustryType
    {
        Agriculture,
        Handicrafts,
        Commerce
    }

    public enum RecruitmentWageLevel
    {
        Frugal,
        Standard,
        Generous
    }

    /// <summary>Base output per worker for each industry type.</summary>
    public sealed record IndustryBaseOutputRates(
        int AgriculturePerWorker,
        int HandicraftsPerWorker,
        int CommercePerWorker);

    public sealed record SectState(
        string Id,
        string RegionId,
        string Name,
        decimal Funds,
        int Population,
        int Loyalty,
        MortalIndustryType? IndustryPreference,
        RecruitmentWageLevel RecruitmentWage,
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
        IReadOnlyList<SectSimulationState> Sects,
        int TurnNumber);

    public sealed record SectSimulationState(
        string SectId,
        string SectName,
        string RegionId,
        decimal CurrentFunds,
        int CurrentPopulation,
        int Loyalty,
        MortalIndustryType? IndustryPreference,
        RecruitmentWageLevel RecruitmentWage,
        IReadOnlyList<OutputMetric> CurrentOutput,
        int RecruitablesFromRegion,
        int LastRecruitsGained,
        decimal LastWagesPaid);

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
        RecruitmentWageLevel RecruitmentWage,
        int RecruitsGathered,
        decimal WagesPaid,
        decimal FundsRemaining,
        string OutcomeSummary);

    public sealed record SectOperationsTurnReport(
        IReadOnlyList<SectOperationEvent> SectEvents,
        SectPurchaseReport PurchaseReport);

    public sealed record SectOperationEvent(
        string SectId,
        string SectName,
        MortalIndustryType InputIndustry,
        int RequestedUnits,
        int PurchasedUnits,
        decimal UpkeepPaid,
        decimal InputPurchaseCost,
        decimal FundsBefore,
        decimal FundsAfter,
        decimal OutputFactor,
        string OperationSummary);

    // ─────────────────────────────────────────────────────────────────────────
    // Industry simulation model
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Per-industry output amounts.</summary>
    public sealed record IndustryOutput(
        int AgricultureUnits,
        int HandicraftsUnits,
        int CommerceUnits);

    /// <summary>Per-town industry simulation state for a single turn.</summary>
    public sealed record MortalTownIndustryState(
        string TownId,
        string TownName,
        LaborForceDistribution LaborForce,
        IndustryOutput GrossOutput,
        decimal GovernmentEfficiency,
        IndustryOutput NetOutput,
        IndustryOutput PurchasableSurplus);

    /// <summary>Labor force distributed across industry types.</summary>
    public sealed record LaborForceDistribution(
        int Agriculture,
        int Handicrafts,
        int Commerce);

    /// <summary>Reports industry production for a single town per turn.</summary>
    public sealed record TownIndustryEvent(
        string TownId,
        string TownName,
        LaborForceDistribution LaborForce,
        IndustryOutput GrossOutput,
        decimal GovernmentEfficiency,
        IndustryOutput NetOutput,
        IndustryOutput PurchasableSurplus);

    /// <summary>Reports all industry events for a turn.</summary>
    public sealed record IndustryTurnReport(
        IReadOnlyList<TownIndustryEvent> TownIndustryEvents);

    /// <summary>Explicit request for a sect to buy a town's industrial output.</summary>
    public sealed record SectPurchaseRequest(
        string SectId,
        string TownId,
        MortalIndustryType Industry,
        int Quantity,
        decimal UnitPrice);

    /// <summary>Outcome of a single sect purchase request.</summary>
    public sealed record SectPurchaseReceipt(
        string SectId,
        string SectName,
        string TownId,
        string TownName,
        MortalIndustryType Industry,
        int RequestedQuantity,
        int PurchasedQuantity,
        decimal UnitPrice,
        decimal FundsSpent,
        decimal FundsRemaining,
        string Resolution);

    /// <summary>Reports all processed sect purchase requests.</summary>
    public sealed record SectPurchaseReport(
        IReadOnlyList<SectPurchaseReceipt> Receipts);

    /// <summary>Aggregated financial result of a purchase batch for one sect.</summary>
    public sealed record SectPurchaseSettlement(
        string SectId,
        string SectName,
        decimal StartingFunds,
        decimal FundsSpent,
        decimal FundsRemaining);

    // ─────────────────────────────────────────────────────────────────────────
    // Taxation simulation model
    // ─────────────────────────────────────────────────────────────────────────

    public enum TaxRateLevel
    {
        Light,
        Standard,
        Heavy
    }

    public sealed record RealmTaxationState(
        decimal CurrentTreasuryFunds,
        decimal LastCollectedRevenue,
        TaxRateLevel SelectedTaxRate,
        decimal ProjectedRevenue,
        IReadOnlyList<TownTaxationState> Towns);

    public sealed record TownTaxationState(
        string TownId,
        string TownName,
        decimal GrossTaxBase,
        decimal CollectedRevenue,
        int StabilityDelta,
        string StabilitySummary);

    public sealed record TaxationTurnReport(
        int TurnNumber,
        TaxRateLevel SelectedTaxRate,
        decimal TreasuryBeforeCollection,
        decimal CollectedRevenue,
        decimal TreasuryAfterCollection,
        IReadOnlyList<TownTaxationEvent> TownEvents);

    public sealed record TownTaxationEvent(
        string TownId,
        string TownName,
        decimal GrossTaxBase,
        decimal CollectedRevenue,
        int StabilityDelta,
        string StabilitySummary);
}
