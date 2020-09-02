public class WindNymph : Nymph {
    
    public WindNymph() : base(SUMMON_TYPE.Wind_Nymph, "Wind Nymph") { }
    public WindNymph(string className) : base(SUMMON_TYPE.Wind_Nymph, className) { }
    public WindNymph(SaveDataSummon data) : base(data) { }
}