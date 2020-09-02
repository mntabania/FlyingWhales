using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Traits {
    public class Angry : Status {

        private Character owner;

        private readonly List<Character> _responsibleCharactersStack; //list of characters that have added this status to the owner, this can contain duplicates of a character
        public List<Character> responsibleCharactersStack => _responsibleCharactersStack;

        #region getters
        public override Type serializedData => typeof(SaveDataAngry);
        #endregion
        
        public Angry() {
            name = "Angry";
            description = "Something or someone has made it mad!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            hindersSocials = true;
            _responsibleCharactersStack = new List<Character>();
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            //effects = new List<TraitEffect>();
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataAngry dataTrait = saveDataTrait as SaveDataAngry;
            Assert.IsNotNull(dataTrait);
            _responsibleCharactersStack.AddRange(SaveUtilities.ConvertIDListToCharacters(dataTrait.characterIDs));
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                if (character.isDead) {
                    Debug.LogWarning($"{GameManager.Instance.TodayLogString()}{character.name} is already dead but gained an angry status!");
                }
                owner = character;

                //adjust opinion of responsible characters because
                //AddCharacterToStackList did not do it yet since owner has not been set yet.
                for (int i = 0; i < responsibleCharactersStack.Count; i++) {
                    Character otherCharacter = responsibleCharactersStack[i];
                    if (otherCharacter != null) {
                        owner.relationshipContainer.AdjustOpinion(owner, otherCharacter, "Anger", -30);    
                    }
                }
                
                character.marker.visionCollider.VoteToUnFilterVision();
                Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                //make sure to remove any lingering opinion adjustments, just in case
                int remainingCharacterCount = responsibleCharactersStack.Count;
                for (int i = 0; i < remainingCharacterCount; i++) {
                    RemoveOldestCharacterFromStackList();
                }
                owner = null;
                if (character.marker != null) { //TODO: Find out why character.marker can be null in this situation. Bug happened when this trait was removed (by schedule) from a character that no longer has a marker 
                    character.marker.visionCollider.VoteToFilterVision();    
                }
                Messenger.RemoveListener(Signals.HOUR_STARTED, PerHourEffect);
            }
        }
        public override void AddCharacterResponsibleForTrait(Character character) {
            base.AddCharacterResponsibleForTrait(character);
            if (character != null) {
                AddCharacterToStackList(character);    
            }
        }
        public override void OnUnstackStatus(ITraitable addedTo) {
            base.OnUnstackStatus(addedTo);
            RemoveOldestCharacterFromStackList();
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            //if (targetPOI is TileObject tileObject) { // || targetPOI is SpecialToken
            //    if (tileObject.mapObjectVisual.IsInvisible() == false && 
            //        tileObject.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && 
            //        Random.Range(0, 100) < 3) {
            //        return characterThatWillDoJob.jobComponent.TriggerDestroy(targetPOI);
            //    }
            //} else 
            if (targetPOI is Character targetCharacter) {
                string log = $"{GameManager.Instance.TodayLogString()}{characterThatWillDoJob.name} saw {targetCharacter.name}";
                if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Critical) {
                    log += "\n -In critical mood";
                    int combatChance = 0;
                    if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Enemy)) {
                        combatChance = 10;
                    } else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival)) {
                        combatChance = 25;
                    }
                    int roll = Random.Range(0, 100);
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
                    if (roll < combatChance) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: true);    
                    }
                } else if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Bad) {
                    log += "\n -In low mood";
                    int combatChance = 0;
                    if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Enemy)) {
                        if (targetCharacter.traitContainer.HasTrait("Unconscious") == false) {
                            combatChance = 10;
                        }
                    } else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival)) {
                        if (targetCharacter.traitContainer.HasTrait("Unconscious") == false) {
                            combatChance = 25;
                        }
                    }    
                    int roll = Random.Range(0, 100);
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
                    if (roll < combatChance) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: false);    
                    }
                } else {
                    log += "\n -In normal mood";
                    int combatChance = 2;
                    int roll = Random.Range(0, 100);
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
                    if (roll < combatChance && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)
                        && !targetCharacter.traitContainer.HasTrait("Unconscious")) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: false);
                    }
                }
                
                Debug.Log(log);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
        
        private void PerHourEffect() {
            if (owner.canPerform && owner.canMove && !owner.isDead && owner != null 
                && owner.marker != null && owner.marker.inVisionTileObjects.Count > 0
                && !owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.DESTROY) && Random.Range(0, 100) < 8) {
                List<TileObject> choices = owner.marker.inVisionTileObjects
                    .Where(x => x.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT).ToList();
                if (choices.Count > 0 && owner.jobQueue.HasJob(JOB_TYPE.DESTROY) == false) {
                    TileObject tileObject = CollectionUtilities.GetRandomElement(choices);
                    owner.jobComponent.TriggerDestroy(tileObject);
                }
            }
        }
        private void AddCharacterToStackList(Character character){
            responsibleCharactersStack.Add(character);
            //add opinion modifier
            owner?.relationshipContainer.AdjustOpinion(owner, character, "Anger", -30);
        }
        private void RemoveOldestCharacterFromStackList() {
            if (responsibleCharactersStack.Count > 0) {
                Character character = responsibleCharactersStack[0];
                responsibleCharactersStack.RemoveAt(0);
                owner.relationshipContainer.AdjustOpinion(owner, character, "Anger", 30);
            }
        }
    }
}

#region Save Data
public class SaveDataAngry : SaveDataTrait {
    public List<string> characterIDs;
    public override void Save(Trait trait) {
        base.Save(trait);
        Angry angry = trait as Angry;
        Assert.IsNotNull(angry);
        characterIDs = SaveUtilities.ConvertSavableListToIDs(angry.responsibleCharactersStack);
    }
}
#endregion

