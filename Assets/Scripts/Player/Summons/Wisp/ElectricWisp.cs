public class ElectricWisp : Wisp {
    public ElectricWisp() : base(SUMMON_TYPE.Electric_Wisp, "Electric Wisp") { }
    public ElectricWisp(string className) : base(SUMMON_TYPE.Electric_Wisp, className) { }
    public ElectricWisp(SaveDataCharacter data) : base(data) { }
}
