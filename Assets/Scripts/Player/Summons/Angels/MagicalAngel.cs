using Inner_Maps;
using Traits;

public class MagicalAngel : Summon {
    public override string raceClassName => $"Magical Angel";
    
    public MagicalAngel() : base(SUMMON_TYPE.Magical_Angel, "Magical Angel", RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }
    public MagicalAngel(string className) : base(SUMMON_TYPE.Magical_Angel, className, RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }

}