using System;
using System.Collections.Generic;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class VengefulGhost : Summon {
    public override string raceClassName => characterClass.className;
    public override Faction defaultFaction => FactionManager.Instance.undeadFaction;
    public VengefulGhost() : base(SUMMON_TYPE.Vengeful_Ghost, "Vengeful Ghost", RACE.GHOST, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public VengefulGhost(string className) : base(SUMMON_TYPE.Vengeful_Ghost, className, RACE.GHOST, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public VengefulGhost(SaveDataSummon data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetToFlying();
        RemoveAdvertisedAction(INTERACTION_TYPE.BURY_CHARACTER);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Vengeful_Ghost_Behaviour);
        isWildMonster = false;
    }
    protected override void OnChangeFaction(Faction prevFaction, Faction newFaction) {
        base.OnChangeFaction(prevFaction, newFaction);
        behaviourComponent.ResetInvadeVillageTarget();
    }
    #endregion
}
