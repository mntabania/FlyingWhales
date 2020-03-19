public class SnowEnt : Ent {
    
    public SnowEnt() : base(SUMMON_TYPE.Snow_Ent, "Snow Ent") { }
    public SnowEnt(string className) : base(SUMMON_TYPE.Snow_Ent, className) { }
    public SnowEnt(SaveDataCharacter data) : base(data) { }
}
