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
        private readonly IMortalTaxationSimulationService _taxationService;
        private readonly IReadOnlyDictionary<string, string> _regionNamesById;
        private readonly IReadOnlyDictionary<string, MortalTownState> _townsById;
        private MortalRealmState _simulationState;
        private IReadOnlyList<MortalTownIndustryState> _industryStates;
        private RealmTaxationState _taxationState;
        private TurnReport? _lastReport;
        private TaxationTurnReport? _lastTaxationReport;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel(
            ICoreDataFoundationService foundationService,
            IMortalRealmSimulationService simulationService,
            IMortalIndustrySimulationService industryService,
            IMortalTaxationSimulationService taxationService)
        {
            _simulationService = simulationService;
            _industryService = industryService;
            _taxationService = taxationService;
            _foundation = foundationService.GetInitialState();
            _regionNamesById = _foundation.Regions.ToDictionary(region => region.Id, region => region.Name);
            _townsById = _foundation.Towns.ToDictionary(town => town.Id);
            _simulationState = _simulationService.Initialize(_foundation);
            _industryStates = _industryService.Initialize(_foundation, _foundation.Ministries);
            _taxationState = _taxationService.Initialize(_foundation, _simulationState, _industryStates);

            AdvanceTurnCommand = new Microsoft.Maui.Controls.Command(ExecuteAdvanceTurn);
            IncreaseTaxRateCommand = new Microsoft.Maui.Controls.Command(ExecuteIncreaseTaxRate);
            DecreaseTaxRateCommand = new Microsoft.Maui.Controls.Command(ExecuteDecreaseTaxRate);
            BuildDisplayState();
        }

        public string PageTitle => "Prototype Closed Loop Dashboard";
        public string PageSubtitle => "Task 1.6 upgrades sect recruitment into a wage-driven workflow with visible hires, wage costs, and sect-fund changes.";
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
        public IReadOnlyList<TurnEventViewModel> TurnEvents { get; private set; } = [];

        public bool HasTurnEvents => TurnEvents.Count > 0;
        public bool HasRecruitmentEvents => RecruitmentEvents.Count > 0;
        public bool HasIndustryEvents => TownIndustries.Count > 0;
        public bool HasTaxationEvents => TownTaxations.Count > 0;

        public System.Windows.Input.ICommand AdvanceTurnCommand { get; }
        public System.Windows.Input.ICommand IncreaseTaxRateCommand { get; }
        public System.Windows.Input.ICommand DecreaseTaxRateCommand { get; }

        private void ExecuteAdvanceTurn()
        {
            var result = _simulationService.Step(_foundation, _simulationState, _taxationState.SelectedTaxRate);
            _simulationState = result.NextState;
            _lastReport = result.Report;

            var industryResult = _industryService.Step(_foundation, _foundation.Ministries, _simulationState, _industryStates);
            _industryStates = industryResult.NextStates;
            var taxationResult = _taxationService.Step(_taxationState, _simulationState, _industryStates);
            _taxationState = taxationResult.NextState;
            _lastTaxationReport = taxationResult.Report;

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
            OnPropertyChanged(nameof(HasTurnEvents));
            OnPropertyChanged(nameof(HasRecruitmentEvents));
            OnPropertyChanged(nameof(TownIndustries));
            OnPropertyChanged(nameof(HasIndustryEvents));
            OnPropertyChanged(nameof(TownTaxations));
            OnPropertyChanged(nameof(HasTaxationEvents));
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
            BuildDisplayState();

            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(TaxRateSummary));
            OnPropertyChanged(nameof(TaxRevenueSummary));
            OnPropertyChanged(nameof(TaxStabilitySummary));
            OnPropertyChanged(nameof(TownTaxations));
            OnPropertyChanged(nameof(HasTaxationEvents));
        }

        private void BuildDisplayState()
        {
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
                    OutputSummary: $"Current output: {string.Join(" | ", sect.CurrentOutput.Select(FormatOutputMetric))}"))
                .ToImmutableArray();

            Ministries = _foundation.Ministries
                .Select(ministry => new MinistryCardViewModel(
                    Name: ministry.Name,
                    AuthoritySummary: $"Authority: {ministry.Authority.DelegationLevel}. {ministry.Authority.EscalationRule}",
                    ResponsibilitySummary: $"Responsibilities: {string.Join(", ", ministry.Authority.Responsibilities)}",
                    StandardSummary: $"Standard: {ministry.Standard.Name}. {ministry.Standard.Summary}",
                    MinisterSummary: $"Minister: {FormatOfficial(ministry.Minister)}",
                    TeamSummary: $"Supporting officials: {string.Join("; ", ministry.SupportingOfficials.Select(FormatOfficial))}"))
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
        string OutputSummary);

    public sealed record SectCardViewModel(
        string Name,
        string LocationSummary,
        string FinanceSummary,
        string LoyaltySummary,
        string IndustryPreferenceSummary,
        string RecruitmentPolicySummary,
        string RecruitmentSummary,
        string OutputSummary);

    public sealed record MinistryCardViewModel(
        string Name,
        string AuthoritySummary,
        string ResponsibilitySummary,
        string StandardSummary,
        string MinisterSummary,
        string TeamSummary);

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
