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
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataAngry dataTrait = p_saveDataTrait as SaveDataAngry;
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
                if (character.HasAfflictedByPlayerWith("Hothead")) {
                    DispenseChaosOrbsForAffliction(character, 1);
                }
                character.marker.visionCollider.VoteToUnFilterVision();
                Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                if (character.marker) {
                    character.marker.visionCollider.VoteToUnFilterVision();
                    Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);    
                }
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
                if (character.marker) { //TODO: Find out why character.marker can be null in this situation. Bug happened when this trait was removed (by schedule) from a character that no longer has a marker 
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
#if DEBUG_LOG
                string log = $"{GameManager.Instance.TodayLogString()}{characterThatWillDoJob.name} saw {targetCharacter.name}";
#endif
                if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Critical) {
#if DEBUG_LOG
                    log += "\n -In critical mood";
#endif
                    int combatChance = 0;
                    if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Enemy)) {
                        combatChance = 10;
                    } else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival)) {
                        combatChance = 25;
                    }
                    int roll = Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
#endif
                    if (roll < combatChance) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: true);    
                    }
                } else if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Bad) {
#if DEBUG_LOG
                    log += "\n -In low mood";
#endif
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
#if DEBUG_LOG
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
#endif
                    if (roll < combatChance) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: false);    
                    }
                } else {
#if DEBUG_LOG
                    log += "\n -In normal mood";
#endif
                    int combatChance = 2;
                    int roll = Random.Range(0, 100);
#if DEBUG_LOG
                    log += $"\nCombat chance is {combatChance.ToString()}. Roll is {roll.ToString()}";
#endif
                    if (roll < combatChance && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)
                        && !targetCharacter.traitContainer.HasTrait("Unconscious")) {
                        characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: false);
                    }
                }

#if DEBUG_LOG
                Debug.Log(log);
#endif
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if(statusToCopy is Angry status) {
                _responsibleCharactersStack.AddRange(status.responsibleCharactersStack);
            }
        }
#endregion

        private void PerHourEffect() {
            if (owner.limiterComponent.canPerform && owner.limiterComponent.canMove && !owner.isDead && owner != null 
                && owner.hasMarker && owner.marker.inVisionTileObjects.Count > 0
                && !owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.DESTROY) && Random.Range(0, 100) < 8) {
                List<TileObject> choices = owner.marker.inVisionTileObjects
                    .Where(x => x.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && x.tileObjectType != TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT).ToList();
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

