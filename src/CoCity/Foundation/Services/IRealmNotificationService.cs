namespace CoCity.Foundation.Services
{
    public interface IRealmNotificationService
    {
        RealmNotificationState Build(RealmNotificationContext context);
    }

    public sealed record RealmNotificationContext(
        RealmState Foundation,
        MortalRealmState CurrentRealmState,
        RealmTaxationState TaxationState,
        RealmMinistryState MinistryState,
        TurnAdvancementReport? TurnReport,
        MortalRealmState? PreviousRealmState = null);

    public enum RealmNotificationSeverity
    {
        Info,
        Warning
    }

    public enum RealmNotificationCategory
    {
        ExpansionRequest,
        Population,
        Stability,
        Treasury,
        Loyalty,
        Ministry
    }

    public sealed record RealmNotificationItem(
        string Id,
        RealmNotificationSeverity Severity,
        RealmNotificationCategory Category,
        string Title,
        string Summary);

    public sealed record RealmNotificationState(
        IReadOnlyList<RealmNotificationItem> Alerts,
        IReadOnlyList<RealmNotificationItem> RecentEvents,
        string Summary);
}
