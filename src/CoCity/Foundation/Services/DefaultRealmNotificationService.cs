using System.Collections.Immutable;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultRealmNotificationService : IRealmNotificationService
    {
        public RealmNotificationState Build(RealmNotificationContext context)
        {
            var alerts = ImmutableArray.CreateBuilder<RealmNotificationItem>();
            var recentEvents = ImmutableArray.CreateBuilder<RealmNotificationItem>();

            AddExpansionRequestAlerts(context, alerts);
            AddStabilityAlerts(context, alerts);
            AddLowTreasuryAlert(context, alerts);
            AddPopulationDeclineEvents(context, recentEvents);
            AddLoyaltyDeclineEvents(context, recentEvents);
            AddMinistryReportEvents(context, recentEvents);

            var orderedAlerts = alerts
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Title)
                .ToImmutableArray();
            var orderedRecentEvents = recentEvents
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Title)
                .ToImmutableArray();

            return new RealmNotificationState(
                Alerts: orderedAlerts,
                RecentEvents: orderedRecentEvents,
                Summary: $"Alerts: {orderedAlerts.Length} | Recent events: {orderedRecentEvents.Length}");
        }

        private static void AddExpansionRequestAlerts(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder alerts)
        {
            foreach (var ministry in context.MinistryState.Ministries.OrderBy(item => item.MinistryId))
            {
                foreach (var escalation in ministry.PendingEscalations
                             .Where(item => item.CaseType == MinistryCaseType.SectApplication)
                             .OrderBy(item => item.CaseId))
                {
                    alerts.Add(new RealmNotificationItem(
                        Id: $"alert:expansion:{escalation.CaseId}",
                        Severity: RealmNotificationSeverity.Warning,
                        Category: RealmNotificationCategory.ExpansionRequest,
                        Title: $"Sect expansion request: {escalation.SubjectName}",
                        Summary: $"{ministry.MinistryName} is holding an expansion request that needs a ruling. {escalation.Reason}"));
                }
            }
        }

        private static void AddStabilityAlerts(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder alerts)
        {
            foreach (var town in context.TaxationState.Towns
                         .Where(item => item.StabilityDelta < 0)
                         .OrderBy(item => item.TownId))
            {
                alerts.Add(new RealmNotificationItem(
                    Id: $"alert:stability:{town.TownId}",
                    Severity: RealmNotificationSeverity.Warning,
                    Category: RealmNotificationCategory.Stability,
                    Title: $"Stability pressure in {town.TownName}",
                    Summary: $"Current policy applies {Math.Abs(town.StabilityDelta)} stability pressure in {town.TownName}. {town.StabilitySummary}"));
            }
        }

        private static void AddLowTreasuryAlert(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder alerts)
        {
            var threshold = Math.Max(20000m, context.Foundation.Treasury.BaselineTaxIncome * 2m);
            if (context.TaxationState.CurrentTreasuryFunds > threshold)
            {
                return;
            }

            alerts.Add(new RealmNotificationItem(
                Id: "alert:treasury:low",
                Severity: RealmNotificationSeverity.Warning,
                Category: RealmNotificationCategory.Treasury,
                Title: "National treasury is running low",
                Summary: $"Treasury reserves are down to {context.TaxationState.CurrentTreasuryFunds:N0} taels, below the baseline safety threshold of {threshold:N0} taels."));
        }

        private static void AddPopulationDeclineEvents(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder recentEvents)
        {
            if (context.PreviousRealmState is null)
            {
                return;
            }

            var previousTownsById = context.PreviousRealmState.Towns.ToDictionary(item => item.TownId);
            foreach (var town in context.CurrentRealmState.Towns.OrderBy(item => item.TownId))
            {
                if (!previousTownsById.TryGetValue(town.TownId, out var previousTown) ||
                    town.CurrentPopulation >= previousTown.CurrentPopulation)
                {
                    continue;
                }

                recentEvents.Add(new RealmNotificationItem(
                    Id: $"event:population:{town.TownId}:{context.CurrentRealmState.TurnNumber}",
                    Severity: RealmNotificationSeverity.Warning,
                    Category: RealmNotificationCategory.Population,
                    Title: $"Population declined in {town.TownName}",
                    Summary: $"{town.TownName} fell from {previousTown.CurrentPopulation:N0} to {town.CurrentPopulation:N0} people this update."));
            }
        }

        private static void AddLoyaltyDeclineEvents(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder recentEvents)
        {
            if (context.PreviousRealmState is null)
            {
                return;
            }

            var previousSectsById = context.PreviousRealmState.Sects.ToDictionary(item => item.SectId);
            foreach (var sect in context.CurrentRealmState.Sects.OrderBy(item => item.SectId))
            {
                if (!previousSectsById.TryGetValue(sect.SectId, out var previousSect) ||
                    sect.Loyalty >= previousSect.Loyalty)
                {
                    continue;
                }

                recentEvents.Add(new RealmNotificationItem(
                    Id: $"event:loyalty:{sect.SectId}:{context.CurrentRealmState.TurnNumber}",
                    Severity: RealmNotificationSeverity.Warning,
                    Category: RealmNotificationCategory.Loyalty,
                    Title: $"Sect loyalty declined: {sect.SectName}",
                    Summary: $"{sect.SectName} loyalty fell from {previousSect.Loyalty} to {sect.Loyalty}."));
            }
        }

        private static void AddMinistryReportEvents(
            RealmNotificationContext context,
            ImmutableArray<RealmNotificationItem>.Builder recentEvents)
        {
            if (context.TurnReport is null)
            {
                return;
            }

            foreach (var ministryEvent in context.TurnReport.MinistryReport.MinistryEvents.OrderBy(item => item.MinistryId))
            {
                recentEvents.Add(new RealmNotificationItem(
                    Id: $"event:ministry:{ministryEvent.MinistryId}:{context.TurnReport.MinistryReport.TurnNumber}",
                    Severity: RealmNotificationSeverity.Info,
                    Category: RealmNotificationCategory.Ministry,
                    Title: $"Ministry report: {ministryEvent.MinistryName}",
                    Summary: $"{ministryEvent.ProcessedCases} processed | {ministryEvent.EscalatedCases} escalated | {Math.Round(ministryEvent.AutomationSuccessRate * 100m, 0, MidpointRounding.AwayFromZero)}% success. {ministryEvent.Summary}"));
            }
        }
    }
}
