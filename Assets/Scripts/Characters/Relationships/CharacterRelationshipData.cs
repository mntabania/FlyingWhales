﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRelationshipData {

    public Character targetCharacter { get; private set; }

    public List<RelationshipTrait> rels { get; private set; }
    public int lastEncounter { get; private set; } //counts up
    public float encounterMultiplier { get; private set; }
    public bool isCharacterMissing { get; private set; }
    public bool isCharacterLocated { get; private set; }
    public LocationStructure knownStructure { get; private set; }
    public Trait trouble { get; private set; } //Set this to trait for now, but this could change in future iterations

    public CharacterRelationshipData(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
        rels = new List<RelationshipTrait>();
        lastEncounter = 0;
        encounterMultiplier = 0f;
        isCharacterMissing = false;
        isCharacterLocated = true;
        knownStructure = targetCharacter.homeStructure;
        trouble = null;
    }

    #region Relationships
    public void AddRelationship(RelationshipTrait newRel) {
        if (!rels.Contains(newRel)) {
            rels.Add(newRel);
        }
    }
    public void RemoveRelationship(RelationshipTrait newRel) {
        rels.Remove(newRel);
    }
    #endregion

    #region Encounter Multiplier
    public void AdjustEncounterMultiplier(float adjustment) {
        encounterMultiplier += adjustment;
    }
    public void ResetEncounterMultiplier() {
        encounterMultiplier = 0;
    }
    #endregion
}
