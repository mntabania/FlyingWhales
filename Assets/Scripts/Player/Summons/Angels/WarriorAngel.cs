using Inner_Maps;
using Traits;

public class WarriorAngel : Summon {
    public override string raceClassName => $"Warrior Angel";
    
    public WarriorAngel() : base(SUMMON_TYPE.Warrior_Angel, "Warrior Angel", RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }
    public WarriorAngel(string className) : base(SUMMON_TYPE.Warrior_Angel, className, RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }

}