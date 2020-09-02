public class SnowEnt : Ent {
    
    public SnowEnt() : base(SUMMON_TYPE.Snow_Ent, "Snow Ent") { }
    public SnowEnt(string className) : base(SUMMON_TYPE.Snow_Ent, className) { }
    public SnowEnt(SaveDataEnt data) : base(data) { }
}
