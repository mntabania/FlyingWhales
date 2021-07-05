public abstract class Animal : Summon {

    //public bool isShearable { set; get; }

    public Animal(SUMMON_TYPE summonType, string className, RACE race) : base(summonType, className, race, UtilityScripts.Utilities.GetRandomGender()) { }
    public Animal(SaveDataSummon data) : base(data) { }
}
