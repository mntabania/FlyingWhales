public abstract class Animal : Summon {
    
    public Animal(SUMMON_TYPE summonType, string className, RACE race) : base(summonType, className, race,
        UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Animal(SaveDataCharacter data) : base(data) { }
}
