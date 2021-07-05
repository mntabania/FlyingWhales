using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Hunting : Status {

        private Character _owner;
        public Area targetArea { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataHunting);
        #endregion
        
        public Hunting() {
            name = "Hunting";
            description = "This is Dousing fires.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(5);
            isHidden = true;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataHunting saveDataHunting = saveDataTrait as SaveDataHunting;
            Assert.IsNotNull(saveDataHunting);
            targetArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataHunting.targetAreaID);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                _owner = character;
                //Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                character.behaviourComponent.AddBehaviourComponent(typeof(HuntPreyBehaviour));
                //Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(HuntPreyBehaviour));
                //Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
            }
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Hunting status) {
                targetArea = status.targetArea;
            }
        }
        #endregion

        public void SetTargetArea(Area p_area) {
            targetArea = p_area;
        }
        
        //private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob goapPlanJob) {
        //    if (character == _owner && goapPlanJob.targetInteractionType == INTERACTION_TYPE.EAT_CORPSE) {
        //        character.traitContainer.RemoveTrait(character, this);
        //    }
        //}
    }
}

#region Save Data
public class SaveDataHunting : SaveDataTrait {

    public string targetAreaID;
    
    public override void Save(Trait trait) {
        base.Save(trait);
        Hunting hunting = trait as Hunting;
        Assert.IsNotNull(hunting);
        targetAreaID = hunting.targetArea.persistentID;
    }
}
#endregion