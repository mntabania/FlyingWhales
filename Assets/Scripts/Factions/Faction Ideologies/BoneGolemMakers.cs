using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class BoneGolemMakers : FactionIdeology {
    public BoneGolemMakers() : base(FACTION_IDEOLOGY.Bone_Golem_Makers) {

    }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        //Bone Golem Makers ideology accepts all characters
        return true;
    }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        return true;
    }
    public override string GetIdeologyDescription() {
        return "May produce Bone Golems to defend their Settlements.";
    }
    public override void OnAddIdeology(FactionType factionType) {

    }
    #endregion
}