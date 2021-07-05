public class EarthenWisp : Wisp {
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;
    public EarthenWisp() : base(SUMMON_TYPE.Earthen_Wisp, "Earthen Wisp") { }
    public EarthenWisp(string className) : base(SUMMON_TYPE.Earthen_Wisp, className) { }
    public EarthenWisp(SaveDataSummon data) : base(data) { }
}