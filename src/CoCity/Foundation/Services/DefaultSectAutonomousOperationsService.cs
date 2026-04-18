using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultSectAutonomousOperationsService : ISectAutonomousOperationsService
    {
        private readonly IMortalIndustrySimulationService _industryService;

        public DefaultSectAutonomousOperationsService(IMortalIndustrySimulationService industryService)
        {
            _industryService = industryService;
        }

        public SectOperationsTurnResult Step(
            RealmState foundation,
            MortalRealmState realmState,
            IReadOnlyList<MortalTownIndustryState> currentIndustryStates)
        {
            ArgumentNullException.ThrowIfNull(foundation);
            ArgumentNullException.ThrowIfNull(realmState);
            ArgumentNullException.ThrowIfNull(currentIndustryStates);

            var townIdsByRegion = foundation.Regions.ToDictionary(
                region => region.Id,
                region => region.TownIds.OrderBy(townId => townId).ToImmutableArray());
            var sectDefinitionsById = foundation.Sects.ToDictionary(sect => sect.Id);
            var industryStatesByTownId = currentIndustryStates.ToDictionary(state => state.TownId);

            var operationContexts = realmState.Sects
                .Select(sect =>
                {
                    if (!sectDefinitionsById.TryGetValue(sect.SectId, out var definition))
                    {
                        throw new InvalidOperationException($"Unknown sect definition '{sect.SectId}'.");
                    }

                    if (!townIdsByRegion.TryGetValue(sect.RegionId, out var townIds))
                    {
                        throw new InvalidOperationException($"Unknown region '{sect.RegionId}' for sect '{sect.SectId}'.");
                    }

                    return CreateOperationContext(sect, definition, townIds, industryStatesByTownId);
                })
                .ToImmutableArray();

            var purchaseActors = operationContexts
                .Select(context => new SectState(
                    Id: context.Sect.SectId,
                    RegionId: context.Sect.RegionId,
                    Name: context.Sect.SectName,
                    Funds: context.FundsAvailableForPurchases,
                    Population: context.Sect.CurrentPopulation,
                    Loyalty: context.Sect.Loyalty,
                    IndustryPreference: context.Sect.IndustryPreference,
                    RecruitmentWage: context.Sect.RecruitmentWage,
                    Output: context.Definition.Output))
                .ToImmutableArray();
            var purchaseRequests = operationContexts
                .SelectMany(context => context.PurchaseRequests)
                .ToImmutableArray();

            var purchaseResult = _industryService.ProcessPurchases(
                currentIndustryStates,
                purchaseActors,
                purchaseRequests);

            var purchaseSettlementsBySectId = purchaseResult.Settlements.ToDictionary(settlement => settlement.SectId);
            var purchasedUnitsBySectId = purchaseResult.Report.Receipts
                .GroupBy(receipt => receipt.SectId)
                .ToDictionary(group => group.Key, group => group.Sum(receipt => receipt.PurchasedQuantity));

            var nextSects = operationContexts
                .Select(context =>
                {
                    var settlement = purchaseSettlementsBySectId[context.Sect.SectId];
                    var purchasedUnits = purchasedUnitsBySectId.GetValueOrDefault(context.Sect.SectId);
                    var outputFactor = CalculateOutputFactor(context.Profile, context.UpkeepPaid, purchasedUnits);

                    return context.Sect with
                    {
                        CurrentFunds = context.FundsBefore - context.UpkeepPaid - settlement.FundsSpent,
                        CurrentOutput = ScaleOutput(context.Definition.Output, outputFactor)
                    };
                })
                .ToImmutableArray();

            var sectEvents = operationContexts
                .Select(context =>
                {
                    var settlement = purchaseSettlementsBySectId[context.Sect.SectId];
                    var purchasedUnits = purchasedUnitsBySectId.GetValueOrDefault(context.Sect.SectId);
                    var outputFactor = CalculateOutputFactor(context.Profile, context.UpkeepPaid, purchasedUnits);

                    return new SectOperationEvent(
                        SectId: context.Sect.SectId,
                        SectName: context.Sect.SectName,
                        InputIndustry: context.Profile.InputIndustry,
                        RequestedUnits: context.Profile.RequiredUnitsPerTurn,
                        PurchasedUnits: purchasedUnits,
                        UpkeepPaid: context.UpkeepPaid,
                        InputPurchaseCost: settlement.FundsSpent,
                        FundsBefore: context.FundsBefore,
                        FundsAfter: context.FundsBefore - context.UpkeepPaid - settlement.FundsSpent,
                        OutputFactor: outputFactor,
                        OperationSummary: DescribeOperationSummary(context.Profile, context.UpkeepPaid, purchasedUnits, settlement.FundsRemaining, outputFactor));
                })
                .ToImmutableArray();

            return new SectOperationsTurnResult(
                NextSects: nextSects,
                NextIndustryStates: purchaseResult.NextStates,
                Report: new SectOperationsTurnReport(
                    SectEvents: sectEvents,
                    PurchaseReport: purchaseResult.Report));
        }

        private static SectOperationContext CreateOperationContext(
            SectSimulationState sect,
            SectState definition,
            IReadOnlyList<string> townIds,
            IReadOnlyDictionary<string, MortalTownIndustryState> industryStatesByTownId)
        {
            var profile = SectOperationsCatalog.Get(sect.SectId);
            var fundsBefore = sect.CurrentFunds;
            var upkeepPaid = Math.Min(fundsBefore, profile.UpkeepCost);
            var fundsAvailableForPurchases = Math.Max(0m, fundsBefore - upkeepPaid);
            var purchaseRequests = BuildPurchaseRequests(sect, townIds, industryStatesByTownId, profile);

            return new SectOperationContext(
                Sect: sect,
                Definition: definition,
                Profile: profile,
                FundsBefore: fundsBefore,
                UpkeepPaid: upkeepPaid,
                FundsAvailableForPurchases: fundsAvailableForPurchases,
                PurchaseRequests: purchaseRequests);
        }

        private static ImmutableArray<SectPurchaseRequest> BuildPurchaseRequests(
            SectSimulationState sect,
            IReadOnlyList<string> townIds,
            IReadOnlyDictionary<string, MortalTownIndustryState> industryStatesByTownId,
            SectOperationsProfile profile)
        {
            var requests = ImmutableArray.CreateBuilder<SectPurchaseRequest>();
            var remainingUnits = profile.RequiredUnitsPerTurn;

            foreach (var townId in townIds)
            {
                if (remainingUnits == 0)
                {
                    break;
                }

                if (!industryStatesByTownId.TryGetValue(townId, out var townState))
                {
                    throw new InvalidOperationException($"Unknown town industry state '{townId}'.");
                }

                var availableUnits = GetIndustryUnits(townState.PurchasableSurplus, profile.InputIndustry);
                var requestedUnits = Math.Min(remainingUnits, availableUnits);
                if (requestedUnits == 0)
                {
                    continue;
                }

                requests.Add(new SectPurchaseRequest(
                    SectId: sect.SectId,
                    TownId: townId,
                    Industry: profile.InputIndustry,
                    Quantity: requestedUnits,
                    UnitPrice: profile.UnitPurchasePrice));
                remainingUnits -= requestedUnits;
            }

            return requests.ToImmutable();
        }

        private static int GetIndustryUnits(IndustryOutput output, MortalIndustryType industry)
            => industry switch
            {
                MortalIndustryType.Agriculture => output.AgricultureUnits,
                MortalIndustryType.Handicrafts => output.HandicraftsUnits,
                MortalIndustryType.Commerce => output.CommerceUnits,
                _ => throw new ArgumentOutOfRangeException(nameof(industry), industry, "Unknown industry type.")
            };

        private static decimal CalculateOutputFactor(
            SectOperationsProfile profile,
            decimal upkeepPaid,
            int purchasedUnits)
        {
            var upkeepCoverage = profile.UpkeepCost <= 0
                ? 1m
                : Math.Min(1m, upkeepPaid / profile.UpkeepCost);
            var inputCoverage = profile.RequiredUnitsPerTurn <= 0
                ? 1m
                : Math.Min(1m, (decimal)purchasedUnits / profile.RequiredUnitsPerTurn);

            return Math.Min(upkeepCoverage, inputCoverage);
        }

        private static ImmutableArray<OutputMetric> ScaleOutput(
            IReadOnlyList<OutputMetric> baselineOutput,
            decimal outputFactor)
            => baselineOutput
                .Select(metric => metric with
                {
                    Amount = Math.Round(metric.Amount * outputFactor, 1, MidpointRounding.AwayFromZero)
                })
                .ToImmutableArray();

        private static string DescribeOperationSummary(
            SectOperationsProfile profile,
            decimal upkeepPaid,
            int purchasedUnits,
            decimal fundsRemainingForInputs,
            decimal outputFactor)
        {
            if (outputFactor >= 1m)
            {
                return "Upkeep and raw materials fully supported baseline output.";
            }

            if (upkeepPaid < profile.UpkeepCost && purchasedUnits == 0)
            {
                return "Funds were exhausted on partial upkeep before raw materials could be secured.";
            }

            if (purchasedUnits < profile.RequiredUnitsPerTurn && fundsRemainingForInputs == 0)
            {
                return "Funds ran out before enough raw materials could be purchased.";
            }

            if (purchasedUnits < profile.RequiredUnitsPerTurn)
            {
                return "Regional industry surplus could not fully cover sect raw-material demand.";
            }

            if (upkeepPaid < profile.UpkeepCost)
            {
                return "Partial upkeep reduced sect operating efficiency.";
            }

            return "Sect operations ran below baseline this turn.";
        }

        private sealed record SectOperationContext(
            SectSimulationState Sect,
            SectState Definition,
            SectOperationsProfile Profile,
            decimal FundsBefore,
            decimal UpkeepPaid,
            decimal FundsAvailableForPurchases,
            ImmutableArray<SectPurchaseRequest> PurchaseRequests);
    }
}
