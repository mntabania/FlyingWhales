using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipProcessor : IRelationshipProcessor {

    public void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType) {
        Character character1 = rel1 as Character;
        Character character2 = rel2 as Character;
        string relString = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(relType.ToString());

        character1.relationshipContainer.AdjustOpinion(character1, character2, relString, 0);

        switch (relType) {
            case RELATIONSHIP_TYPE.LOVER:
                if (character1.homeSettlement != null && character2.homeSettlement != null && character1.homeRegion == character2.homeRegion
                    && character1.homeStructure != character2.homeStructure) {
                    if(character1.homeStructure == null && character2.homeStructure != null) {
                        character1.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character2);
                    } else if (character1.homeStructure != null && character2.homeStructure == null) {
                        character2.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character1);
                    } else {
                        //Lover conquers all, even if one character is factionless they will be together, meaning the factionless character will still have home structure
                        character1.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character2);
                    }
                }
                break;
        }
    }

    public void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE relType) {
        Character character1 = rel1 as Character;
        Character character2 = rel2 as Character;
        string relString = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(relType.ToString());
        character1.relationshipContainer.RemoveOpinion(character2, relString);
    }
}
