namespace CoCity.Foundation.Services
{
    public sealed class DefaultTurnAdvancementPipelineService : ITurnAdvancementPipelineService
    {
        private readonly IMortalRealmSimulationService _realmSimulationService;
        private readonly IMortalIndustrySimulationService _industrySimulationService;
        private readonly ISectAutonomousOperationsService _sectOperationsService;
        private readonly IBuildingSystemService _buildingSystemService;
        private readonly IMortalTaxationSimulationService _taxationService;
        private readonly IMinistryFrameworkService _ministryFrameworkService;

        public DefaultTurnAdvancementPipelineService(
            IMortalRealmSimulationService realmSimulationService,
            IMortalIndustrySimulationService industrySimulationService,
            ISectAutonomousOperationsService sectOperationsService,
            IBuildingSystemService buildingSystemService,
            IMortalTaxationSimulationService taxationService,
            IMinistryFrameworkService ministryFrameworkService)
        {
            _realmSimulationService = realmSimulationService;
            _industrySimulationService = industrySimulationService;
            _sectOperationsService = sectOperationsService;
            _buildingSystemService = buildingSystemService;
            _taxationService = taxationService;
            _ministryFrameworkService = ministryFrameworkService;
        }

        public TurnAdvancementResult Advance(
            RealmState foundation,
            TurnAdvancementState currentState)
        {
            var realmResult = _realmSimulationService.Step(
                foundation,
                currentState.RealmState,
                currentState.TaxationState.SelectedTaxRate);
            var industryResult = _industrySimulationService.Step(
                foundation,
                foundation.Ministries,
                realmResult.NextState,
                currentState.IndustryStates);
            var sectOperationsResult = _sectOperationsService.Step(
                foundation,
                realmResult.NextState,
                industryResult.NextStates);
            var buildingResult = _buildingSystemService.ApplyTurn(
                foundation,
                currentState.BuildingState,
                sectOperationsResult.NextSects,
                sectOperationsResult.NextIndustryStates,
                currentState.TaxationState.CurrentTreasuryFunds);

            var advancedRealmState = realmResult.NextState with
            {
                Sects = buildingResult.NextSects
            };

            var taxationSeedState = currentState.TaxationState with
            {
                CurrentTreasuryFunds = buildingResult.NextTreasuryFunds
            };
            var taxationResult = _taxationService.Step(
                taxationSeedState,
                advancedRealmState,
                buildingResult.NextIndustryStates);
            var ministryResult = _ministryFrameworkService.Step(
                foundation,
                currentState.MinistryState,
                advancedRealmState,
                buildingResult.NextState,
                taxationResult.NextState);

            return new TurnAdvancementResult(
                NextState: new TurnAdvancementState(
                    RealmState: advancedRealmState,
                    BuildingState: buildingResult.NextState,
                    IndustryStates: buildingResult.NextIndustryStates,
                    TaxationState: taxationResult.NextState,
                    MinistryState: ministryResult.NextState),
                Report: new TurnAdvancementReport(
                    RealmReport: realmResult.Report,
                    IndustryReport: industryResult.Report,
                    SectOperationsReport: sectOperationsResult.Report,
                    BuildingReport: buildingResult.Report,
                    TaxationReport: taxationResult.Report,
                    MinistryReport: ministryResult.Report));
        }
    }
}
