﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class TaskFilter {
	protected Character _character2;

    #region virtuals
	public virtual bool MeetsRequirements(Character character) {
        return true;
    }
    #endregion

	public void SetSecondCharacter(Character character2){
		_character2 = character2;
	}
}

public class MustNotHaveTraits : TaskFilter {

    private List<TRAIT> _traits;

    public MustNotHaveTraits(List<TRAIT> traits) {
        _traits = traits;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if (character.HasTrait(_traits[i])) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustHaveTraits : TaskFilter {

    private List<TRAIT> _requiredTraits;

    public MustHaveTraits(List<TRAIT> requiredTraits) {
        _requiredTraits = requiredTraits;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        for (int i = 0; i < _requiredTraits.Count; i++) {
            if (!character.HasTrait(_requiredTraits[i])) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustNotHaveTags : TaskFilter {

	private List<CHARACTER_TAG> _tags;

	public MustNotHaveTags(List<CHARACTER_TAG> tags) {
		_tags = tags;
	}
	public MustNotHaveTags(CHARACTER_TAG tag) {
		_tags = new List<CHARACTER_TAG>();
		_tags.Add (tag);
	}

	#region overrides
	public override bool MeetsRequirements(Character character) {
		for (int i = 0; i < _tags.Count; i++) {
			if (character.HasTag(_tags[i])) {
				return false;
			}
		}
		return true;
	}
	#endregion
}

public class MustHaveTags : TaskFilter {

	private List<CHARACTER_TAG> _tags;

	public MustHaveTags(List<CHARACTER_TAG> tags) {
		_tags = tags;
	}
	public MustHaveTags(CHARACTER_TAG tag) {
		_tags = new List<CHARACTER_TAG>();
		_tags.Add (tag);
	}

	#region overrides
	public override bool MeetsRequirements(Character character) {
		for (int i = 0; i < _tags.Count; i++) {
			if (!character.HasTag(_tags[i])) {
				return false;
			}
		}
		return true;
	}
	#endregion
}

//public class MustBeRole : QuestFilter {
//
//    private CHARACTER_ROLE _requiredRole;
//
//    public MustBeRole(CHARACTER_ROLE requiredRole) {
//        _requiredRole = requiredRole;
//    }
//
//    #region overrides
//    public override bool MeetsRequirements(Character character) {
//        if(character.role != null) {
//			return character.role.roleType == _requiredRole;
//        }
//        return false;
//    }
//    #endregion
//}

public class MustBeClass : TaskFilter {

	private List<CHARACTER_CLASS> _allowedClasses;

	public MustBeClass(CHARACTER_CLASS requiredClass) {
        _allowedClasses = new List<CHARACTER_CLASS>();
        _allowedClasses.Add(requiredClass);
    }
    public MustBeClass(List<CHARACTER_CLASS> requiredClasses) {
        _allowedClasses = requiredClasses;
    }
    #region overrides
    public override bool MeetsRequirements(Character character) {
        //TODO change enum parsing if possible
        return _allowedClasses.Contains((CHARACTER_CLASS)System.Enum.Parse(typeof(CHARACTER_CLASS), character.characterClass.className, true));
    }
    #endregion
}

public class MustHaveSkills : TaskFilter {
    //TODO: Create MustHaveSkill OldQuest.Quest Filter on Merge of Combat Prototype 
}

public class MustBeFaction : TaskFilter {

    private List<Faction> _allowedFactions;

    public MustBeFaction(List<Faction> allowedFactions) {
        _allowedFactions = allowedFactions;
    }
    public MustBeFaction(Faction allowedFaction) {
        _allowedFactions = new List<Faction>();
        _allowedFactions.Add(allowedFaction);
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        if(character.faction != null) {
            if (_allowedFactions.Contains(character.faction)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public class MustNotBeFaction : TaskFilter {

    private List<Faction> _bannedFactions;

    public MustNotBeFaction(List<Faction> bannedFactions) {
        _bannedFactions = bannedFactions;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        if (character.faction != null) {
            if (_bannedFactions.Contains(character.faction)) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustBeCharacter : TaskFilter {

    private List<Character> _allowedCharacters;

    public MustBeCharacter(List<Character> allowedCharacters) {
        _allowedCharacters = allowedCharacters;
    }
    public MustBeCharacter(Character allowedCharacter) {
        _allowedCharacters = new List<Character>();
        _allowedCharacters.Add(allowedCharacter);
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        return _allowedCharacters.Contains(character);
    }
    #endregion
}

public class MustNotBeCharacter : TaskFilter {

    private List<Character> _bannedCharacters;

    public MustNotBeCharacter(List<Character> bannedCharacetrs) {
        _bannedCharacters = bannedCharacetrs;
    }
    public MustNotBeCharacter(Character bannedCharacter) {
        _bannedCharacters = new List<Character>();
        _bannedCharacters.Add(bannedCharacter);
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        return !_bannedCharacters.Contains(character);
    }
    #endregion
}

public class MustHaveItem : TaskFilter {

	private string _itemName;

	public MustHaveItem(string itemName) {
		_itemName = itemName;
	}

	public MustHaveItem() {
		_itemName = string.Empty;
	}

	#region overrides
	public override bool MeetsRequirements(Character character) {
		if(_itemName != string.Empty){
			if(character.GetItemInAll(_itemName) != null) {
				return true;
			}
		}else{
			if(character.inventory.Count > 0){
				return true;
			}
		}
		return false;
	}
	#endregion
}

public class MustBeRelationship : TaskFilter {

    private CHARACTER_RELATIONSHIP _relationshipStatus;

    public MustBeRelationship(CHARACTER_RELATIONSHIP relationshipStatus) {
        _relationshipStatus = relationshipStatus;
    }

	#region overrides
	public override bool MeetsRequirements(Character character) {
		Relationship relationship = character.GetRelationshipWith (_character2);
        if (relationship != null && relationship.HasStatus(CHARACTER_RELATIONSHIP.ENEMY)) {
            return true;
        }
        return false;
	}
	#endregion
}