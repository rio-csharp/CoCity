using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.ViewModels
{
    public sealed class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly RealmState _foundation;
        private readonly IMortalRealmSimulationService _simulationService;
        private readonly IMortalIndustrySimulationService _industryService;
        private readonly ISectAutonomousOperationsService _sectOperationsService;
        private readonly IBuildingSystemService _buildingSystemService;
        private readonly IMortalTaxationSimulationService _taxationService;
        private readonly IMinistryFrameworkService _ministryFrameworkService;
        private readonly IReadOnlyDictionary<string, string> _regionNamesById;
        private readonly IReadOnlyDictionary<string, MortalTownState> _townsById;
        private MortalRealmState _simulationState;
        private RealmBuildingState _buildingState;
        private IReadOnlyList<MortalTownIndustryState> _industryStates;
        private RealmTaxationState _taxationState;
        private RealmMinistryState _ministryState;
        private TurnReport? _lastReport;
        private SectOperationsTurnReport? _lastSectOperationsReport;
        private BuildingReport? _lastBuildingReport;
        private TaxationTurnReport? _lastTaxationReport;
        private MinistryTurnReport? _lastMinistryReport;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel(
            ICoreDataFoundationService foundationService,
            IMortalRealmSimulationService simulationService,
            IMortalIndustrySimulationService industryService,
            ISectAutonomousOperationsService sectOperationsService,
            IBuildingSystemService buildingSystemService,
            IMortalTaxationSimulationService taxationService,
            IMinistryFrameworkService ministryFrameworkService)
        {
            _simulationService = simulationService;
            _industryService = industryService;
            _sectOperationsService = sectOperationsService;
            _buildingSystemService = buildingSystemService;
            _taxationService = taxationService;
            _ministryFrameworkService = ministryFrameworkService;
            _foundation = foundationService.GetInitialState();
            _regionNamesById = _foundation.Regions.ToDictionary(region => region.Id, region => region.Name);
            _townsById = _foundation.Towns.ToDictionary(town => town.Id);
            _simulationState = _simulationService.Initialize(_foundation);
            _buildingState = _buildingSystemService.Initialize(_foundation);
            _industryStates = _industryService.Initialize(_foundation, _foundation.Ministries);
            _taxationState = _taxationService.Initialize(_foundation, _simulationState, _industryStates);
            _ministryState = _ministryFrameworkService.Initialize(_foundation, _simulationState, _buildingState, _taxationState);

            AdvanceTurnCommand = new Microsoft.Maui.Controls.Command(ExecuteAdvanceTurn);
            ConstructSectInfrastructureCommand = new Microsoft.Maui.Controls.Command(ExecuteConstructSectInfrastructure);
            ConstructMortalInfrastructureCommand = new Microsoft.Maui.Controls.Command(ExecuteConstructMortalInfrastructure);
            IncreaseTaxRateCommand = new Microsoft.Maui.Controls.Command(ExecuteIncreaseTaxRate);
            DecreaseTaxRateCommand = new Microsoft.Maui.Controls.Command(ExecuteDecreaseTaxRate);
            BuildDisplayState();
        }

        public string PageTitle => "Prototype Closed Loop Dashboard";
        public string PageSubtitle => "Task 1.10 adds a runtime ministry framework with cases, staffing capacity, and visible ministry reports.";
        public string RealmSummary => $"{_foundation.RealmName} — Turn {SimulationTurnNumber}";
        public int SimulationTurnNumber => _simulationState.TurnNumber;
        public string TaxRateSummary => $"Tax rate: {TaxationPolicyCatalog.Get(_taxationState.SelectedTaxRate).DisplayName}";
        public string TaxRevenueSummary => _lastTaxationReport is null
            ? $"Projected collection at current rate: {FormatNumber(_taxationState.ProjectedRevenue)} taels | Baseline plan: {FormatNumber(_foundation.Treasury.BaselineTaxIncome)} taels"
            : $"Last collection: {FormatNumber(_taxationState.LastCollectedRevenue)} taels | Next projection: {FormatNumber(_taxationState.ProjectedRevenue)} taels";
        public string TaxStabilitySummary
        {
            get
            {
                var policy = TaxationPolicyCatalog.Get(_taxationState.SelectedTaxRate);
                return $"Mortal stability effect: {FormatSignedNumber(policy.StabilityDelta)} ({policy.StabilitySummary})";
            }
        }

        public string TreasurySummary => SimulationTurnNumber == 0
            ? $"State reserves: {FormatNumber(_taxationState.CurrentTreasuryFunds)} taels"
            : $"State reserves: {FormatNumber(_taxationState.CurrentTreasuryFunds)} taels | Turn {SimulationTurnNumber} complete";

        public IReadOnlyList<RegionCardViewModel> Regions { get; private set; } = [];
        public IReadOnlyList<TownCardViewModel> Towns { get; private set; } = [];
        public IReadOnlyList<SectCardViewModel> Sects { get; private set; } = [];
        public IReadOnlyList<MinistryCardViewModel> Ministries { get; private set; } = [];
        public IReadOnlyList<TownSimulationCardViewModel> TownSimulations { get; private set; } = [];
        public IReadOnlyList<TownIndustryCardViewModel> TownIndustries { get; private set; } = [];
        public IReadOnlyList<TownTaxationCardViewModel> TownTaxations { get; private set; } = [];
        public IReadOnlyList<RecruitmentEventViewModel> RecruitmentEvents { get; private set; } = [];
        public IReadOnlyList<SectOperationEventViewModel> SectOperationEvents { get; private set; } = [];
        public IReadOnlyList<BuildingEventViewModel> BuildingEvents { get; private set; } = [];
        public IReadOnlyList<MinistryEventViewModel> MinistryEvents { get; private set; } = [];
        public IReadOnlyList<TurnEventViewModel> TurnEvents { get; private set; } = [];

        public bool HasTurnEvents => TurnEvents.Count > 0;
        public bool HasRecruitmentEvents => RecruitmentEvents.Count > 0;
        public bool HasSectOperationEvents => SectOperationEvents.Count > 0;
        public bool HasBuildingEvents => BuildingEvents.Count > 0;
        public bool HasIndustryEvents => TownIndustries.Count > 0;
        public bool HasTaxationEvents => TownTaxations.Count > 0;
        public bool HasMinistryEvents => MinistryEvents.Count > 0;

        public System.Windows.Input.ICommand AdvanceTurnCommand { get; }
        public System.Windows.Input.ICommand ConstructSectInfrastructureCommand { get; }
        public System.Windows.Input.ICommand ConstructMortalInfrastructureCommand { get; }
        public System.Windows.Input.ICommand IncreaseTaxRateCommand { get; }
        public System.Windows.Input.ICommand DecreaseTaxRateCommand { get; }

        private void ExecuteAdvanceTurn()
        {
            var realmResult = _simulationService.Step(_foundation, _simulationState, _taxationState.SelectedTaxRate);
            _lastReport = realmResult.Report;

            var industryResult = _industryService.Step(_foundation, _foundation.Ministries, realmResult.NextState, _industryStates);
            var sectOperationsResult = _sectOperationsService.Step(_foundation, realmResult.NextState, industryResult.NextStates);
            var buildingTurnResult = _buildingSystemService.ApplyTurn(
                _foundation,
                _buildingState,
                sectOperationsResult.NextSects,
                sectOperationsResult.NextIndustryStates,
                _taxationState.CurrentTreasuryFunds);

            _buildingState = buildingTurnResult.NextState;
            _simulationState = realmResult.NextState with { Sects = buildingTurnResult.NextSects };
            _industryStates = buildingTurnResult.NextIndustryStates;
            _lastSectOperationsReport = sectOperationsResult.Report;
            _lastBuildingReport = buildingTurnResult.Report;

            var taxationSeedState = _taxationState with { CurrentTreasuryFunds = buildingTurnResult.NextTreasuryFunds };
            var taxationResult = _taxationService.Step(taxationSeedState, _simulationState, _industryStates);
            _taxationState = taxationResult.NextState;
            _lastTaxationReport = taxationResult.Report;
            var ministryResult = _ministryFrameworkService.Step(_foundation, _ministryState, _simulationState, _buildingState, _taxationState);
            _ministryState = ministryResult.NextState;
            _lastMinistryReport = ministryResult.Report;

            BuildDisplayState();

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(PageSubtitle));
            OnPropertyChanged(nameof(SimulationTurnNumber));
            OnPropertyChanged(nameof(RealmSummary));
            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(TaxRateSummary));
            OnPropertyChanged(nameof(TaxRevenueSummary));
            OnPropertyChanged(nameof(TaxStabilitySummary));
            OnPropertyChanged(nameof(Towns));
            OnPropertyChanged(nameof(Sects));
            OnPropertyChanged(nameof(TownSimulations));
            OnPropertyChanged(nameof(TurnEvents));
            OnPropertyChanged(nameof(RecruitmentEvents));
            OnPropertyChanged(nameof(SectOperationEvents));
            OnPropertyChanged(nameof(BuildingEvents));
            OnPropertyChanged(nameof(HasTurnEvents));
            OnPropertyChanged(nameof(HasRecruitmentEvents));
            OnPropertyChanged(nameof(HasSectOperationEvents));
            OnPropertyChanged(nameof(HasBuildingEvents));
            OnPropertyChanged(nameof(TownIndustries));
            OnPropertyChanged(nameof(HasIndustryEvents));
            OnPropertyChanged(nameof(TownTaxations));
            OnPropertyChanged(nameof(HasTaxationEvents));
            OnPropertyChanged(nameof(Ministries));
            OnPropertyChanged(nameof(MinistryEvents));
            OnPropertyChanged(nameof(HasMinistryEvents));
        }

        private void ExecuteConstructSectInfrastructure()
        {
            var result = _buildingSystemService.ConstructNextSectBuildings(
                _buildingState,
                _simulationState.Sects,
                _taxationState.CurrentTreasuryFunds);

            _buildingState = result.NextState;
            _simulationState = _simulationState with { Sects = result.NextSects };
            _taxationState = _taxationState with { CurrentTreasuryFunds = result.NextTreasuryFunds };
            _ministryState = _ministryFrameworkService.Recalculate(_foundation, _ministryState, _simulationState, _buildingState, _taxationState);
            _lastBuildingReport = result.Report;
            BuildDisplayState();

            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(Sects));
            OnPropertyChanged(nameof(Ministries));
            OnPropertyChanged(nameof(BuildingEvents));
            OnPropertyChanged(nameof(HasBuildingEvents));
        }

        private void ExecuteConstructMortalInfrastructure()
        {
            var result = _buildingSystemService.ConstructNextTownBuildings(
                _buildingState,
                _simulationState.Sects,
                _simulationState.Towns,
                _taxationState.CurrentTreasuryFunds);

            _buildingState = result.NextState;
            _simulationState = _simulationState with { Sects = result.NextSects };
            _taxationState = _taxationState with { CurrentTreasuryFunds = result.NextTreasuryFunds };
            _ministryState = _ministryFrameworkService.Recalculate(_foundation, _ministryState, _simulationState, _buildingState, _taxationState);
            _lastBuildingReport = result.Report;
            BuildDisplayState();

            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(Towns));
            OnPropertyChanged(nameof(Ministries));
            OnPropertyChanged(nameof(BuildingEvents));
            OnPropertyChanged(nameof(HasBuildingEvents));
        }

        private void ExecuteIncreaseTaxRate()
            => SetTaxRate(TaxationPolicyCatalog.Raise(_taxationState.SelectedTaxRate));

        private void ExecuteDecreaseTaxRate()
            => SetTaxRate(TaxationPolicyCatalog.Lower(_taxationState.SelectedTaxRate));

        private void SetTaxRate(TaxRateLevel taxRate)
        {
            if (taxRate == _taxationState.SelectedTaxRate)
            {
                return;
            }

            _taxationState = _taxationService.SetTaxRate(_taxationState, _simulationState, _industryStates, taxRate);
            _ministryState = _ministryFrameworkService.Recalculate(_foundation, _ministryState, _simulationState, _buildingState, _taxationState);
            BuildDisplayState();

            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(TaxRateSummary));
            OnPropertyChanged(nameof(TaxRevenueSummary));
            OnPropertyChanged(nameof(TaxStabilitySummary));
            OnPropertyChanged(nameof(TownTaxations));
            OnPropertyChanged(nameof(HasTaxationEvents));
            OnPropertyChanged(nameof(Ministries));
        }

        private void BuildDisplayState()
        {
            var sectOperationEventsById = _lastSectOperationsReport?.SectEvents
                .ToDictionary(sectorEvent => sectorEvent.SectId);
            var sectBuildingsById = _buildingState.Sects.ToDictionary(item => item.SectId);
            var townBuildingsById = _buildingState.Towns.ToDictionary(item => item.TownId);

            Regions = _foundation.Regions
                .Select(region => new RegionCardViewModel(
                    Name: region.Name,
                    SpiritualVeinSummary: $"Spiritual vein strength: {region.SpiritualVeinStrength}",
                    BaselineSummary: $"Terrain: {region.Baseline.Terrain} | Climate: {region.Baseline.Climate} | Settlement pattern: {region.Baseline.SettlementPattern}",
                    LocalPowerSummary: $"{region.TownIds.Count} towns | {region.SectIds.Count} sects"))
                .ToImmutableArray();

            Towns = _simulationState.Towns
                .Select(simulation =>
                {
                    var town = _townsById[simulation.TownId];

                    return new TownCardViewModel(
                        Name: town.Name,
                        LocationSummary: $"Region: {_regionNamesById.GetValueOrDefault(town.RegionId, "Unknown region")}",
                        PopulationSummary: $"Population: {FormatNumber(simulation.CurrentPopulation)}",
                        IndustrySummary: $"Industry mix: {string.Join(" | ", town.Industries.Select(FormatIndustryAllocation))}",
                        BuildingSummary: $"Buildings: {FormatTownBuildings(townBuildingsById.GetValueOrDefault(town.Id))}",
                        OutputSummary: $"Baseline output: {string.Join(" | ", town.Output.Select(FormatOutputMetric))}");
                })
                .ToImmutableArray();

            Sects = _simulationState.Sects
                .Select(sect => new SectCardViewModel(
                    Name: sect.SectName,
                    LocationSummary: $"Region: {_regionNamesById.GetValueOrDefault(sect.RegionId, "Unknown region")}",
                    FinanceSummary: $"Funds: {FormatNumber(sect.CurrentFunds)} taels | Population: {FormatNumber(sect.CurrentPopulation)}",
                    LoyaltySummary: $"Loyalty: {FormatNumber(sect.Loyalty)}",
                    IndustryPreferenceSummary: $"Industry preference: {FormatIndustryPreference(sect.IndustryPreference)}",
                    RecruitmentPolicySummary: $"Recruitment wage: {FormatRecruitmentWage(sect.RecruitmentWage)} ({FormatNumber(SectRecruitmentPolicyCatalog.Get(sect.RecruitmentWage).WagePerRecruit)} taels per recruit)",
                    RecruitmentSummary: $"Last hires: {FormatNumber(sect.LastRecruitsGained)} | Last wages paid: {FormatNumber(sect.LastWagesPaid)} taels | Recruitables remaining: {FormatNumber(sect.RecruitablesFromRegion)}",
                    BuildingSummary: $"Buildings: {FormatSectBuildings(sectBuildingsById.GetValueOrDefault(sect.SectId))}",
                    OperationsSummary: FormatOperationsSummary(
                        sect,
                        sectOperationEventsById?.GetValueOrDefault(sect.SectId)),
                    OutputSummary: $"Current output: {string.Join(" | ", sect.CurrentOutput.Select(FormatOutputMetric))}"))
                .ToImmutableArray();

            Ministries = _ministryState.Ministries
                .Select(ministry => new MinistryCardViewModel(
                    Name: ministry.MinistryName,
                    AuthoritySummary: $"Authority: {ministry.Authority.DelegationLevel}. {ministry.Authority.EscalationRule}",
                    ResponsibilitySummary: $"Responsibilities: {string.Join(", ", ministry.Authority.Responsibilities)}",
                    StandardSummary: $"Standard: {ministry.Standard.Name}. {ministry.Standard.Summary}",
                    MinisterSummary: $"Minister: {FormatOfficial(ministry.Minister)}",
                    TeamSummary: $"Supporting officials: {string.Join("; ", ministry.SupportingOfficials.Select(FormatOfficial))}",
                    CapacitySummary: $"Handling capacity: {ministry.HandlingCapacity} | Active cases: {ministry.ActiveCaseCount} | Escalated: {ministry.EscalatedCaseCount}",
                    CaseSummary: ministry.ActiveCases.Count == 0
                        ? "Current docket: none."
                        : $"Current docket: {string.Join("; ", ministry.ActiveCases.Select(item => item.Summary))}",
                    ReportSummary: $"Latest report: {ministry.LastSummary}"))
                .ToImmutableArray();

            TownSimulations = _simulationState.Towns
                .Select(town => new TownSimulationCardViewModel(
                    TownName: town.TownName,
                    PopulationSummary: $"Population: {FormatNumber(town.CurrentPopulation)}",
                    FoodBalanceSummary: $"Food balance: {FormatSignedNumber(town.FoodBalance)} units",
                    RecruitmentPoolSummary: $"Recruitment pool: {FormatNumber(town.RecruitmentPool)} mortals",
                    RecruitmentImpactSummary: town.RecruitsLostLastTurn > 0
                        ? $"Recruited away this turn: {FormatNumber(town.RecruitsLostLastTurn)}"
                        : "Recruited away this turn: 0",
                    ChangeSummary: $"{FormatSignedNumber(town.PopulationChange)} total ({town.ChangeReason})"))
                .ToImmutableArray();

            TownIndustries = _industryStates
                .Select(industry => new TownIndustryCardViewModel(
                    TownName: industry.TownName,
                    LaborForceSummary: $"Labor force: Agriculture {FormatNumber(industry.LaborForce.Agriculture)} | Handicrafts {FormatNumber(industry.LaborForce.Handicrafts)} | Commerce {FormatNumber(industry.LaborForce.Commerce)}",
                    GrossOutputSummary: $"Gross output: Agriculture {FormatNumber(industry.GrossOutput.AgricultureUnits)} | Handicrafts {FormatNumber(industry.GrossOutput.HandicraftsUnits)} | Commerce {FormatNumber(industry.GrossOutput.CommerceUnits)}",
                    GovernmentEfficiencySummary: $"Government efficiency: {industry.GovernmentEfficiency:F2}x",
                    NetOutputSummary: $"Net output: Agriculture {FormatNumber(industry.NetOutput.AgricultureUnits)} | Handicrafts {FormatNumber(industry.NetOutput.HandicraftsUnits)} | Commerce {FormatNumber(industry.NetOutput.CommerceUnits)}",
                    PurchasableSurplusSummary: $"Available for purchase: Agriculture {FormatNumber(industry.PurchasableSurplus.AgricultureUnits)} | Handicrafts {FormatNumber(industry.PurchasableSurplus.HandicraftsUnits)} | Commerce {FormatNumber(industry.PurchasableSurplus.CommerceUnits)}"))
                .ToImmutableArray();

            TownTaxations = _taxationState.Towns
                .Select(taxation => new TownTaxationCardViewModel(
                    TownName: taxation.TownName,
                    GrossTaxBaseSummary: $"Gross tax base: {FormatNumber(taxation.GrossTaxBase)} taels",
                    CollectedRevenueSummary: $"Projected collection: {FormatNumber(taxation.CollectedRevenue)} taels",
                    StabilitySummary: $"Stability effect: {FormatSignedNumber(taxation.StabilityDelta)} ({taxation.StabilitySummary})"))
                .ToImmutableArray();

            if (_lastReport is null)
            {
                TurnEvents = [];
                RecruitmentEvents = [];
                SectOperationEvents = [];
                BuildingEvents = [];
                MinistryEvents = [];
                return;
            }

            TurnEvents = _lastReport.TownEvents
                .Select(turnEvent => new TurnEventViewModel(
                    TownName: turnEvent.TownName,
                    PopulationChange: FormatSignedNumber(turnEvent.CurrentPopulation - turnEvent.PreviousPopulation),
                    ChangeReason: turnEvent.ChangeReason,
                    RecruitmentLoss: turnEvent.RecruitsLost > 0
                        ? $"{FormatNumber(turnEvent.RecruitsLost)} taken by sects"
                        : "No recruits taken",
                    FoodBalance: FormatSignedNumber(turnEvent.FoodBalance)))
                .ToImmutableArray();

            RecruitmentEvents = _lastReport.RecruitmentEvents
                .Select(recruitmentEvent => new RecruitmentEventViewModel(
                    SectName: recruitmentEvent.SectName,
                    RecruitsSummary: $"{FormatRecruitmentWage(recruitmentEvent.RecruitmentWage)} wage recruited {FormatNumber(recruitmentEvent.RecruitsGathered)} mortals from {_regionNamesById.GetValueOrDefault(recruitmentEvent.RegionId, "Unknown region")}, paid {FormatNumber(recruitmentEvent.WagesPaid)} taels, left {FormatNumber(recruitmentEvent.FundsRemaining)} taels. {recruitmentEvent.OutcomeSummary}"))
                .ToImmutableArray();

            SectOperationEvents = _lastSectOperationsReport?.SectEvents
                .Select(operationEvent => new SectOperationEventViewModel(
                    SectName: operationEvent.SectName,
                    Summary: $"{operationEvent.InputIndustry} inputs {FormatNumber(operationEvent.PurchasedUnits)}/{FormatNumber(operationEvent.RequestedUnits)}, upkeep {FormatNumber(operationEvent.UpkeepPaid)} taels, input cost {FormatNumber(operationEvent.InputPurchaseCost)} taels, output {FormatPercent(operationEvent.OutputFactor)}, funds after ops {FormatNumber(operationEvent.FundsAfter)} taels. {operationEvent.OperationSummary}"))
                .ToImmutableArray()
                ?? [];

            BuildingEvents = _lastBuildingReport is null
                ? []
                : _lastBuildingReport.ConstructionEvents
                    .Select(evt => new BuildingEventViewModel(evt.OwnerName, evt.Summary))
                    .Concat(_lastBuildingReport.OperationEvents.Select(evt => new BuildingEventViewModel(evt.OwnerName, evt.Summary)))
                    .ToImmutableArray();

            MinistryEvents = _lastMinistryReport?.MinistryEvents
                .Select(evt => new MinistryEventViewModel(
                    MinistryName: evt.MinistryName,
                    Summary: $"{evt.Summary}"))
                .ToImmutableArray()
                ?? [];
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static string FormatIndustryAllocation(IndustryAllocation allocation)
            => $"{allocation.Industry} {allocation.WorkforceShare}%";

        private static string FormatOfficial(OfficialState official)
            => $"{official.Name} ({official.Role}) - Administration {official.Ratings.Administration}, Integrity {official.Ratings.Integrity}, Loyalty {official.Ratings.Loyalty}";

        private static string FormatOutputMetric(OutputMetric output)
            => $"{output.Label} {FormatNumber(output.Amount)} {output.Unit}";

        private static string FormatIndustryPreference(MortalIndustryType? industryPreference)
            => industryPreference?.ToString() ?? "None yet";

        private static string FormatRecruitmentWage(RecruitmentWageLevel recruitmentWage)
            => SectRecruitmentPolicyCatalog.Get(recruitmentWage).DisplayName;

        private static string FormatOperationsSummary(
            SectSimulationState sect,
            SectOperationEvent? operationEvent)
        {
            var profile = SectOperationsCatalog.Get(sect.SectId);
            return operationEvent is null
                ? $"Autonomous ops: upkeep {FormatNumber(profile.UpkeepCost)} taels | needs {FormatNumber(profile.RequiredUnitsPerTurn)} {profile.InputIndustry} units"
                : $"Autonomous ops: upkeep {FormatNumber(operationEvent.UpkeepPaid)} taels | inputs {FormatNumber(operationEvent.PurchasedUnits)}/{FormatNumber(operationEvent.RequestedUnits)} {operationEvent.InputIndustry} | output {FormatPercent(operationEvent.OutputFactor)}";
        }

        private static string FormatPercent(decimal value)
            => $"{Math.Round(value * 100m, 0, MidpointRounding.AwayFromZero)}%";

        private static string FormatSectBuildings(SectBuildingInventoryState? inventory)
            => inventory is null
                ? "None"
                : string.Join(
                    " | ",
                    inventory.Buildings.Select(item => $"{BuildingCatalog.Get(item.Building).DisplayName} x{item.Quantity}")
                        .Concat(inventory.ActiveProject is null
                            ? []
                            : [$"Project: {BuildingCatalog.Get(inventory.ActiveProject.Building).DisplayName} ({inventory.ActiveProject.TurnsRemaining} turn(s))"]))
                    .Trim() is { Length: > 0 } summary
                        ? summary
                        : "None";

        private static string FormatTownBuildings(TownBuildingInventoryState? inventory)
            => inventory is null || inventory.Buildings.Count == 0
                ? "None"
                : string.Join(" | ", inventory.Buildings.Select(item => $"{BuildingCatalog.Get(item.Building).DisplayName} x{item.Quantity}"));

        private static string FormatNumber(decimal value)
        {
            if (decimal.Truncate(value) == value)
            {
                return ((int)value).ToString("N0", CultureInfo.InvariantCulture);
            }

            return value.ToString("N1", CultureInfo.InvariantCulture);
        }

        private static string FormatSignedNumber(int value)
            => value > 0
                ? $"+{FormatNumber(value)}"
                : value < 0
                    ? $"-{FormatNumber(-value)}"
                    : "0";
    }

    public sealed record RegionCardViewModel(
        string Name,
        string SpiritualVeinSummary,
        string BaselineSummary,
        string LocalPowerSummary);

    public sealed record TownCardViewModel(
        string Name,
        string LocationSummary,
        string PopulationSummary,
        string IndustrySummary,
        string BuildingSummary,
        string OutputSummary);

    public sealed record SectCardViewModel(
        string Name,
        string LocationSummary,
        string FinanceSummary,
        string LoyaltySummary,
        string IndustryPreferenceSummary,
        string RecruitmentPolicySummary,
        string RecruitmentSummary,
        string BuildingSummary,
        string OperationsSummary,
        string OutputSummary);

    public sealed record MinistryCardViewModel(
        string Name,
        string AuthoritySummary,
        string ResponsibilitySummary,
        string StandardSummary,
        string MinisterSummary,
        string TeamSummary,
        string CapacitySummary,
        string CaseSummary,
        string ReportSummary);

    public sealed record TownSimulationCardViewModel(
        string TownName,
        string PopulationSummary,
        string FoodBalanceSummary,
        string RecruitmentPoolSummary,
        string RecruitmentImpactSummary,
        string ChangeSummary);

    public sealed record TurnEventViewModel(
        string TownName,
        string PopulationChange,
        string ChangeReason,
        string RecruitmentLoss,
        string FoodBalance);

    public sealed record RecruitmentEventViewModel(
        string SectName,
        string RecruitsSummary);

    public sealed record SectOperationEventViewModel(
        string SectName,
        string Summary);

    public sealed record MinistryEventViewModel(
        string MinistryName,
        string Summary);

    public sealed record BuildingEventViewModel(
        string OwnerName,
        string Summary);

    public sealed record TownIndustryCardViewModel(
        string TownName,
        string LaborForceSummary,
        string GrossOutputSummary,
        string GovernmentEfficiencySummary,
        string NetOutputSummary,
        string PurchasableSurplusSummary);

    public sealed record TownTaxationCardViewModel(
        string TownName,
        string GrossTaxBaseSummary,
        string CollectedRevenueSummary,
        string StabilitySummary);
}
