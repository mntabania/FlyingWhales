using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionIdeology {
    public FACTION_IDEOLOGY ideologyType { get; protected set; }
    public string name { get; protected set; }

    public FactionIdeology(FACTION_IDEOLOGY ideology) {
        ideologyType = ideology;
        name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(ideology.ToString());
    }

    #region Virtuals
    public virtual void SetRequirements(Faction faction) { }
    public virtual bool DoesCharacterFitIdeology(Character character) { return false; }
    public virtual string GetRequirementsForJoiningAsString() { return "None"; }
    #endregion
}
