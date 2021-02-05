using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class NatureWorship : FactionIdeology {

    public NatureWorship() : base(FACTION_IDEOLOGY.Nature_Worship) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Inclusive ideology accepts all characters
        return true;
    }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "Worships Father Nature.";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Divine_Worship);
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Demon_Worship);
        factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
    }
    #endregion
}
