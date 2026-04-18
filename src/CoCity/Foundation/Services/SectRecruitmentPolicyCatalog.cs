using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed record RecruitmentWagePolicy(
        string DisplayName,
        decimal WagePerRecruit,
        int TargetRecruitsPerTurn);

    public static class SectRecruitmentPolicyCatalog
    {
        public static RecruitmentWagePolicy Get(RecruitmentWageLevel wageLevel)
            => wageLevel switch
            {
                RecruitmentWageLevel.Frugal => new(
                    DisplayName: "Frugal",
                    WagePerRecruit: 6m,
                    TargetRecruitsPerTurn: 20),
                RecruitmentWageLevel.Standard => new(
                    DisplayName: "Standard",
                    WagePerRecruit: 9m,
                    TargetRecruitsPerTurn: 35),
                RecruitmentWageLevel.Generous => new(
                    DisplayName: "Generous",
                    WagePerRecruit: 12m,
                    TargetRecruitsPerTurn: 50),
                _ => throw new ArgumentOutOfRangeException(nameof(wageLevel), wageLevel, "Unknown recruitment wage level.")
            };
    }
}
