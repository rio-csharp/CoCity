using System.Globalization;
using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.ViewModels
{
    public sealed class MainPageViewModel
    {
        public MainPageViewModel(ICoreDataFoundationService foundationService)
        {
            var realm = foundationService.GetInitialState();
            var regionNames = realm.Regions.ToDictionary(region => region.Id, region => region.Name);

            PageTitle = "Core Data Foundations";
            PageSubtitle = "Task 1.1 establishes the first read-only world state for CoCity. The app now exposes the realm's core entities without running later simulation systems.";
            RealmSummary = $"{realm.RealmName} currently tracks {realm.Regions.Count} regions, {realm.Towns.Count} towns, {realm.Sects.Count} sects, and {realm.Ministries.Count} ministries.";

            Treasury = new TreasuryCardViewModel(
                FundsSummary: $"State reserves: {FormatNumber(realm.Treasury.Funds)} taels",
                TaxIncomeSummary: $"Baseline tax income: {FormatNumber(realm.Treasury.BaselineTaxIncome)} taels per cycle");

            Regions = realm.Regions
                .Select(region => new RegionCardViewModel(
                    Name: region.Name,
                    SpiritualVeinSummary: $"Spiritual vein strength: {region.SpiritualVeinStrength}",
                    BaselineSummary: $"Terrain: {region.Baseline.Terrain} | Climate: {region.Baseline.Climate} | Settlement pattern: {region.Baseline.SettlementPattern}",
                    LocalPowerSummary: $"{region.TownIds.Count} towns | {region.SectIds.Count} sects"))
                .ToArray();

            Towns = realm.Towns
                .Select(town => new TownCardViewModel(
                    Name: town.Name,
                    LocationSummary: $"Region: {regionNames.GetValueOrDefault(town.RegionId, "Unknown region")}",
                    PopulationSummary: $"Population: {FormatNumber(town.Population)}",
                    IndustrySummary: $"Industry mix: {string.Join(" | ", town.Industries.Select(FormatIndustryAllocation))}",
                    OutputSummary: $"Baseline output: {string.Join(" | ", town.Output.Select(FormatOutputMetric))}"))
                .ToArray();

            Sects = realm.Sects
                .Select(sect => new SectCardViewModel(
                    Name: sect.Name,
                    LocationSummary: $"Region: {regionNames.GetValueOrDefault(sect.RegionId, "Unknown region")}",
                    FinanceSummary: $"Funds: {FormatNumber(sect.Funds)} taels | Population: {FormatNumber(sect.Population)}",
                    OutputSummary: $"Baseline output: {string.Join(" | ", sect.Output.Select(FormatOutputMetric))}"))
                .ToArray();

            Ministries = realm.Ministries
                .Select(ministry => new MinistryCardViewModel(
                    Name: ministry.Name,
                    AuthoritySummary: $"Authority: {ministry.Authority.DelegationLevel}. {ministry.Authority.EscalationRule}",
                    ResponsibilitySummary: $"Responsibilities: {string.Join(", ", ministry.Authority.Responsibilities)}",
                    StandardSummary: $"Standard: {ministry.Standard.Name}. {ministry.Standard.Summary}",
                    MinisterSummary: $"Minister: {FormatOfficial(ministry.Minister)}",
                    TeamSummary: $"Supporting officials: {string.Join("; ", ministry.SupportingOfficials.Select(FormatOfficial))}"))
                .ToArray();
        }

        public string PageTitle { get; }

        public string PageSubtitle { get; }

        public string RealmSummary { get; }

        public TreasuryCardViewModel Treasury { get; }

        public IReadOnlyList<RegionCardViewModel> Regions { get; }

        public IReadOnlyList<TownCardViewModel> Towns { get; }

        public IReadOnlyList<SectCardViewModel> Sects { get; }

        public IReadOnlyList<MinistryCardViewModel> Ministries { get; }

        private static string FormatIndustryAllocation(IndustryAllocation allocation)
        {
            return $"{allocation.Industry} {allocation.WorkforceShare}%";
        }

        private static string FormatOfficial(OfficialState official)
        {
            return $"{official.Name} ({official.Role}) - Administration {official.Ratings.Administration}, Integrity {official.Ratings.Integrity}, Loyalty {official.Ratings.Loyalty}";
        }

        private static string FormatOutputMetric(OutputMetric output)
        {
            return $"{output.Label} {FormatNumber(output.Amount)} {output.Unit}";
        }

        private static string FormatNumber(decimal value)
        {
            if (decimal.Truncate(value) == value)
            {
                return value.ToString("N0", CultureInfo.InvariantCulture);
            }

            return value.ToString("N1", CultureInfo.InvariantCulture);
        }
    }

    public sealed record TreasuryCardViewModel(
        string FundsSummary,
        string TaxIncomeSummary);

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
        string OutputSummary);

    public sealed record MinistryCardViewModel(
        string Name,
        string AuthoritySummary,
        string ResponsibilitySummary,
        string StandardSummary,
        string MinisterSummary,
        string TeamSummary);
}
