using System.Collections.Immutable;

namespace CoCity.Foundation.Services
{
    public static class MinistryPolicyCatalog
    {
        public static IReadOnlyList<MinistryAuthorityProfile> GetAuthorities(string ministryId)
            => ministryId switch
            {
                "ministry.personnel" => ImmutableArray.Create(
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Narrow delegation",
                        EscalationRule: "Only small routine petitions are cleared below the throne.",
                        Responsibilities: ImmutableArray.Create(
                            "Review sect petitions",
                            "Maintain official rosters",
                            "Track regional appointments")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Moderate delegation",
                        EscalationRule: "Unusual sect growth and sensitive appointments are escalated to the throne.",
                        Responsibilities: ImmutableArray.Create(
                            "Review sect petitions",
                            "Maintain official rosters",
                            "Track regional appointments")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Broad delegation",
                        EscalationRule: "Only strategic expansion disputes and contested appointments reach the throne.",
                        Responsibilities: ImmutableArray.Create(
                            "Review sect petitions",
                            "Maintain official rosters",
                            "Track regional appointments"))),
                "ministry.revenue" => ImmutableArray.Create(
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Moderate delegation",
                        EscalationRule: "Large emergency draws and destabilizing levies require direct approval.",
                        Responsibilities: ImmutableArray.Create(
                            "Track treasury reserves",
                            "Assess tax obligations",
                            "Record local remittances")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Broad delegation",
                        EscalationRule: "Revenue shocks and emergency treasury draws require direct approval.",
                        Responsibilities: ImmutableArray.Create(
                            "Track treasury reserves",
                            "Assess tax obligations",
                            "Record local remittances")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Emergency delegation",
                        EscalationRule: "Only realm-wide fiscal crises bypass the ministry.",
                        Responsibilities: ImmutableArray.Create(
                            "Track treasury reserves",
                            "Assess tax obligations",
                            "Record local remittances"))),
                "ministry.rites" => ImmutableArray.Create(
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Narrow delegation",
                        EscalationRule: "Disputes that could shift sect standing are elevated immediately.",
                        Responsibilities: ImmutableArray.Create(
                            "Maintain sect protocol",
                            "Track diplomatic standing",
                            "Prepare ceremonial notices")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Moderate delegation",
                        EscalationRule: "Only factional disputes with military or economic implications reach the throne.",
                        Responsibilities: ImmutableArray.Create(
                            "Maintain sect protocol",
                            "Track diplomatic standing",
                            "Prepare ceremonial notices")),
                    new MinistryAuthorityProfile(
                        DelegationLevel: "Broad delegation",
                        EscalationRule: "Only realm-threatening sect disputes bypass ministry mediation.",
                        Responsibilities: ImmutableArray.Create(
                            "Maintain sect protocol",
                            "Track diplomatic standing",
                            "Prepare ceremonial notices"))),
                _ => throw new ArgumentOutOfRangeException(nameof(ministryId), ministryId, "Unknown ministry id.")
            };

        public static IReadOnlyList<HandlingStandardProfile> GetStandards(string ministryId)
            => ministryId switch
            {
                "ministry.personnel" => ImmutableArray.Create(
                    new HandlingStandardProfile(
                        Name: "Conservative review",
                        Summary: "Routine requests can be screened quickly, but rapid expansion is treated as a strategic risk."),
                    new HandlingStandardProfile(
                        Name: "Balanced review",
                        Summary: "Routine sect growth is processed steadily while sensitive projects still receive throne oversight."),
                    new HandlingStandardProfile(
                        Name: "Expansion charter",
                        Summary: "Growth petitions are favored unless they risk destabilizing the realm.")),
                "ministry.revenue" => ImmutableArray.Create(
                    new HandlingStandardProfile(
                        Name: "Balanced collection",
                        Summary: "Treasury growth matters, but extraction should not destabilize frontier or heartland settlements."),
                    new HandlingStandardProfile(
                        Name: "Aggressive extraction",
                        Summary: "Revenue targets are prioritized and only severe unrest triggers relief."),
                    new HandlingStandardProfile(
                        Name: "Stability relief",
                        Summary: "Collection is moderated to preserve local order and ease pressure on strained towns.")),
                "ministry.rites" => ImmutableArray.Create(
                    new HandlingStandardProfile(
                        Name: "Precautionary mediation",
                        Summary: "Minor etiquette issues are settled quietly, but factional tension is documented for the ruler."),
                    new HandlingStandardProfile(
                        Name: "Formal mediation",
                        Summary: "Routine disputes are mediated through protocol before escalating to the throne."),
                    new HandlingStandardProfile(
                        Name: "Conciliatory outreach",
                        Summary: "The ministry proactively smooths sect friction before it hardens into disloyalty.")),
                _ => throw new ArgumentOutOfRangeException(nameof(ministryId), ministryId, "Unknown ministry id.")
            };

        public static MinistryAuthorityProfile NextAuthority(string ministryId, MinistryAuthorityProfile current)
            => CycleProfile(GetAuthorities(ministryId), current, profile => profile.DelegationLevel);

        public static HandlingStandardProfile NextStandard(string ministryId, HandlingStandardProfile current)
            => CycleProfile(GetStandards(ministryId), current, profile => profile.Name);

        public static decimal GetDelegationModifier(MinistryAuthorityProfile authority)
            => authority.DelegationLevel switch
            {
                "Broad delegation" => 0.05m,
                "Emergency delegation" => 0.08m,
                "Narrow delegation" => -0.05m,
                _ => 0m
            };

        public static decimal GetStandardThresholdModifier(HandlingStandardProfile standard)
            => standard.Name switch
            {
                "Conservative review" => 0.05m,
                "Precautionary mediation" => 0.08m,
                "Aggressive extraction" => -0.04m,
                "Stability relief" => 0.03m,
                "Expansion charter" => -0.05m,
                "Conciliatory outreach" => -0.04m,
                _ => 0m
            };

        private static T CycleProfile<T>(
            IReadOnlyList<T> options,
            T current,
            Func<T, string> keySelector)
        {
            var currentIndex = options
                .Select((option, index) => (Option: option, Index: index))
                .FirstOrDefault(item => keySelector(item.Option) == keySelector(current))
                .Index;

            return options[(currentIndex + 1) % options.Count];
        }
    }
}
