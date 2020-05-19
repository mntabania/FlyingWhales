public class FireWisp : Wisp {
    public FireWisp() : base(SUMMON_TYPE.Fire_Wisp, "Fire Wisp") { }
    public FireWisp(string className) : base(SUMMON_TYPE.Fire_Wisp, className) { }
    public FireWisp(SaveDataCharacter data) : base(data) { }
}