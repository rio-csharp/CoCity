using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed record SectBuildingDefinition(
        SectBuildingType Building,
        string DisplayName,
        decimal ConstructionCost,
        decimal UpkeepCost,
        decimal OutputBonus);

    public sealed record MortalBuildingDefinition(
        MortalBuildingType Building,
        string DisplayName,
        decimal ConstructionCost,
        decimal UpkeepCost,
        MortalIndustryType AffectedIndustry,
        decimal OutputBonus);

    public static class BuildingCatalog
    {
        public static IReadOnlyList<SectBuildingType> SectBuildOrder { get; } =
            ImmutableArray.Create(
                SectBuildingType.GateHall,
                SectBuildingType.DiscipleQuarters,
                SectBuildingType.Warehouse,
                SectBuildingType.SpiritGatheringArray,
                SectBuildingType.AlchemyRoom);

        public static IReadOnlyList<MortalBuildingType> MortalBuildOrder { get; } =
            ImmutableArray.Create(
                MortalBuildingType.Farm,
                MortalBuildingType.Workshop,
                MortalBuildingType.Market);

        public static SectBuildingDefinition Get(SectBuildingType building)
            => building switch
            {
                SectBuildingType.GateHall => new(building, "Gate Hall", 600m, 35m, 0.05m),
                SectBuildingType.DiscipleQuarters => new(building, "Disciple Quarters", 550m, 30m, 0.07m),
                SectBuildingType.Warehouse => new(building, "Warehouse", 500m, 28m, 0.04m),
                SectBuildingType.SpiritGatheringArray => new(building, "Spirit Gathering Array", 800m, 45m, 0.12m),
                SectBuildingType.AlchemyRoom => new(building, "Alchemy Room", 750m, 40m, 0.10m),
                _ => throw new ArgumentOutOfRangeException(nameof(building), building, "Unknown sect building type.")
            };

        public static MortalBuildingDefinition Get(MortalBuildingType building)
            => building switch
            {
                MortalBuildingType.Farm => new(building, "Farm", 450m, 25m, MortalIndustryType.Agriculture, 0.10m),
                MortalBuildingType.Workshop => new(building, "Workshop", 520m, 30m, MortalIndustryType.Handicrafts, 0.10m),
                MortalBuildingType.Market => new(building, "Market", 480m, 28m, MortalIndustryType.Commerce, 0.08m),
                _ => throw new ArgumentOutOfRangeException(nameof(building), building, "Unknown mortal building type.")
            };
    }
}
