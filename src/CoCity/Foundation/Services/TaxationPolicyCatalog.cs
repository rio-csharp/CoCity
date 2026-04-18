namespace CoCity.Foundation.Services
{
    public sealed record TaxRatePolicy(
        TaxRateLevel Rate,
        string DisplayName,
        decimal RevenueMultiplier,
        decimal GrowthRateModifier,
        decimal DeclineRateModifier,
        int StabilityDelta,
        string StabilitySummary,
        string ChangeReasonSuffix);

    public static class TaxationPolicyCatalog
    {
        public static TaxRatePolicy Get(TaxRateLevel rate)
            => rate switch
            {
                TaxRateLevel.Light => new TaxRatePolicy(
                    Rate: TaxRateLevel.Light,
                    DisplayName: "Light",
                    RevenueMultiplier: 0.85m,
                    GrowthRateModifier: 0.0025m,
                    DeclineRateModifier: -0.0025m,
                    StabilityDelta: 6,
                    StabilitySummary: "Tax relief supports mortal confidence.",
                    ChangeReasonSuffix: "tax relief"),
                TaxRateLevel.Standard => new TaxRatePolicy(
                    Rate: TaxRateLevel.Standard,
                    DisplayName: "Standard",
                    RevenueMultiplier: 1.0m,
                    GrowthRateModifier: 0m,
                    DeclineRateModifier: 0m,
                    StabilityDelta: 0,
                    StabilitySummary: "Standard levy keeps mortal confidence steady.",
                    ChangeReasonSuffix: string.Empty),
                TaxRateLevel.Heavy => new TaxRatePolicy(
                    Rate: TaxRateLevel.Heavy,
                    DisplayName: "Heavy",
                    RevenueMultiplier: 1.2m,
                    GrowthRateModifier: -0.004m,
                    DeclineRateModifier: 0.004m,
                    StabilityDelta: -8,
                    StabilitySummary: "Heavy levy strains mortal confidence.",
                    ChangeReasonSuffix: "tax strain"),
                _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, null)
            };

        public static TaxRateLevel Raise(TaxRateLevel rate)
            => rate switch
            {
                TaxRateLevel.Light => TaxRateLevel.Standard,
                TaxRateLevel.Standard => TaxRateLevel.Heavy,
                TaxRateLevel.Heavy => TaxRateLevel.Heavy,
                _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, null)
            };

        public static TaxRateLevel Lower(TaxRateLevel rate)
            => rate switch
            {
                TaxRateLevel.Heavy => TaxRateLevel.Standard,
                TaxRateLevel.Standard => TaxRateLevel.Light,
                TaxRateLevel.Light => TaxRateLevel.Light,
                _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, null)
            };
    }
}
