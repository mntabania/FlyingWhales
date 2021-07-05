using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WOLF;
    public override string name => "Wolf";
    public override string description => "Wolf";
    public WolfData() {
        summonType = SUMMON_TYPE.Wolf;
        race = RACE.WOLF;
        className = "Ravager";
    }
}

public class DireWolfData : SummonPlayerSkill
{
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DIRE_WOLF;
    public override string name => "Dire Wolf";
    public override string description => "Dire Wolf";
    public DireWolfData() {
        summonType = SUMMON_TYPE.Dire_Wolf;
        race = RACE.WOLF;
        className = "Dire";
    }
}
public class GolemData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GOLEM;
    public override string name => "Golem";
    public override string description => "Golem";
    public GolemData() {
        summonType = SUMMON_TYPE.Golem;
        race = RACE.GOLEM;
        className = "Golem";
    }
}
public class BoneGolemData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BONE_GOLEM;
    public override string name => "Bone Golem";
    public override string description => "Bone Golem";
    public BoneGolemData() {
        summonType = SUMMON_TYPE.Bone_Golem;
        race = RACE.GOLEM;
        className = "Bone Golem";
    }
}
public class IncubusData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INCUBUS;
    public override string name => "Incubus";
    public override string description => "Incubus";
    public IncubusData() {
        summonType = SUMMON_TYPE.Incubus;
        race = RACE.LESSER_DEMON;
        className = "Incubus";
    }
}
public class SuccubusData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SUCCUBUS;
    public override string name => "Succubus";
    public override string description => "Succubus";
    public SuccubusData() {
        summonType = SUMMON_TYPE.Succubus;
        race = RACE.LESSER_DEMON;
        className = "Succubus";
    }
}
public class FireElementalData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FIRE_ELEMENTAL;
    public override string name => "Fire Elemental";
    public override string description => "Fire Elemental";
    public FireElementalData() {
        summonType = SUMMON_TYPE.Fire_Elemental;
        race = RACE.ELEMENTAL;
        className = "Fire Elemental";
    }
}
public class KoboldData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.KOBOLD;
    public override string name => "Kobold";
    public override string description => "Kobold";
    public KoboldData() {
        summonType = SUMMON_TYPE.Kobold;
        race = RACE.KOBOLD;
        className = "Kobold";
    }
}
public class GhostData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GHOST;
    public override string name => "Ghost";
    public override string description => "Ghost";
    public GhostData() {
        summonType = SUMMON_TYPE.Ghost;
        race = RACE.GHOST;
        className = "Ghost";
    }
}
public class VengefulGhostData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.VENGEFUL_GHOST;
    public override string name => "Vengeful Ghost";
    public override string description => "Vengeful Ghost";
    public VengefulGhostData() {
        summonType = SUMMON_TYPE.Vengeful_Ghost;
        race = RACE.GHOST;
        className = "Vengeful Ghost";
    }
}
public class AbominationData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ABOMINATION;
    public override string name => "Abomination";
    public override string description => "Abomination";
    public AbominationData() {
        summonType = SUMMON_TYPE.Abomination;
        race = RACE.ABOMINATION;
        className = "Abomination";
    }
}
public class MimicData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MIMIC;
    public override string name => "Mimic";
    public override string description => "Mimic";
    public MimicData() {
        summonType = SUMMON_TYPE.Mimic;
        race = RACE.MIMIC;
        className = "Mimic";
    }
}
public class PigData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PIG;
    public override string name => "Pig";
    public override string description => "Pig";
    public PigData() {
        summonType = SUMMON_TYPE.Pig;
        race = RACE.PIG;
        className = "Pig";
    }
}
public class ChickenData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CHICKEN;
    public override string name => "Chicken";
    public override string description => "Chicken";
    public ChickenData() {
        summonType = SUMMON_TYPE.Chicken;
        race = RACE.CHICKEN;
        className = "Chicken";
    }
}
public class SheepData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SHEEP;
    public override string name => "Sheep";
    public override string description => "Sheep";
    public SheepData() {
        summonType = SUMMON_TYPE.Sheep;
        race = RACE.SHEEP;
        className = "Sheep";
    }
}
public class SludgeData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SLUDGE;
    public override string name => "Sludge";
    public override string description => "Sludge";
    public SludgeData() {
        summonType = SUMMON_TYPE.Sludge;
        race = RACE.SLUDGE;
        className = "Sludge";
    }
}
public class WaterNymphData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WATER_NYMPH;
    public override string name => "Water Nymph";
    public override string description => "Water Nymph";
    public WaterNymphData() {
        summonType = SUMMON_TYPE.Water_Nymph;
        race = RACE.NYMPH;
        className = "Water Nymph";
    }
}
public class WindNymphData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WIND_NYMPH;
    public override string name => "Wind Nymph";
    public override string description => "Wind Nymph";
    public WindNymphData() {
        summonType = SUMMON_TYPE.Wind_Nymph;
        race = RACE.NYMPH;
        className = "Wind Nymph";
    }
}
public class IceNymphData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICE_NYMPH;
    public override string name => "Ice Nymph";
    public override string description => "Ice Nymph";
    public IceNymphData() {
        summonType = SUMMON_TYPE.Ice_Nymph;
        race = RACE.NYMPH;
        className = "Ice Nymph";
    }
}
public class ElectricWispData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ELECTRIC_WISP;
    public override string name => "Electric Wisp";
    public override string description => "Electric Wisp";
    public ElectricWispData() {
        summonType = SUMMON_TYPE.Electric_Wisp;
        race = RACE.WISP;
        className = "Electric Wisp";
    }
}
public class EarthenWispData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTHEN_WISP;
    public override string name => "Earthen Wisp";
    public override string description => "Earthen Wisp";
    public EarthenWispData() {
        summonType = SUMMON_TYPE.Earthen_Wisp;
        race = RACE.WISP;
        className = "Earthen Wisp";
    }
}
public class FireWispData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FIRE_WISP;
    public override string name => "Fire Wisp";
    public override string description => "Fire Wisp";
    public FireWispData() {
        summonType = SUMMON_TYPE.Fire_Wisp;
        race = RACE.WISP;
        className = "Fire Wisp";
    }
}
public class GrassEntData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GRASS_ENT;
    public override string name => "Grass Ent";
    public override string description => "Grass Ent";
    public GrassEntData() {
        summonType = SUMMON_TYPE.Grass_Ent;
        race = RACE.ENT;
        className = "Grass Ent";
    }
}
public class SnowEntData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNOW_ENT;
    public override string name => "Snow Ent";
    public override string description => "Snow Ent";
    public SnowEntData() {
        summonType = SUMMON_TYPE.Snow_Ent;
        race = RACE.ENT;
        className = "Snow Ent";
    }
}
public class CorruptEntData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CORRUPT_ENT;
    public override string name => "Corrupt Ent";
    public override string description => "Corrupt Ent";
    public CorruptEntData() {
        summonType = SUMMON_TYPE.Corrupt_Ent;
        race = RACE.ENT;
        className = "Corrupt Ent";
    }
}
public class ForestEntData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FOREST_ENT;
    public override string name => "Forest Ent";
    public override string description => "Forest Ent";
    public ForestEntData() {
        summonType = SUMMON_TYPE.Forest_Ent;
        race = RACE.ENT;
        className = "Forest Ent";
    }
}
public class DesertEntData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESERT_ENT;
    public override string name => "Desert Ent";
    public override string description => "Desert Ent";
    public DesertEntData() {
        summonType = SUMMON_TYPE.Desert_Ent;
        race = RACE.ENT;
        className = "Desert Ent";
    }
}
public class GiantSpiderData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GIANT_SPIDER;
    public override string name => "Giant Spider";
    public override string description => "Giant Spider";
    public GiantSpiderData() {
        summonType = SUMMON_TYPE.Giant_Spider;
        race = RACE.SPIDER;
        className = "Giant Spider";
    }
}
public class SmallSpiderData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SMALL_SPIDER;
    public override string name => "Small Spider";
    public override string description => "Small Spider";
    public SmallSpiderData() {
        summonType = SUMMON_TYPE.Small_Spider;
        race = RACE.SPIDER;
        className = "Small Spider";
    }
}
public class WurmData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WURM;
    public override string name => "Wurm";
    public override string description => "Wurm";
    public WurmData() {
        summonType = SUMMON_TYPE.Wurm;
        race = RACE.WURM;
        className = "Wurm";
    }
}
public class TrollData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TROLL;
    public override string name => "Troll";
    public override string description => "Troll";
    public TrollData() {
        summonType = SUMMON_TYPE.Troll;
        race = RACE.TROLL;
        className = "Troll";
    }
}
public class RevenantData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REVENANT;
    public override string name => "Revenant";
    public override string description => "Revenant";
    public RevenantData() {
        summonType = SUMMON_TYPE.Revenant;
        race = RACE.REVENANT;
        className = "Revenant";
    }
}
public class ScorpionData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCORPION;
    public override string name => "Scorpion";
    public override string description => "Scorpion";
    public ScorpionData() {
        summonType = SUMMON_TYPE.Scorpion;
        race = RACE.SCORPION;
        className = "Scorpion";
    }
}
public class HarpyData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HARPY;
    public override string name => "Harpy";
    public override string description => "Harpy";
    public HarpyData() {
        summonType = SUMMON_TYPE.Harpy;
        race = RACE.HARPY;
        className = "Harpy";
    }
}
public class TritonData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRITON;
    public override string name => "Triton";
    public override string description => "Triton";
    public TritonData() {
        summonType = SUMMON_TYPE.Triton;
        race = RACE.TRITON;
        className = "Triton";
 
    }
}

public class SkeletonData : SummonPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SKELETON;
    public override string name => "Skeleton";
    public override string description => "Skeleton";
    public SkeletonData() {
        summonType = SUMMON_TYPE.Skeleton;
        race = RACE.SKELETON;
        className = "Skeleton";

    }
}