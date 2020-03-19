public class CorruptEnt : Ent {
    public CorruptEnt() : base(SUMMON_TYPE.Corrupt_Ent, "Corrupt Ent") { }
    public CorruptEnt(string className) : base(SUMMON_TYPE.Corrupt_Ent, className) { }
    public CorruptEnt(SaveDataCharacter data) : base(data) { }
}
