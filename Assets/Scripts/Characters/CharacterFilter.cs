using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterFilter {

    public Faction requiredFaction;
    public List<CHARACTER_ROLE> allowedRoles;
    public List<CHARACTER_CLASS> allowedClasses;

    public CharacterFilter(Faction requiredFaction = null, List<CHARACTER_ROLE> allowedRoles = null, List<CHARACTER_CLASS> allowedClasses = null) {
        this.requiredFaction = requiredFaction;
        this.allowedRoles = allowedRoles;
        this.allowedClasses = allowedClasses;
    }

    public bool MeetsRequirements(Character character) {
        if(requiredFaction != null) {
            if (character.faction != requiredFaction) {
                return false;
            }
        }
        // if (allowedRoles != null) {
        //     if (character.role == null) {
        //         return false;
        //     } else {
        //         if (!allowedRoles.Contains(character.role.roleType)) {
        //             return false;
        //         }
        //     }
        // }
        if (allowedClasses != null) {
            if (character.characterClass == null) {
                return false;
            } else {
                if (character.characterClass != null && !character.characterClass.className.Equals("Classless")) {
                    CHARACTER_CLASS charClass = (CHARACTER_CLASS)System.Enum.Parse(typeof(CHARACTER_CLASS), character.characterClass.className, true);
                    if (!allowedClasses.Contains(charClass)) {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
