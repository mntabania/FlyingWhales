using Inner_Maps;
using Traits;

public class Mimic : Summon {

    public override string raceClassName => "Mimic";
    
    public Mimic() : base(SUMMON_TYPE.Mimic, "Mimic", RACE.MIMIC,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Mimic(SaveDataCharacter data) : base(data) { }
    
    
}