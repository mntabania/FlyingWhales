using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultOtherStructure : CharacterBehaviourComponent {
    public DefaultOtherStructure() {
        priority = 70;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.currentStructure.isInterior && character.currentStructure != character.homeStructure
             && character.trapStructure.IsTrapped() == false) {
            // if (((character.currentStructure.structureType == STRUCTURE_TYPE.DWELLING && character.currentStructure != character.homeStructure)
            //      || character.currentStructure.structureType == STRUCTURE_TYPE.INN
            //      || character.currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE
            //      || character.currentStructure.structureType == STRUCTURE_TYPE.PRISON
            //      || character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY
            //      || character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER)
            //     && character.trapStructure.IsTrapped() == false) {
            log +=
                $"\n-{character.name} is in Interior Structure and Base Structure is empty";
            log += "\n-100% chance to return home";
            character.PlanIdleReturnHome();
            return true;
        }
        return false;
    }
}
