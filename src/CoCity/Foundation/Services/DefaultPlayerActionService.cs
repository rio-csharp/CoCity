using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultPlayerActionService : IPlayerActionService
    {
        private readonly IMinistryFrameworkService _ministryFrameworkService;
        private readonly IMortalTaxationSimulationService _taxationService;

        public DefaultPlayerActionService(
            IMinistryFrameworkService ministryFrameworkService,
            IMortalTaxationSimulationService taxationService)
        {
            _ministryFrameworkService = ministryFrameworkService;
            _taxationService = taxationService;
        }

        public RealmMinistryState CycleMinistryAuthority(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            string ministryId)
        {
            var updatedState = new RealmMinistryState(
                ministryState.Ministries
                    .Select(ministry => ministry.MinistryId == ministryId
                        ? ministry with { Authority = MinistryPolicyCatalog.NextAuthority(ministryId, ministry.Authority) }
                        : ministry)
                    .ToImmutableArray());

            return _ministryFrameworkService.Recalculate(foundation, updatedState, mortalRealmState, buildingState, taxationState);
        }

        public RealmMinistryState CycleMinistryStandard(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            string ministryId)
        {
            var updatedState = new RealmMinistryState(
                ministryState.Ministries
                    .Select(ministry => ministry.MinistryId == ministryId
                        ? ministry with { Standard = MinistryPolicyCatalog.NextStandard(ministryId, ministry.Standard) }
                        : ministry)
                    .ToImmutableArray());

            return _ministryFrameworkService.Recalculate(foundation, updatedState, mortalRealmState, buildingState, taxationState);
        }

        public PlayerActionResolutionResult ApproveEscalation(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            string ministryId,
            string caseId)
            => ResolveEscalation(
                foundation,
                ministryState,
                mortalRealmState,
                buildingState,
                taxationState,
                industryStates,
                ministryId,
                caseId,
                approved: true);

        public PlayerActionResolutionResult RejectEscalation(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            string ministryId,
            string caseId)
            => ResolveEscalation(
                foundation,
                ministryState,
                mortalRealmState,
                buildingState,
                taxationState,
                industryStates,
                ministryId,
                caseId,
                approved: false);

        private PlayerActionResolutionResult ResolveEscalation(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            string ministryId,
            string caseId,
            bool approved)
        {
            var ministry = ministryState.Ministries.Single(item => item.MinistryId == ministryId);
            var escalation = ministry.PendingEscalations.Single(item => item.CaseId == caseId);

            var nextRealmState = mortalRealmState;
            var nextBuildingState = buildingState;
            var nextTaxationState = taxationState;
            var nextMinistryState = ministryState;
            string summary;

            switch (escalation.CaseType)
            {
                case MinistryCaseType.SectApplication:
                    if (approved)
                    {
                        nextMinistryState = ApplyApproval(ministryState, ministryId, escalation, "The throne approved the sect expansion request.");
                        summary = $"Approved {escalation.SubjectName}'s expansion request.";
                    }
                    else
                    {
                        nextBuildingState = CancelSectProject(buildingState, escalation.SubjectId);
                        nextMinistryState = ApplyRejection(ministryState, ministryId, escalation, "The throne rejected the sect expansion request.");
                        summary = $"Rejected {escalation.SubjectName}'s expansion request and halted the project.";
                    }
                    break;

                case MinistryCaseType.TaxCollection:
                    if (approved)
                    {
                        nextMinistryState = ApplyApproval(ministryState, ministryId, escalation, "The throne authorized the elevated tax collection plan.");
                        summary = $"Approved the revenue request for {escalation.SubjectName}.";
                    }
                    else
                    {
                        nextTaxationState = _taxationService.SetTaxRate(
                            taxationState,
                            mortalRealmState,
                            industryStates,
                            TaxationPolicyCatalog.Lower(taxationState.SelectedTaxRate));
                        nextMinistryState = ApplyRejection(ministryState, ministryId, escalation, "The throne rejected the elevated revenue request.");
                        summary = $"Rejected the revenue request for {escalation.SubjectName} and lowered the tax rate.";
                    }
                    break;

                case MinistryCaseType.SectDiplomacy:
                    if (approved)
                    {
                        nextRealmState = AdjustSectLoyalty(mortalRealmState, escalation.SubjectId, +3);
                        nextMinistryState = ApplyApproval(ministryState, ministryId, escalation, "The throne endorsed ministry mediation.");
                        summary = $"Approved rites mediation for {escalation.SubjectName}.";
                    }
                    else
                    {
                        nextRealmState = AdjustSectLoyalty(mortalRealmState, escalation.SubjectId, -2);
                        nextMinistryState = ApplyRejection(ministryState, ministryId, escalation, "The throne rejected ministry mediation.");
                        summary = $"Rejected rites mediation for {escalation.SubjectName}; sect loyalty fell.";
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var recalculatedMinistryState = _ministryFrameworkService.Recalculate(
                foundation,
                nextMinistryState,
                nextRealmState,
                nextBuildingState,
                nextTaxationState);

            return new PlayerActionResolutionResult(
                NextMinistryState: recalculatedMinistryState,
                NextRealmState: nextRealmState,
                NextBuildingState: nextBuildingState,
                NextTaxationState: nextTaxationState,
                Summary: summary);
        }

        private static RealmMinistryState ApplyApproval(
            RealmMinistryState ministryState,
            string ministryId,
            MinistryEscalationState escalation,
            string summary)
            => new(
                ministryState.Ministries
                    .Select(ministry => ministry.MinistryId == ministryId
                        ? ministry with
                        {
                            ApprovedCases = ministry.ApprovedCases
                                .Concat([
                                    new MinistryDecisionState(
                                        escalation.CaseId,
                                        escalation.CaseType,
                                        escalation.SubjectId,
                                        escalation.SubjectName,
                                        summary)
                                ])
                                .DistinctBy(item => item.CaseId)
                                .ToImmutableArray()
                        }
                        : ministry)
                    .ToImmutableArray());

        private static RealmMinistryState ApplyRejection(
            RealmMinistryState ministryState,
            string ministryId,
            MinistryEscalationState escalation,
            string summary)
            => new(
                ministryState.Ministries
                    .Select(ministry => ministry.MinistryId == ministryId
                        ? ministry with
                        {
                            RejectedCases = ministry.RejectedCases
                                .Concat([
                                    new MinistryDecisionState(
                                        escalation.CaseId,
                                        escalation.CaseType,
                                        escalation.SubjectId,
                                        escalation.SubjectName,
                                        summary)
                                ])
                                .DistinctBy(item => item.CaseId)
                                .ToImmutableArray()
                        }
                        : ministry)
                    .ToImmutableArray());

        private static RealmBuildingState CancelSectProject(RealmBuildingState buildingState, string sectId)
            => buildingState with
            {
                Sects = buildingState.Sects
                    .Select(item => item.SectId == sectId
                        ? item with { ActiveProject = null }
                        : item)
                    .ToImmutableArray()
            };

        private static MortalRealmState AdjustSectLoyalty(MortalRealmState realmState, string sectId, int delta)
            => realmState with
            {
                Sects = realmState.Sects
                    .Select(sect => sect.SectId == sectId
                        ? sect with { Loyalty = Math.Max(0, sect.Loyalty + delta) }
                        : sect)
                    .ToImmutableArray()
            };
    }
}
