using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class BurningSource {

    public string persistentID { get; }
    public int id { get; }
    public List<ITraitable> objectsOnFire { get; }

    private int _poisOnFireCount; //Number of POI's currently on fire

    public BurningSource(string overrideID = "") {
        persistentID = string.IsNullOrEmpty(overrideID) ? UtilityScripts.Utilities.GetNewUniqueID() : overrideID;
        id = UtilityScripts.Utilities.SetID(this);
        objectsOnFire = new List<ITraitable>();
        _poisOnFireCount = 0;
        Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        DatabaseManager.Instance.burningSourceDatabase.Register(this);
    }
    public void AddObjectOnFire(ITraitable traitable) {
        if (objectsOnFire.Contains(traitable) == false) {
            objectsOnFire.Add(traitable);
            if (traitable is IPointOfInterest) {
                _poisOnFireCount++;
            }
        }
    }
    private void RemoveObjectOnFire(ITraitable traitable) {
        if (objectsOnFire.Remove(traitable)) {
#if DEBUG_LOG
            Debug.Log($"Removed object on fire {traitable.name} from burning source {id.ToString()}");
#endif
            if (objectsOnFire.Count == 0) {
                SetAsInactive();
            } else if (traitable is IPointOfInterest && _poisOnFireCount > 0){
                //only execute this if there are still more than 0 pois on fire,
                //this is because, if the number is already 0, this part has already been executed before or is currently being processed.
                _poisOnFireCount--;
                if (_poisOnFireCount == 0) {
                    //remove all other burning objects
                    List<ITraitable> remainingBurningObjects = new List<ITraitable>(objectsOnFire);
                    for (int i = 0; i < remainingBurningObjects.Count; i++) {
                        ITraitable objOnFire = remainingBurningObjects[i];
                        objOnFire.traitContainer.RemoveTrait(objOnFire, "Burning");
                    }
                }
            }
        }
    }
    private void SetAsInactive() {
        Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        Messenger.Broadcast(InnerMapSignals.BURNING_SOURCE_INACTIVE, this);
        DatabaseManager.Instance.burningSourceDatabase.UnRegister(this);
    }
    
#region Listeners
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Burning) {
            RemoveObjectOnFire(traitable);
        }
    }
#endregion
    
    public override string ToString() {
        return $"Burning Source {id.ToString()}. Objects: {objectsOnFire.Count.ToString()}";
    }
}