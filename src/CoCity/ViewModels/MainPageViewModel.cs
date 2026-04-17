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
        private readonly IReadOnlyDictionary<string, string> _regionNamesById;
        private readonly IReadOnlyDictionary<string, MortalTownState> _townsById;
        private MortalRealmState _simulationState;
        private IReadOnlyList<MortalTownIndustryState> _industryStates;
        private TurnReport? _lastReport;
        private IndustryTurnReport? _lastIndustryReport;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel(
            ICoreDataFoundationService foundationService,
            IMortalRealmSimulationService simulationService,
            IMortalIndustrySimulationService industryService)
        {
            _simulationService = simulationService;
            _industryService = industryService;
            _foundation = foundationService.GetInitialState();
            _regionNamesById = _foundation.Regions.ToDictionary(region => region.Id, region => region.Name);
            _townsById = _foundation.Towns.ToDictionary(town => town.Id);
            _simulationState = _simulationService.Initialize(_foundation);
            _industryStates = _industryService.Initialize(_foundation, _foundation.Ministries);

            AdvanceTurnCommand = new Microsoft.Maui.Controls.Command(ExecuteAdvanceTurn);
            BuildDisplayState();
        }

        public string PageTitle => "Mortal Realm Simulation";
        public string PageSubtitle => "Task 1.3 adds industry output calculation: labor force drives production, government efficiency modulates output.";
        public string RealmSummary => $"{_foundation.RealmName} — Turn {SimulationTurnNumber}";
        public int SimulationTurnNumber => _simulationState.TurnNumber;

        public string TreasurySummary => SimulationTurnNumber == 0
            ? $"State reserves: {FormatNumber(_foundation.Treasury.Funds)} taels"
            : $"State reserves: {FormatNumber(_foundation.Treasury.Funds)} taels | Turn {SimulationTurnNumber} complete";

        public IReadOnlyList<RegionCardViewModel> Regions { get; private set; } = [];
        public IReadOnlyList<TownCardViewModel> Towns { get; private set; } = [];
        public IReadOnlyList<SectCardViewModel> Sects { get; private set; } = [];
        public IReadOnlyList<MinistryCardViewModel> Ministries { get; private set; } = [];
        public IReadOnlyList<TownSimulationCardViewModel> TownSimulations { get; private set; } = [];
        public IReadOnlyList<TownIndustryCardViewModel> TownIndustries { get; private set; } = [];
        public IReadOnlyList<RecruitmentEventViewModel> RecruitmentEvents { get; private set; } = [];
        public IReadOnlyList<TurnEventViewModel> TurnEvents { get; private set; } = [];

        public bool HasTurnEvents => TurnEvents.Count > 0;
        public bool HasRecruitmentEvents => RecruitmentEvents.Count > 0;
        public bool HasIndustryEvents => TownIndustries.Count > 0;

        public System.Windows.Input.ICommand AdvanceTurnCommand { get; }

        private void ExecuteAdvanceTurn()
        {
            var result = _simulationService.Step(_foundation, _simulationState);
            _simulationState = result.NextState;
            _lastReport = result.Report;

            var industryResult = _industryService.Step(_foundation, _foundation.Ministries, _industryStates);
            _industryStates = industryResult.NextStates;
            _lastIndustryReport = industryResult.Report;

            BuildDisplayState();

            OnPropertyChanged(nameof(SimulationTurnNumber));
            OnPropertyChanged(nameof(RealmSummary));
            OnPropertyChanged(nameof(TreasurySummary));
            OnPropertyChanged(nameof(Towns));
            OnPropertyChanged(nameof(Sects));
            OnPropertyChanged(nameof(TownSimulations));
            OnPropertyChanged(nameof(TurnEvents));
            OnPropertyChanged(nameof(RecruitmentEvents));
            OnPropertyChanged(nameof(HasTurnEvents));
            OnPropertyChanged(nameof(HasRecruitmentEvents));
            OnPropertyChanged(nameof(TownIndustries));
            OnPropertyChanged(nameof(HasIndustryEvents));
        }

        private void BuildDisplayState()
        {
            var sectRecruitablesById = _simulationState.Sects.ToDictionary(sect => sect.SectId, sect => sect.RecruitablesFromRegion);

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

            Sects = _foundation.Sects
                .Select(sect => new SectCardViewModel(
                    Name: sect.Name,
                    LocationSummary: $"Region: {_regionNamesById.GetValueOrDefault(sect.RegionId, "Unknown region")}",
                    FinanceSummary: $"Funds: {FormatNumber(sect.Funds)} taels | Population: {FormatNumber(sect.Population)}",
                    RecruitmentSummary: $"Recruitables remaining in region: {FormatNumber(sectRecruitablesById.GetValueOrDefault(sect.Id))}",
                    OutputSummary: $"Baseline output: {string.Join(" | ", sect.Output.Select(FormatOutputMetric))}"))
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
                    RecruitsSummary: $"Recruited {FormatNumber(recruitmentEvent.RecruitsGathered)} mortals from {_regionNamesById.GetValueOrDefault(recruitmentEvent.RegionId, "Unknown region")}"))
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
}
