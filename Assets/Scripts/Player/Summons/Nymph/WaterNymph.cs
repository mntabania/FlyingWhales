public class WaterNymph : Nymph {
    
    public WaterNymph() : base(SUMMON_TYPE.Water_Nymph, "Water Nymph") { }
    public WaterNymph(string className) : base(SUMMON_TYPE.Water_Nymph, className) { }
    public WaterNymph(SaveDataCharacter data) : base(data) { }
}