using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultBuildingSystemService : IBuildingSystemService
    {
        public RealmBuildingState Initialize(RealmState foundation)
            => new(
                Sects: foundation.Sects
                    .OrderBy(sect => sect.RegionId)
                    .ThenBy(sect => sect.Id)
                    .Select(sect => new SectBuildingInventoryState(sect.Id, []))
                    .ToImmutableArray(),
                Towns: foundation.Towns
                    .OrderBy(town => town.RegionId)
                    .ThenBy(town => town.Id)
                    .Select(town => new TownBuildingInventoryState(town.Id, []))
                    .ToImmutableArray());

        public BuildingConstructionResult ConstructNextSectBuildings(
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            decimal currentTreasuryFunds)
        {
            var sectInventories = currentState.Sects.ToDictionary(item => item.SectId);
            var nextSects = sects
                .Select(sect =>
                {
                    var inventory = sectInventories[sect.SectId];
                    var nextBuilding = GetNextSectBuilding(inventory.Buildings);
                    if (nextBuilding is null)
                    {
                        return sect;
                    }

                    var definition = BuildingCatalog.Get(nextBuilding.Value);
                    if (sect.CurrentFunds < definition.ConstructionCost)
                    {
                        return sect;
                    }

                    sectInventories[sect.SectId] = inventory with
                    {
                        Buildings = AddSectBuilding(inventory.Buildings, nextBuilding.Value)
                    };

                    return sect with
                    {
                        CurrentFunds = sect.CurrentFunds - definition.ConstructionCost
                    };
                })
                .ToImmutableArray();

            var constructionEvents = nextSects
                .Zip(sects)
                .Select(pair => (Next: pair.First, Previous: pair.Second))
                .Where(pair => pair.Next.CurrentFunds != pair.Previous.CurrentFunds)
                .Select(pair =>
                {
                    var addedBuilding = GetAddedSectBuilding(
                        currentState.Sects.Single(item => item.SectId == pair.Next.SectId).Buildings,
                        sectInventories[pair.Next.SectId].Buildings);
                    var definition = BuildingCatalog.Get(addedBuilding);
                    return new BuildingConstructionEvent(
                        OwnerName: pair.Next.SectName,
                        BuildingName: definition.DisplayName,
                        ConstructionCost: definition.ConstructionCost,
                        Summary: $"Constructed {definition.DisplayName} for {definition.ConstructionCost} taels.");
                })
                .ToImmutableArray();

            return new BuildingConstructionResult(
                NextState: currentState with { Sects = sectInventories.Values.OrderBy(item => item.SectId).ToImmutableArray() },
                NextSects: nextSects,
                NextTreasuryFunds: currentTreasuryFunds,
                Report: new BuildingReport(constructionEvents, []));
        }

        public BuildingConstructionResult ConstructNextTownBuildings(
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyList<MortalTownSimulationState> towns,
            decimal currentTreasuryFunds)
        {
            var treasury = currentTreasuryFunds;
            var townInventories = currentState.Towns.ToDictionary(item => item.TownId);
            var constructionEvents = ImmutableArray.CreateBuilder<BuildingConstructionEvent>();

            foreach (var town in towns.OrderBy(item => item.TownId))
            {
                var inventory = townInventories[town.TownId];
                var nextBuilding = GetNextTownBuilding(inventory.Buildings);
                if (nextBuilding is null)
                {
                    continue;
                }

                var definition = BuildingCatalog.Get(nextBuilding.Value);
                if (treasury < definition.ConstructionCost)
                {
                    continue;
                }

                treasury -= definition.ConstructionCost;
                townInventories[town.TownId] = inventory with
                {
                    Buildings = AddTownBuilding(inventory.Buildings, nextBuilding.Value)
                };
                constructionEvents.Add(new BuildingConstructionEvent(
                    OwnerName: town.TownName,
                    BuildingName: definition.DisplayName,
                    ConstructionCost: definition.ConstructionCost,
                    Summary: $"Constructed {definition.DisplayName} for {definition.ConstructionCost} taels."));
            }

            return new BuildingConstructionResult(
                NextState: currentState with { Towns = townInventories.Values.OrderBy(item => item.TownId).ToImmutableArray() },
                NextSects: sects,
                NextTreasuryFunds: treasury,
                Report: new BuildingReport(constructionEvents.ToImmutable(), []));
        }

        public BuildingTurnResult ApplyTurn(
            RealmState foundation,
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            decimal currentTreasuryFunds)
        {
            var sectInventories = currentState.Sects.ToDictionary(item => item.SectId);
            var townInventories = currentState.Towns.ToDictionary(item => item.TownId);

            var nextSects = sects
                .Select(sect =>
                {
                    var inventory = sectInventories[sect.SectId];
                    var upkeepRequired = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                    var upkeepPaid = Math.Min(sect.CurrentFunds, upkeepRequired);
                    var upkeepCoverage = upkeepRequired <= 0 ? 1m : Math.Min(1m, upkeepPaid / upkeepRequired);
                    var outputBonus = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity);
                    var multiplier = 1m + (outputBonus * upkeepCoverage);

                    return sect with
                    {
                        CurrentFunds = sect.CurrentFunds - upkeepPaid,
                        CurrentOutput = ScaleOutput(sect.CurrentOutput, multiplier)
                    };
                })
                .ToImmutableArray();

            var treasury = currentTreasuryFunds;
            var nextIndustryStatesBuilder = ImmutableArray.CreateBuilder<MortalTownIndustryState>();
            foreach (var state in industryStates)
            {
                var inventory = townInventories[state.TownId];
                var upkeepRequired = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                var upkeepPaid = Math.Min(treasury, upkeepRequired);
                treasury -= upkeepPaid;
                var coverage = upkeepRequired <= 0 ? 1m : Math.Min(1m, upkeepPaid / upkeepRequired);

                nextIndustryStatesBuilder.Add(ApplyTownBuildingBonuses(state, inventory.Buildings, coverage));
            }

            var nextIndustryStates = nextIndustryStatesBuilder.ToImmutable();

            var operationEvents = nextSects
                .Select(sect =>
                {
                    var inventory = sectInventories[sect.SectId];
                    var upkeepRequired = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                    var upkeepPaid = Math.Min(sects.Single(current => current.SectId == sect.SectId).CurrentFunds, upkeepRequired);
                    var outputBonus = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity);
                    var coverage = upkeepRequired <= 0 ? 1m : Math.Min(1m, upkeepPaid / upkeepRequired);
                    return new BuildingOperationEvent(
                        OwnerName: sect.SectName,
                        Summary: inventory.Buildings.Count == 0
                            ? "No sect buildings constructed yet."
                            : $"Building upkeep {upkeepPaid} taels | output modifier +{Math.Round(outputBonus * coverage * 100m, 0, MidpointRounding.AwayFromZero)}%.");
                })
                .Concat(nextIndustryStates.Select(state =>
                {
                    var inventory = townInventories[state.TownId];
                    var upkeepRequired = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                    return new BuildingOperationEvent(
                        OwnerName: state.TownName,
                        Summary: inventory.Buildings.Count == 0
                            ? "No mortal buildings constructed yet."
                            : $"Building upkeep applied to local infrastructure (required {upkeepRequired} taels this turn).");
                }))
                .ToImmutableArray();

            return new BuildingTurnResult(
                NextState: currentState,
                NextSects: nextSects,
                NextIndustryStates: nextIndustryStates,
                NextTreasuryFunds: treasury,
                Report: new BuildingReport([], operationEvents));
        }

        private static SectBuildingType? GetNextSectBuilding(IReadOnlyList<SectBuildingCount> buildings)
            => BuildingCatalog.SectBuildOrder
                .Cast<SectBuildingType?>()
                .FirstOrDefault(building => building is not null && buildings.All(item => item.Building != building.Value));

        private static MortalBuildingType? GetNextTownBuilding(IReadOnlyList<MortalBuildingCount> buildings)
            => BuildingCatalog.MortalBuildOrder
                .Cast<MortalBuildingType?>()
                .FirstOrDefault(building => building is not null && buildings.All(item => item.Building != building.Value));

        private static SectBuildingType GetAddedSectBuilding(
            IReadOnlyList<SectBuildingCount> previous,
            IReadOnlyList<SectBuildingCount> next)
            => next.Single(item => previous.All(existing => existing.Building != item.Building)).Building;

        private static ImmutableArray<SectBuildingCount> AddSectBuilding(
            IReadOnlyList<SectBuildingCount> buildings,
            SectBuildingType building)
            => buildings
                .Concat([new SectBuildingCount(building, 1)])
                .ToImmutableArray();

        private static ImmutableArray<MortalBuildingCount> AddTownBuilding(
            IReadOnlyList<MortalBuildingCount> buildings,
            MortalBuildingType building)
            => buildings
                .Concat([new MortalBuildingCount(building, 1)])
                .ToImmutableArray();

        private static ImmutableArray<OutputMetric> ScaleOutput(
            IReadOnlyList<OutputMetric> output,
            decimal multiplier)
            => output
                .Select(metric => metric with
                {
                    Amount = Math.Round(metric.Amount * multiplier, 1, MidpointRounding.AwayFromZero)
                })
                .ToImmutableArray();

        private static MortalTownIndustryState ApplyTownBuildingBonuses(
            MortalTownIndustryState state,
            IReadOnlyList<MortalBuildingCount> buildings,
            decimal coverage)
        {
            var agricultureBonus = buildings
                .Where(item => BuildingCatalog.Get(item.Building).AffectedIndustry == MortalIndustryType.Agriculture)
                .Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity) * coverage;
            var handicraftsBonus = buildings
                .Where(item => BuildingCatalog.Get(item.Building).AffectedIndustry == MortalIndustryType.Handicrafts)
                .Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity) * coverage;
            var commerceBonus = buildings
                .Where(item => BuildingCatalog.Get(item.Building).AffectedIndustry == MortalIndustryType.Commerce)
                .Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity) * coverage;

            var netOutput = state.NetOutput with
            {
                AgricultureUnits = (int)Math.Round(state.NetOutput.AgricultureUnits * (1m + agricultureBonus), 0, MidpointRounding.AwayFromZero),
                HandicraftsUnits = (int)Math.Round(state.NetOutput.HandicraftsUnits * (1m + handicraftsBonus), 0, MidpointRounding.AwayFromZero),
                CommerceUnits = (int)Math.Round(state.NetOutput.CommerceUnits * (1m + commerceBonus), 0, MidpointRounding.AwayFromZero)
            };

            return state with
            {
                NetOutput = netOutput,
                PurchasableSurplus = netOutput
            };
        }
    }
}
