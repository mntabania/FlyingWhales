public class FireWisp : Wisp {
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;
    public FireWisp() : base(SUMMON_TYPE.Fire_Wisp, "Fire Wisp") { }
    public FireWisp(string className) : base(SUMMON_TYPE.Fire_Wisp, className) { }
    public FireWisp(SaveDataSummon data) : base(data) { }
}