using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class DouseFireBehaviour : CharacterBehaviourComponent {
    
    public DouseFireBehaviour() {
        priority = 950;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (StillHasFire(character)) {
            if (DouseNearestFire(character) == false) {
                //could not find any fires to douse
                character.traitContainer.RemoveTrait(character, "Dousing");
            }
        } else {
            //no more fires, stop dousing fires
            character.traitContainer.RemoveTrait(character, "Dousing");
        }
        return true;
    }

    #region Helpers
    private bool StillHasFire(Character character) {
        if (character.behaviourComponent.dousingFireForSettlement != null) {
            return character.behaviourComponent.dousingFireForSettlement.firesInSettlement.Count > 0;
        }
        return false;
    }
    private bool HasWater(Character character) {
        return character.HasItem(TILE_OBJECT_TYPE.WATER_FLASK);
    }
    #endregion

    #region Douse Fire
    private bool DouseNearestFire(Character character) {
        IPointOfInterest nearestFire = null;
        float nearest = 99999f;

        for (int i = 0; i < character.behaviourComponent.dousingFireForSettlement.firesInSettlement.Count; i++) {
            IPointOfInterest currFire = character.behaviourComponent.dousingFireForSettlement.firesInSettlement[i];
            Burning burning = currFire.traitContainer.GetNormalTrait<Burning>("Burning");
            if (burning != null && burning.douser == null) {
                //only consider dousing fire that is not yet assigned
                float dist = Vector2.Distance(character.worldObject.transform.position, currFire.worldObject.transform.position);
                if (dist < nearest) {
                    nearestFire = currFire;
                    nearest = dist;
                }    
            }
        }
        if (nearestFire != null) {
            Burning burning = nearestFire.traitContainer.GetNormalTrait<Burning>("Burning"); 
            Assert.IsNotNull(burning, $"Burning of {nearestFire} is null.");
            burning.SetDouser(character);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, INTERACTION_TYPE.DOUSE_FIRE,
                nearestFire, character);
            character.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    #endregion

    #region Get Water
    private void GetWater(Character character) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, 
            new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", 
                false, GOAP_EFFECT_TARGET.ACTOR), character, character);
        character.jobQueue.AddJobInQueue(job);
        
    }
    #endregion
    
}