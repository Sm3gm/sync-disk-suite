namespace SoDVanillaSplit;

public class SplitDef
{
    public string DiskName;   // FROZEN SAVE KEY once installed. Do not edit casually.
    public string Parent;     // vanilla preset .name
    public int Branch;        // 1, 2 or 3
    public int Tiers;         // 0-3, taken from the dump's OPTIONn tier counts
    public int Price;         // Deliberate ladder value. NOT the parent's price.
                              // Splitting decouples branches, so each is priced
                              // for what it does rather than what it came from.
                              // Not a save key - tunable on a live save forever.
}

public static class Splits
{
    // Branch names and tier counts transcribed from syncdisk_dump.txt, 5 Sep.
    // Game typos preserved deliberately: "Hygeine", "Safecacker".
    public static readonly SplitDef[] All = new SplitDef[]
    {
        // --- 3-branch parents ---
               new SplitDef { DiskName="Clout",                      Parent="ElGen-Tenacity",                    Branch=1, Tiers=3, Price=1500 },
        new SplitDef { DiskName="Brawn",                      Parent="ElGen-Tenacity",                    Branch=2, Tiers=3, Price=1250 },
        new SplitDef { DiskName="Reflexes",                   Parent="ElGen-Tenacity",                    Branch=3, Tiers=3, Price=2500 },

        new SplitDef { DiskName="Lockpicker",                 Parent="BlackMarket-Trespasser",            Branch=1, Tiers=0, Price=1750 },
        new SplitDef { DiskName="Resourceful",                Parent="BlackMarket-Trespasser",            Branch=2, Tiers=0, Price=1750 },
        new SplitDef { DiskName="Invisible",                  Parent="BlackMarket-Trespasser",            Branch=3, Tiers=0, Price=1250 },

        new SplitDef { DiskName="Gold Medical Cover",         Parent="Kensington-SpartanInsuranceSchemes",Branch=1, Tiers=2, Price=1000 },
        new SplitDef { DiskName="Gold Legal Cover",           Parent="Kensington-SpartanInsuranceSchemes",Branch=2, Tiers=2, Price=1250 },
        new SplitDef { DiskName="Gold Accident Cover",        Parent="Kensington-SpartanInsuranceSchemes",Branch=3, Tiers=2, Price=750 },

        new SplitDef { DiskName="Street Cleaner",             Parent="Candor-ModelCitizen",               Branch=1, Tiers=3, Price=500 },
        new SplitDef { DiskName="Bookworm",                   Parent="Candor-ModelCitizen",               Branch=2, Tiers=3, Price=500 },

        new SplitDef { DiskName="Power",                      Parent="ElGen-Vigor",                       Branch=1, Tiers=0, Price=2500 },
        new SplitDef { DiskName="Stability",                  Parent="ElGen-Vigor",                       Branch=2, Tiers=0, Price=3000 },

        new SplitDef { DiskName="Physiological Perception",   Parent="Kaizen-DovePlus",                   Branch=1, Tiers=3, Price=1750 },
        new SplitDef { DiskName="Socioeconomic Perception",   Parent="Kaizen-DovePlus",                   Branch=2, Tiers=3, Price=1000 },

        new SplitDef { DiskName="Food Hygeine Inspector",     Parent="Candor-PublicService",              Branch=1, Tiers=3, Price=500 },
        new SplitDef { DiskName="Sanitary Hygeine Inspector", Parent="Candor-PublicService",              Branch=2, Tiers=3, Price=500 },

        new SplitDef { DiskName="Rogue",                      Parent="BlackMarket-Infiltrator",           Branch=1, Tiers=1, Price=1750 },
        new SplitDef { DiskName="Hacker",                     Parent="BlackMarket-Infiltrator",           Branch=2, Tiers=1, Price=1750 },

        new SplitDef { DiskName="Cash Flow",                  Parent="Kensington-AmbassadorScheme",       Branch=1, Tiers=3, Price=500 },
        new SplitDef { DiskName="Competitor Data Mining",     Parent="Kensington-AmbassadorScheme",       Branch=2, Tiers=3, Price=500 },

        new SplitDef { DiskName="Fortitude",                  Parent="ElGen-Physique",                    Branch=1, Tiers=1, Price=1750 },
        new SplitDef { DiskName="Vitality",                   Parent="ElGen-Physique",                    Branch=2, Tiers=1, Price=1500 },

        new SplitDef { DiskName="Allure",                     Parent="ElGen-Beauty",                      Branch=1, Tiers=3, Price=1500 },
        new SplitDef { DiskName="Charm",                      Parent="ElGen-Beauty",                      Branch=2, Tiers=3, Price=1500 },

        new SplitDef { DiskName="Chemistry",                  Parent="ElGen-Constitution",                Branch=1, Tiers=2, Price=2000 },
        new SplitDef { DiskName="Cardiovascular",             Parent="ElGen-Constitution",                Branch=2, Tiers=2, Price=2000 },

        new SplitDef { DiskName="Spread the word!",           Parent="Starch-BrandAmbassador",            Branch=1, Tiers=2, Price=500 },
        new SplitDef { DiskName="Put some life into it!",     Parent="Starch-BrandAmbassador",            Branch=2, Tiers=2, Price=500 },

        new SplitDef { DiskName="Urbex Cartographer",         Parent="Candor-Cartographer",               Branch=1, Tiers=0, Price=500 },
        new SplitDef { DiskName="Crawlspace Engineer",        Parent="Candor-Cartographer",               Branch=2, Tiers=0, Price=500 },

        new SplitDef { DiskName="Care in the Community",      Parent="Candor-Community",                  Branch=1, Tiers=1, Price=750 },
        new SplitDef { DiskName="Heavy Lifter",               Parent="Candor-Community",                  Branch=2, Tiers=1, Price=1500 },

        new SplitDef { DiskName="Statuesque",                 Parent="ElGen-Frame",                       Branch=1, Tiers=2, Price=750 },
        new SplitDef { DiskName="Compact",                    Parent="ElGen-Frame",                       Branch=2, Tiers=2, Price=750 },

        new SplitDef { DiskName="Mailing List",               Parent="BlackMarket-Interceptor",           Branch=1, Tiers=0, Price=500 },
        new SplitDef { DiskName="Safecacker",                 Parent="BlackMarket-Interceptor",           Branch=2, Tiers=0, Price=500 },
    };

    // Starch-SugarDaddy is single-branch and is deliberately absent: nothing to split.
    public static readonly string[] Parents = new string[]
    {
        "ElGen-Tenacity", "BlackMarket-Trespasser", "Kensington-SpartanInsuranceSchemes",
        "Candor-ModelCitizen", "ElGen-Vigor", "Kaizen-DovePlus", "Candor-PublicService",
        "BlackMarket-Infiltrator", "Kensington-AmbassadorScheme", "ElGen-Physique",
        "ElGen-Beauty", "ElGen-Constitution", "Starch-BrandAmbassador",
        "Candor-Cartographer", "Candor-Community", "ElGen-Frame", "BlackMarket-Interceptor",
    };
}