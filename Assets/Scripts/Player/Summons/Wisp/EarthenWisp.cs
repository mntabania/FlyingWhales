public class EarthenWisp : Wisp {
    public EarthenWisp() : base(SUMMON_TYPE.Earthen_Wisp, "Earthen Wisp") { }
    public EarthenWisp(string className) : base(SUMMON_TYPE.Earthen_Wisp, className) { }
    public EarthenWisp(SaveDataCharacter data) : base(data) { }
}