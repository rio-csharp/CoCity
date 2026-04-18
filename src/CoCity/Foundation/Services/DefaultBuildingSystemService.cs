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
                    .Select(sect => new SectBuildingInventoryState(sect.Id, [], null))
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
                    if (inventory.ActiveProject is not null)
                    {
                        return sect;
                    }

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
                        ActiveProject = new SectBuildingProject(nextBuilding.Value, definition.BuildTimeTurns)
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
                    var project = sectInventories[pair.Next.SectId].ActiveProject!;
                    var definition = BuildingCatalog.Get(project.Building);
                    return new BuildingConstructionEvent(
                        OwnerName: pair.Next.SectName,
                        BuildingName: definition.DisplayName,
                        ConstructionCost: definition.ConstructionCost,
                        Summary: $"Started {definition.DisplayName}; completion in {project.TurnsRemaining} turn(s).");
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

            var sectConstructionEvents = ImmutableArray.CreateBuilder<BuildingConstructionEvent>();
            var updatedSectInventories = new Dictionary<string, SectBuildingInventoryState>();
            var fundedSects = new List<SectSimulationState>();
            foreach (var sect in sects)
            {
                var advancedInventory = AdvanceSectProject(sectInventories[sect.SectId], out var completedBuilding);
                var upkeepRequired = advancedInventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                var upkeepPaid = Math.Min(sect.CurrentFunds, upkeepRequired);
                var upkeepCoverage = upkeepRequired <= 0 ? 1m : Math.Min(1m, upkeepPaid / upkeepRequired);
                var outputBonus = advancedInventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity);
                var multiplier = 1m + (outputBonus * upkeepCoverage);
                var currentSect = sect with
                {
                    CurrentFunds = sect.CurrentFunds - upkeepPaid,
                    CurrentOutput = ScaleOutput(sect.CurrentOutput, multiplier)
                };

                if (completedBuilding is not null)
                {
                    var completedDefinition = BuildingCatalog.Get(completedBuilding.Value);
                    sectConstructionEvents.Add(new BuildingConstructionEvent(
                        OwnerName: sect.SectName,
                        BuildingName: completedDefinition.DisplayName,
                        ConstructionCost: 0m,
                        Summary: $"Completed {completedDefinition.DisplayName}."));
                }

                if (advancedInventory.ActiveProject is null)
                {
                    var nextBuilding = GetNextSectBuilding(advancedInventory.Buildings);
                    if (nextBuilding is not null)
                    {
                        var definition = BuildingCatalog.Get(nextBuilding.Value);
                        if (currentSect.CurrentFunds >= definition.ConstructionCost)
                        {
                            currentSect = currentSect with { CurrentFunds = currentSect.CurrentFunds - definition.ConstructionCost };
                            advancedInventory = advancedInventory with
                            {
                                ActiveProject = new SectBuildingProject(nextBuilding.Value, definition.BuildTimeTurns)
                            };
                            sectConstructionEvents.Add(new BuildingConstructionEvent(
                                OwnerName: sect.SectName,
                                BuildingName: definition.DisplayName,
                                ConstructionCost: definition.ConstructionCost,
                                Summary: $"Auto-started {definition.DisplayName}; completion in {definition.BuildTimeTurns} turn(s)."));
                        }
                    }
                }

                updatedSectInventories[sect.SectId] = advancedInventory;
                fundedSects.Add(currentSect);
            }

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

            var operationEvents = fundedSects
                .Select(sect =>
                {
                    var inventory = updatedSectInventories[sect.SectId];
                    var originalSect = sects.Single(current => current.SectId == sect.SectId);
                    var upkeepRequired = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).UpkeepCost * item.Quantity);
                    var upkeepPaid = Math.Min(originalSect.CurrentFunds, upkeepRequired);
                    var outputBonus = inventory.Buildings.Sum(item => BuildingCatalog.Get(item.Building).OutputBonus * item.Quantity);
                    var coverage = upkeepRequired <= 0 ? 1m : Math.Min(1m, upkeepPaid / upkeepRequired);
                    return new BuildingOperationEvent(
                        OwnerName: sect.SectName,
                        Summary: inventory.Buildings.Count == 0 && inventory.ActiveProject is null
                            ? "No sect buildings constructed yet."
                            : $"Building upkeep {upkeepPaid} taels | output modifier +{Math.Round(outputBonus * coverage * 100m, 0, MidpointRounding.AwayFromZero)}% | active project: {FormatProject(inventory.ActiveProject)}.");
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
                NextState: currentState with { Sects = updatedSectInventories.Values.OrderBy(item => item.SectId).ToImmutableArray() },
                NextSects: fundedSects.ToImmutableArray(),
                NextIndustryStates: nextIndustryStates,
                NextTreasuryFunds: treasury,
                Report: new BuildingReport(sectConstructionEvents.ToImmutable(), operationEvents));
        }

        private static SectBuildingType? GetNextSectBuilding(IReadOnlyList<SectBuildingCount> buildings)
            => BuildingCatalog.SectBuildOrder
                .Cast<SectBuildingType?>()
                .FirstOrDefault(building =>
                {
                    if (building is null)
                    {
                        return false;
                    }

                    var builtCount = buildings.FirstOrDefault(item => item.Building == building.Value)?.Quantity ?? 0;
                    return builtCount < BuildingCatalog.Get(building.Value).QuantityCap;
                });

        private static MortalBuildingType? GetNextTownBuilding(IReadOnlyList<MortalBuildingCount> buildings)
            => BuildingCatalog.MortalBuildOrder
                .Cast<MortalBuildingType?>()
                .FirstOrDefault(building => building is not null && buildings.All(item => item.Building != building.Value));

        private static ImmutableArray<SectBuildingCount> AddSectBuilding(
            IReadOnlyList<SectBuildingCount> buildings,
            SectBuildingType building)
        {
            var existing = buildings.FirstOrDefault(item => item.Building == building);
            return existing is null
                ? buildings.Concat([new SectBuildingCount(building, 1)]).ToImmutableArray()
                : buildings.Select(item => item.Building == building ? item with { Quantity = item.Quantity + 1 } : item).ToImmutableArray();
        }

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

        private static SectBuildingInventoryState AdvanceSectProject(
            SectBuildingInventoryState inventory,
            out SectBuildingType? completedBuilding)
        {
            completedBuilding = null;
            if (inventory.ActiveProject is null)
            {
                return inventory;
            }

            var remainingTurns = inventory.ActiveProject.TurnsRemaining - 1;
            if (remainingTurns > 0)
            {
                return inventory with
                {
                    ActiveProject = inventory.ActiveProject with { TurnsRemaining = remainingTurns }
                };
            }

            completedBuilding = inventory.ActiveProject.Building;
            return inventory with
            {
                Buildings = AddSectBuilding(inventory.Buildings, inventory.ActiveProject.Building),
                ActiveProject = null
            };
        }

        private static string FormatProject(SectBuildingProject? project)
            => project is null
                ? "None"
                : $"{BuildingCatalog.Get(project.Building).DisplayName} ({project.TurnsRemaining} turn(s) remaining)";
    }
}
