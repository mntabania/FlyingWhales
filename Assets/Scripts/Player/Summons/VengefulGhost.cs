using System;
using System.Collections.Generic;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class VengefulGhost : Summon {
    public override string raceClassName => characterClass.className;
    public VengefulGhost() : base(SUMMON_TYPE.Vengeful_Ghost, "Vengeful Ghost", RACE.GHOST,
        UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public VengefulGhost(string className) : base(SUMMON_TYPE.Vengeful_Ghost, className, RACE.GHOST,
    UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public VengefulGhost(SaveDataCharacter data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Vengeful_Ghost_Behaviour);
    }
    #endregion
}
