using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed record SectOperationsProfile(
        MortalIndustryType InputIndustry,
        int RequiredUnitsPerTurn,
        decimal UnitPurchasePrice,
        decimal UpkeepCost);

    public static class SectOperationsCatalog
    {
        public static SectOperationsProfile Get(string sectId)
            => sectId switch
            {
                "sect.azure-talisman-academy" => new(
                    InputIndustry: MortalIndustryType.Handicrafts,
                    RequiredUnitsPerTurn: 120,
                    UnitPurchasePrice: 3m,
                    UpkeepCost: 180m),
                "sect.iron-peak-hall" => new(
                    InputIndustry: MortalIndustryType.Commerce,
                    RequiredUnitsPerTurn: 90,
                    UnitPurchasePrice: 4m,
                    UpkeepCost: 220m),
                "sect.verdant-crucible-sect" => new(
                    InputIndustry: MortalIndustryType.Agriculture,
                    RequiredUnitsPerTurn: 160,
                    UnitPurchasePrice: 2m,
                    UpkeepCost: 260m),
                _ => throw new InvalidOperationException($"No sect operations profile configured for '{sectId}'.")
            };
    }
}
