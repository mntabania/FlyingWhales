public class DesertEnt : Ent {
    
    public DesertEnt() : base(SUMMON_TYPE.Desert_Ent, "Desert Ent") { }
    public DesertEnt(string className) : base(SUMMON_TYPE.Desert_Ent, className) { }
    public DesertEnt(SaveDataCharacter data) : base(data) { }
}
