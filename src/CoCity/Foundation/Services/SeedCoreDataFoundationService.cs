using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class SeedCoreDataFoundationService : ICoreDataFoundationService
    {
        public RealmState GetInitialState()
        {
            RegionState[] regions =
            {
                new(
                    Id: "region.azure-river",
                    Name: "Azure River Prefecture",
                    SpiritualVeinStrength: 78,
                    Baseline: new RegionBaselineAttributes(
                        Terrain: "River basin",
                        Climate: "Temperate and fertile",
                        SettlementPattern: "Canal-linked farming towns"),
                    TownIds:
                    [
                        "town.azure-ford",
                        "town.gold-reed-market"
                    ],
                    SectIds:
                    [
                        "sect.azure-talisman-academy"
                    ]),
                new(
                    Id: "region.redcliff-frontier",
                    Name: "Redcliff Frontier",
                    SpiritualVeinStrength: 61,
                    Baseline: new RegionBaselineAttributes(
                        Terrain: "Dry highland frontier",
                        Climate: "Wind-scoured and severe",
                        SettlementPattern: "Fortified trade posts and mining camps"),
                    TownIds:
                    [
                        "town.redcliff-garrison"
                    ],
                    SectIds:
                    [
                        "sect.iron-peak-hall"
                    ]),
                new(
                    Id: "region.mistwood-heartland",
                    Name: "Mistwood Heartland",
                    SpiritualVeinStrength: 88,
                    Baseline: new RegionBaselineAttributes(
                        Terrain: "Forest basin",
                        Climate: "Humid and spirit-rich",
                        SettlementPattern: "Garden cities supported by orchard villages"),
                    TownIds:
                    [
                        "town.mistwood-harbor"
                    ],
                    SectIds:
                    [
                        "sect.verdant-crucible-sect"
                    ])
            };

            MortalTownState[] towns =
            {
                new(
                    Id: "town.azure-ford",
                    RegionId: "region.azure-river",
                    Name: "Azure Ford",
                    Population: 18200,
                    Industries:
                    [
                        new IndustryAllocation(MortalIndustryType.Agriculture, 45),
                        new IndustryAllocation(MortalIndustryType.Handicrafts, 30),
                        new IndustryAllocation(MortalIndustryType.Commerce, 25)
                    ],
                    Output:
                    [
                        new OutputMetric("Grain", 340, "loads"),
                        new OutputMetric("Paper and ink", 96, "bundles"),
                        new OutputMetric("Market surplus", 72, "taels")
                    ]),
                new(
                    Id: "town.gold-reed-market",
                    RegionId: "region.azure-river",
                    Name: "Gold Reed Market",
                    Population: 13400,
                    Industries:
                    [
                        new IndustryAllocation(MortalIndustryType.Agriculture, 30),
                        new IndustryAllocation(MortalIndustryType.Handicrafts, 25),
                        new IndustryAllocation(MortalIndustryType.Commerce, 45)
                    ],
                    Output:
                    [
                        new OutputMetric("River grain", 190, "loads"),
                        new OutputMetric("Trade contracts", 44, "deals"),
                        new OutputMetric("Transit taxes", 58, "taels")
                    ]),
                new(
                    Id: "town.redcliff-garrison",
                    RegionId: "region.redcliff-frontier",
                    Name: "Redcliff Garrison",
                    Population: 9600,
                    Industries:
                    [
                        new IndustryAllocation(MortalIndustryType.Agriculture, 25),
                        new IndustryAllocation(MortalIndustryType.Handicrafts, 45),
                        new IndustryAllocation(MortalIndustryType.Commerce, 30)
                    ],
                    Output:
                    [
                        new OutputMetric("Hard grain", 110, "loads"),
                        new OutputMetric("Mining tools", 68, "sets"),
                        new OutputMetric("Border tolls", 39, "taels")
                    ]),
                new(
                    Id: "town.mistwood-harbor",
                    RegionId: "region.mistwood-heartland",
                    Name: "Mistwood Harbor",
                    Population: 15700,
                    Industries:
                    [
                        new IndustryAllocation(MortalIndustryType.Agriculture, 40),
                        new IndustryAllocation(MortalIndustryType.Handicrafts, 35),
                        new IndustryAllocation(MortalIndustryType.Commerce, 25)
                    ],
                    Output:
                    [
                        new OutputMetric("Medicinal herbs", 128, "crates"),
                        new OutputMetric("Timber goods", 74, "bundles"),
                        new OutputMetric("Port duties", 51, "taels")
                    ])
            };

            SectState[] sects =
            {
                new(
                    Id: "sect.azure-talisman-academy",
                    RegionId: "region.azure-river",
                    Name: "Azure Talisman Academy",
                    Funds: 5200,
                    Population: 164,
                    Output:
                    [
                        new OutputMetric("Warding talismans", 58, "seals"),
                        new OutputMetric("Ritual commissions", 18, "contracts")
                    ]),
                new(
                    Id: "sect.iron-peak-hall",
                    RegionId: "region.redcliff-frontier",
                    Name: "Iron Peak Hall",
                    Funds: 4380,
                    Population: 141,
                    Output:
                    [
                        new OutputMetric("Refined ore", 92, "crates"),
                        new OutputMetric("Forged implements", 37, "sets")
                    ]),
                new(
                    Id: "sect.verdant-crucible-sect",
                    RegionId: "region.mistwood-heartland",
                    Name: "Verdant Crucible Sect",
                    Funds: 6130,
                    Population: 188,
                    Output:
                    [
                        new OutputMetric("Spirit herbs", 144, "bundles"),
                        new OutputMetric("Alchemy batches", 32, "lots")
                    ])
            };

            MinistryState[] ministries =
            {
                new(
                    Id: "ministry.personnel",
                    Name: "Ministry of Personnel",
                    Authority: new MinistryAuthorityProfile(
                        DelegationLevel: "Moderate delegation",
                        EscalationRule: "Unusual sect growth and sensitive appointments are escalated to the throne.",
                        Responsibilities:
                        [
                            "Review sect petitions",
                            "Maintain official rosters",
                            "Track regional appointments"
                        ]),
                    Standard: new HandlingStandardProfile(
                        Name: "Conservative review",
                        Summary: "Routine requests can be screened quickly, but rapid expansion is treated as a strategic risk."),
                    Minister: new OfficialState(
                        Id: "official.shen-yu",
                        Name: "Shen Yu",
                        Role: "Minister",
                        Ratings: new OfficialRatings(84, 79, 86)),
                    SupportingOfficials:
                    [
                        new OfficialState(
                            Id: "official.qiao-lin",
                            Name: "Qiao Lin",
                            Role: "Registrar",
                            Ratings: new OfficialRatings(73, 81, 74)),
                        new OfficialState(
                            Id: "official.he-zhen",
                            Name: "He Zhen",
                            Role: "Personnel clerk",
                            Ratings: new OfficialRatings(68, 76, 77))
                    ]),
                new(
                    Id: "ministry.revenue",
                    Name: "Ministry of Revenue",
                    Authority: new MinistryAuthorityProfile(
                        DelegationLevel: "Broad delegation",
                        EscalationRule: "Revenue shocks and emergency treasury draws require direct approval.",
                        Responsibilities:
                        [
                            "Track treasury reserves",
                            "Assess tax obligations",
                            "Record local remittances"
                        ]),
                    Standard: new HandlingStandardProfile(
                        Name: "Balanced collection",
                        Summary: "Treasury growth matters, but extraction should not destabilize frontier or heartland settlements."),
                    Minister: new OfficialState(
                        Id: "official.song-mei",
                        Name: "Song Mei",
                        Role: "Minister",
                        Ratings: new OfficialRatings(88, 82, 80)),
                    SupportingOfficials:
                    [
                        new OfficialState(
                            Id: "official.yin-kai",
                            Name: "Yin Kai",
                            Role: "Tax assessor",
                            Ratings: new OfficialRatings(75, 69, 72)),
                        new OfficialState(
                            Id: "official.luo-fen",
                            Name: "Luo Fen",
                            Role: "Treasury scribe",
                            Ratings: new OfficialRatings(71, 85, 78))
                    ]),
                new(
                    Id: "ministry.rites",
                    Name: "Ministry of Rites",
                    Authority: new MinistryAuthorityProfile(
                        DelegationLevel: "Narrow delegation",
                        EscalationRule: "Disputes that could shift sect standing are elevated immediately.",
                        Responsibilities:
                        [
                            "Maintain sect protocol",
                            "Track diplomatic standing",
                            "Prepare ceremonial notices"
                        ]),
                    Standard: new HandlingStandardProfile(
                        Name: "Precautionary mediation",
                        Summary: "Minor etiquette issues are settled quietly, but factional tension is documented for the ruler."),
                    Minister: new OfficialState(
                        Id: "official.pei-rong",
                        Name: "Pei Rong",
                        Role: "Minister",
                        Ratings: new OfficialRatings(79, 88, 83)),
                    SupportingOfficials:
                    [
                        new OfficialState(
                            Id: "official.duan-xi",
                            Name: "Duan Xi",
                            Role: "Protocol envoy",
                            Ratings: new OfficialRatings(74, 77, 81)),
                        new OfficialState(
                            Id: "official.su-an",
                            Name: "Su An",
                            Role: "Record keeper",
                            Ratings: new OfficialRatings(66, 84, 75))
                    ])
            };

            return new RealmState(
                RealmName: "River-Crown Protectorate",
                Treasury: new TreasuryState(
                    Funds: 125000,
                    BaselineTaxIncome: 12800),
                Regions: regions,
                Towns: towns,
                Sects: sects,
                Ministries: ministries);
        }
    }
}
