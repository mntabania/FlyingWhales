using System.Collections.Generic;

public static class PlayerDB {
    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;
    public const int DIVINE_INTERVENTION_DURATION = 2880; //4320;
    public const int MAX_INTEL = 3;
    //public const int MAX_MINIONS = 7;
    public const int MAX_INTERVENTION_ABILITIES = 4;
    
    //actions
    public const string Zap_Action = "Zap";
    //public const string Summon_Minion_Action = "Summon Minion";
    //public const string Poison_Action = "Poison";
    //public const string Ignite_Action = "Ignite";
    //public const string Destroy_Action = "Destroy";
    //public const string Corrupt_Action = "Corrupt";
    //public const string Build_Demonic_Structure_Action = "Build Demonic Structure";
    //public const string Animate_Action = "Animate";
    //public const string Afflict_Action = "Afflict";
    public const string Seize_Character_Action = "Seize Character";
    public const string Seize_Object_Action = "Seize Object";
    //public const string Bless_Action = "Bless";
    //public const string Booby_Trap_Action = "Booby Trap";
    //public const string Torture_Action = "Torture";
    //public const string Interfere_Action = "Interfere";
    //public const string Learn_Spell_Action = "Learn Spell";
    //public const string Stop_Action = "Stop";
    //public const string Return_To_Portal_Action = "Return To Portal";
    //public const string Harass_Action = "Harass";
    //public const string Raid_Action = "Raid";
    //public const string Invade_Action = "Invade";
    //public const string End_Harass_Action = "End Harass";
    //public const string End_Raid_Action = "End Raid";
    //public const string End_Invade_Action = "End Invade";
    //public const string Breed_Monster_Action = "Breed Monster";
    //public const string Activate_Artifact_Action = "Activate Artifact";
    public const string Remove_Trait_Action = "Remove Trait";
    //public const string Share_Intel_Action = "Share Intel";
    //public const string Combat_Mode_Action = "Combat Mode";
    //public const string Raise_Skeleton_Action = "Raise Skeleton";

    //spells
    //public const string Tornado = "Tornado";
    //public const string Meteor = "Meteor";
    //public const string Poison_Cloud = "Poison Cloud";
    //public const string Lightning = "Lightning";
    //public const string Ravenous_Spirit = "Ravenous Spirit";
    //public const string Feeble_Spirit = "Feeble Spirit";
    //public const string Forlorn_Spirit = "Forlorn Spirit";
    //public const string Locust_Swarm = "Locust Swarm";
    //public const string Spawn_Boulder = "Spawn Boulder";
    //public const string Landmine = "Landmine";
    //public const string Manifest_Food = "Manifest Food";
    //public const string Brimstones = "Brimstones";
    //public const string Acid_Rain = "Acid Rain";
    //public const string Rain = "Rain";
    //public const string Heat_Wave = "Heat Wave";
    //public const string Wild_Growth = "Wild Growth";
    //public const string Spider_Rain = "Spider Rain";
    //public const string Blizzard = "Blizzard";
    //public const string Earthquake = "Earthquake";
    //public const string Fertility = "Fertility";
    //public const string Spawn_Bandit_Camp = "Spawn Bandit Camp";
    //public const string Spawn_Monster_Lair = "Spawn Monster Lair";
    //public const string Spawn_Haunted_Grounds = "Spawn Haunted Grounds";
    //public const string Water_Bomb = "Water Bomb";
    //public const string Splash_Poison = "Splash Poison";


    public static List<SPELL_TYPE> spells = new List<SPELL_TYPE>() {
        SPELL_TYPE.TORNADO, SPELL_TYPE.METEOR, SPELL_TYPE.POISON_CLOUD, SPELL_TYPE.LIGHTNING,
        SPELL_TYPE.RAVENOUS_SPIRIT, SPELL_TYPE.FEEBLE_SPIRIT, SPELL_TYPE.FORLORN_SPIRIT,
        SPELL_TYPE.LOCUST_SWARM, SPELL_TYPE.BLIZZARD, SPELL_TYPE.SPAWN_BOULDER, SPELL_TYPE.MANIFEST_FOOD,
        SPELL_TYPE.BRIMSTONES, SPELL_TYPE.EARTHQUAKE, SPELL_TYPE.WATER_BOMB, SPELL_TYPE.SPLASH_POISON, SPELL_TYPE.RAIN, //Landmine, Acid_Rain, Rain, Heat_Wave, Wild_Growth, Spider_Rain, Fertility, Spawn_Bandit_Camp, Spawn_Monster_Lair, Spawn_Haunted_Grounds,
        SPELL_TYPE.BALL_LIGHTNING, SPELL_TYPE.ELECTRIC_STORM, SPELL_TYPE.FROSTY_FOG, SPELL_TYPE.VAPOR, SPELL_TYPE.FIRE_BALL,
        SPELL_TYPE.POISON_BLOOM, SPELL_TYPE.LANDMINE, SPELL_TYPE.TERRIFYING_HOWL, SPELL_TYPE.FREEZING_TRAP, SPELL_TYPE.SNARE_TRAP, SPELL_TYPE.WIND_BLAST,
        SPELL_TYPE.ICETEROIDS, SPELL_TYPE.HEAT_WAVE,
    };

    public static List<SPELL_TYPE> afflictions = new List<SPELL_TYPE>() { 
        SPELL_TYPE.PARALYSIS, SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.KLEPTOMANIA, SPELL_TYPE.AGORAPHOBIA, 
        SPELL_TYPE.PSYCHOPATHY, SPELL_TYPE.PESTILENCE, SPELL_TYPE.LYCANTHROPY, 
        SPELL_TYPE.VAMPIRISM, SPELL_TYPE.ZOMBIE_VIRUS, SPELL_TYPE.COWARDICE, SPELL_TYPE.PYROPHOBIA, SPELL_TYPE.NARCOLEPSY, SPELL_TYPE.GLUTTONY
    };
    
    private static string[] unlockableActions = new[] {
        Seize_Object_Action,
        Seize_Character_Action,
        Remove_Trait_Action,
        //Share_Intel_Action,
        Zap_Action,
    };
    private static string[] unlockableStructures = new[] {
        "THE_KENNEL",
        "THE_PIT",
        "TORTURE_CHAMBER",
        "THE_EYE",
        "THE_PROFANE",
    };

    public static string[] GetChoicesForUnlockableType(ARTIFACT_UNLOCKABLE_TYPE type) {
        switch (type) {
            case ARTIFACT_UNLOCKABLE_TYPE.Action:
                return unlockableActions;
            case ARTIFACT_UNLOCKABLE_TYPE.Structure:
                return unlockableStructures;
            default:
                return null;
        }
    }

}
