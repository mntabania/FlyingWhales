public class GrassEnt : Ent{
    
    public GrassEnt() : base(SUMMON_TYPE.Grass_Ent, "Grass Ent") { }
    public GrassEnt(string className) : base(SUMMON_TYPE.Grass_Ent, className) { }
    public GrassEnt(SaveDataCharacter data) : base(data) { }
}
