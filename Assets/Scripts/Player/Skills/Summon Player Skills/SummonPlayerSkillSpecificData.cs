using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonMarauderData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_MARAUDER;
    public override string name { get { return "Skeleton Marauder"; } }
    public override string description { get { return "Skeleton Marauder"; } }

    public SkeletonMarauderData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Marauder";
    }
}
public class SkeletonArcherData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_ARCHER;
    public override string name { get { return "Skeleton Archer"; } }
    public override string description { get { return "Skeleton Archer"; } }

    public SkeletonArcherData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Archer";
    }
}
public class SkeletonBarbarianData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_BARBARIAN;
    public override string name { get { return "Skeleton Barbarian"; } }
    public override string description { get { return "Skeleton Barbarian"; } }

    public SkeletonBarbarianData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Barbarian";
    }
}
public class SkeletonCraftsmanData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_CRAFTSMAN;
    public override string name { get { return "Skeleton Craftsman"; } }
    public override string description { get { return "Skeleton Craftsman"; } }

    public SkeletonCraftsmanData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Craftsman";
    }
}
public class SkeletonDruidData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_DRUID;
    public override string name { get { return "Skeleton Druid"; } }
    public override string description { get { return "Skeleton Druid"; } }

    public SkeletonDruidData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Druid";
    }
}
public class SkeletonHunterData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_HUNTER;
    public override string name { get { return "Skeleton Hunter"; } }
    public override string description { get { return "Skeleton Hunter"; } }

    public SkeletonHunterData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Hunter";
    }
}
public class SkeletonMageData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_MAGE;
    public override string name { get { return "Skeleton Mage"; } }
    public override string description { get { return "Skeleton Mage"; } }

    public SkeletonMageData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Mage";
    }
}
public class SkeletonKnightData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_KNIGHT;
    public override string name { get { return "Skeleton Knight"; } }
    public override string description { get { return "Skeleton Knight"; } }

    public SkeletonKnightData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Knight";
    }
}
public class SkeletonMinerData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_MINER;
    public override string name { get { return "Skeleton Miner"; } }
    public override string description { get { return "Skeleton Miner"; } }

    public SkeletonMinerData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Miner";
    }
}
public class SkeletonNobleData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_NOBLE;
    public override string name { get { return "Skeleton Noble"; } }
    public override string description { get { return "Skeleton Noble"; } }

    public SkeletonNobleData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Noble";
    }
}
public class SkeletonPeasantData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_PEASANT;
    public override string name { get { return "Skeleton Peasant"; } }
    public override string description { get { return "Skeleton Peasant"; } }

    public SkeletonPeasantData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Peasant";
    }
}
public class SkeletonShamanData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_SHAMAN;
    public override string name { get { return "Skeleton Shaman"; } }
    public override string description { get { return "Skeleton Shaman"; } }

    public SkeletonShamanData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Shaman";
    }
}
public class SkeletonStalkerData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_STALKER;
    public override string name { get { return "Skeleton Stalker"; } }
    public override string description { get { return "Skeleton Stalker"; } }

    public SkeletonStalkerData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Stalker";
    }
}
public class WolfData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.WOLF;
    public override string name { get { return "Wolf"; } }
    public override string description { get { return "Wolf"; } }

    public WolfData() {
        summonType = SUMMON_TYPE.Wolf;
        race = RACE.WOLF;
        className = "Ravager";
    }
}
public class GolemData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.GOLEM;
    public override string name { get { return "Golem"; } }
    public override string description { get { return "Golem"; } }

    public GolemData() {
        summonType = SUMMON_TYPE.Golem;
        race = RACE.GOLEM;
        className = "Golem";
    }
}
public class IncubusData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.INCUBUS;
    public override string name { get { return "Incubus"; } }
    public override string description { get { return "Incubus"; } }

    public IncubusData() {
        summonType = SUMMON_TYPE.Incubus;
        race = RACE.LESSER_DEMON;
        className = "Incubus";
    }
}
public class SuccubusData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SUCCUBUS;
    public override string name { get { return "Succubus"; } }
    public override string description { get { return "Succubus"; } }

    public SuccubusData() {
        summonType = SUMMON_TYPE.Succubus;
        race = RACE.LESSER_DEMON;
        className = "Succubus";
    }
}
public class FireElementalData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.FIRE_ELEMENTAL;
    public override string name { get { return "Fire Elemental"; } }
    public override string description { get { return "Fire Elemental"; } }

    public FireElementalData() {
        summonType = SUMMON_TYPE.Fire_Elemental;
        race = RACE.ELEMENTAL;
        className = "Fire Elemental";
    }
}
public class KoboldData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.KOBOLD;
    public override string name { get { return "Kobold"; } }
    public override string description { get { return "Kobold"; } }

    public KoboldData() {
        summonType = SUMMON_TYPE.Kobold;
        race = RACE.KOBOLD;
        className = "Kobold";
    }
}
public class GhostData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.GHOST;
    public override string name { get { return "Ghost"; } }
    public override string description { get { return "Ghost"; } }

    public GhostData() {
        summonType = SUMMON_TYPE.Ghost;
        race = RACE.GHOST;
        className = "Ghost";
    }
}
public class VengefulGhostData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.VENGEFUL_GHOST;
    public override string name { get { return "Vengeful Ghost"; } }
    public override string description { get { return "Vengeful Ghost"; } }

    public VengefulGhostData() {
        summonType = SUMMON_TYPE.Vengeful_Ghost;
        race = RACE.GHOST;
        className = "Vengeful Ghost";
    }
}
public class AbominationData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.ABOMINATION;
    public override string name { get { return "Abomination"; } }
    public override string description { get { return "Abomination"; } }

    public AbominationData() {
        summonType = SUMMON_TYPE.Abomination;
        race = RACE.ABOMINATION;
        className = "Abomination";
    }
}
public class MimicData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.MIMIC;
    public override string name { get { return "Mimic"; } }
    public override string description { get { return "Mimic"; } }

    public MimicData() {
        summonType = SUMMON_TYPE.Mimic;
        race = RACE.MIMIC;
        className = "Mimic";
    }
}
public class PigData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.PIG;
    public override string name { get { return "Pig"; } }
    public override string description { get { return "Pig"; } }

    public PigData() {
        summonType = SUMMON_TYPE.Pig;
        race = RACE.PIG;
        className = "Pig";
    }
}
public class ChickenData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.CHICKEN;
    public override string name { get { return "Chicken"; } }
    public override string description { get { return "Chicken"; } }

    public ChickenData() {
        summonType = SUMMON_TYPE.Chicken;
        race = RACE.CHICKEN;
        className = "Chicken";
    }
}
public class SheepData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SHEEP;
    public override string name { get { return "Sheep"; } }
    public override string description { get { return "Sheep"; } }

    public SheepData() {
        summonType = SUMMON_TYPE.Sheep;
        race = RACE.SHEEP;
        className = "Sheep";
    }
}
public class SludgeData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SLUDGE;
    public override string name { get { return "Sludge"; } }
    public override string description { get { return "Sludge"; } }

    public SludgeData() {
        summonType = SUMMON_TYPE.Sludge;
        race = RACE.SLUDGE;
        className = "Sludge";
    }
}
public class WaterNymphData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.WATER_NYMPH;
    public override string name { get { return "Water Nymph"; } }
    public override string description { get { return "Water Nymph"; } }

    public WaterNymphData() {
        summonType = SUMMON_TYPE.Water_Nymph;
        race = RACE.NYMPH;
        className = "Water Nymph";
    }
}
public class WindNymphData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.WIND_NYMPH;
    public override string name { get { return "Wind Nymph"; } }
    public override string description { get { return "Wind Nymph"; } }

    public WindNymphData() {
        summonType = SUMMON_TYPE.Wind_Nymph;
        race = RACE.NYMPH;
        className = "Wind Nymph";
    }
}
public class IceNymphData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.ICE_NYMPH;
    public override string name { get { return "Ice Nymph"; } }
    public override string description { get { return "Ice Nymph"; } }

    public IceNymphData() {
        summonType = SUMMON_TYPE.Ice_Nymph;
        race = RACE.NYMPH;
        className = "Ice Nymph";
    }
}
public class ElectricWispData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.ELECTRIC_WISP;
    public override string name { get { return "Electric Wisp"; } }
    public override string description { get { return "Electric Wisp"; } }

    public ElectricWispData() {
        summonType = SUMMON_TYPE.Electric_Wisp;
        race = RACE.WISP;
        className = "Electric Wisp";
    }
}
public class EarthenWispData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.EARTHEN_WISP;
    public override string name { get { return "Earthen Wisp"; } }
    public override string description { get { return "Earthen Wisp"; } }

    public EarthenWispData() {
        summonType = SUMMON_TYPE.Earthen_Wisp;
        race = RACE.WISP;
        className = "Earthen Wisp";
    }
}
public class FireWispData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.FIRE_WISP;
    public override string name { get { return "Fire Wisp"; } }
    public override string description { get { return "Fire Wisp"; } }

    public FireWispData() {
        summonType = SUMMON_TYPE.Fire_Wisp;
        race = RACE.WISP;
        className = "Fire Wisp";
    }
}
public class GrassEntData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.GRASS_ENT;
    public override string name { get { return "Grass Ent"; } }
    public override string description { get { return "Grass Ent"; } }

    public GrassEntData() {
        summonType = SUMMON_TYPE.Grass_Ent;
        race = RACE.ENT;
        className = "Grass Ent";
    }
}
public class SnowEntData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SNOW_ENT;
    public override string name { get { return "Snow Ent"; } }
    public override string description { get { return "Snow Ent"; } }

    public SnowEntData() {
        summonType = SUMMON_TYPE.Snow_Ent;
        race = RACE.ENT;
        className = "Snow Ent";
    }
}
public class CorruptEntData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.CORRUPT_ENT;
    public override string name { get { return "Corrupt Ent"; } }
    public override string description { get { return "Corrupt Ent"; } }

    public CorruptEntData() {
        summonType = SUMMON_TYPE.Corrupt_Ent;
        race = RACE.ENT;
        className = "Corrupt Ent";
    }
}
public class ForestEntData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.FOREST_ENT;
    public override string name { get { return "Forest Ent"; } }
    public override string description { get { return "Forest Ent"; } }

    public ForestEntData() {
        summonType = SUMMON_TYPE.Forest_Ent;
        race = RACE.ENT;
        className = "Forest Ent";
    }
}
public class DesertEntData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DESERT_ENT;
    public override string name { get { return "Desert Ent"; } }
    public override string description { get { return "Desert Ent"; } }

    public DesertEntData() {
        summonType = SUMMON_TYPE.Desert_Ent;
        race = RACE.ENT;
        className = "Desert Ent";
    }
}
public class GiantSpiderData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.GIANT_SPIDER;
    public override string name { get { return "Giant Spider"; } }
    public override string description { get { return "Giant Spider"; } }

    public GiantSpiderData() {
        summonType = SUMMON_TYPE.Giant_Spider;
        race = RACE.SPIDER;
        className = "Giant Spider";
    }
}
public class SmallSpiderData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SMALL_SPIDER;
    public override string name { get { return "Small Spider"; } }
    public override string description { get { return "Small Spider"; } }

    public SmallSpiderData() {
        summonType = SUMMON_TYPE.Small_Spider;
        race = RACE.SPIDER;
        className = "Small Spider";
    }
}
public class WurmData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.WURM;
    public override string name { get { return "Wurm"; } }
    public override string description { get { return "Wurm"; } }

    public WurmData() {
        summonType = SUMMON_TYPE.Wurm;
        race = RACE.WURM;
        className = "Wurm";
    }
}