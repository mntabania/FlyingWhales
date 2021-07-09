using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

/// <summary>
/// A slow-walking undead that may spook civilian NPCs.
/// </summary>
public class Skeleton : Summon {

    public override string bredBehaviour => "Snatcher";
    public override Faction defaultFaction => FactionManager.Instance.undeadFaction;
    public override string raceClassName => "Skeleton";
    public Skeleton() : base(SUMMON_TYPE.Skeleton, "Skeleton", RACE.SKELETON, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Skeleton(string className) : base(SUMMON_TYPE.Skeleton, className, RACE.SKELETON, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Skeleton(SaveDataSummon data) : base(data) {
        visuals.SetHasBlood(false);
    }

	public override void Initialize() {
		base.Initialize();
		isWildMonster = false;
	}
	protected override void OnTickEnded() {
		base.OnTickEnded();
		PerTickOutsideCombatHPRecovery();
	}
	
	//#region Overrides
	//public override bool SetFaction(Faction newFaction) {
	//    if (base.SetFaction(newFaction)) {
	//        if (newFaction.isPlayerFaction) {
	//            //if skeleton became part of player faction, add bre 
	//        }
	//        return true;
	//    }
	//    return false;
	//}
	//#endregion
}

