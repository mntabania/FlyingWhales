using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

/// <summary>
/// A slow-walking undead that may spook civilian NPCs.
/// </summary>
public class Imp : Summon {

    public override string bredBehaviour => "Snatcher";
    public override Faction defaultFaction => FactionManager.Instance.undeadFaction;
    public override string raceClassName => "Imp";

    public Imp() : base(SUMMON_TYPE.Imp, "Imp", RACE.IMP, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Imp(string className) : base(SUMMON_TYPE.Imp, className, RACE.IMP, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Imp(SaveDataSummon data) : base(data) {
    }
}

