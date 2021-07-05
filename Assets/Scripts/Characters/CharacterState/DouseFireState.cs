using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;

public class DouseFireState : CharacterState {

    private ITraitable currentTarget;
    private readonly List<ITraitable> _fires;

    private bool isFetchingWater;
    private bool isDousingFire;

    public DouseFireState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Douse Fire State";
        characterState = CHARACTER_STATE.DOUSE_FIRE;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Douse_Icon;
        _fires = new List<ITraitable>();
    }

    #region Overrides
    protected override void StartState() {
        //add initial objects on fire
        // for (int i = 0; i < stateComponent.character.currentRegion.innerMap.activeBurningSources.Count; i++) {
        //     BurningSource burningSource = stateComponent.character.currentRegion.innerMap.activeBurningSources[i];
        //     for (int j = 0; j < burningSource.objectsOnFire.Count; j++) {
        //         ITraitable traitable = burningSource.objectsOnFire[j];
        //         if (traitable is IPointOfInterest pointOfInterest && pointOfInterest.gridTileLocation != null && pointOfInterest.gridTileLocation.
        //             IsPartOfSettlement(stateComponent.character.homeSettlement)) {
        //             AddFire(pointOfInterest);
        //         }
        //     }
        // }
        // for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
        //     IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
        //     AddFire(poi);
        // }
        AddFire(stateComponent.owner);
        base.StartState();
        Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    protected override void EndState() {
        base.EndState();
        Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    private void DetermineAction() {
        if (StillHasFire()) {
            if (HasWater() || NeedsWater() == false) {
                //douse nearest fire
                DouseNearestFire();
            } else {
                //get water from pond
                if (GetWater() == false) {
                    job.AddBlacklistedCharacter(stateComponent.owner);
                    stateComponent.ExitCurrentState();
                }
            }
        } else {
            if (stateComponent.owner.currentActionNode == null && stateComponent.currentState == this) {
                //no more fire, exit state
                stateComponent.ExitCurrentState();
            }
        }
    }
    // public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
    //     if (AddFire(targetPOI)) {
    //         //DetermineAction();
    //         //return true;
    //     }
    //     return base.OnEnterVisionWith(targetPOI);
    // }
    //protected override void DoMovementBehavior() {
    //    base.DoMovementBehavior();
    //    DetermineAction();
    //}
    public override void PauseState() {
        base.PauseState();
        if (isFetchingWater) {
            isFetchingWater = false;
        }
        if (isDousingFire) {
            isDousingFire = false;
        }
        //currentTarget = null;
    }
    public override void PerTickInState() {
        DetermineAction();
        //if (!StillHasFire() && stateComponent.character.currentAction == null && stateComponent.currentState == this) {
        //    //if there is no longer any fire, and the character is still trying to douse fire, exit this state
        //    OnExitThisState();
        //}else if (StillHasFire()) {
        //    DetermineAction();
        //}
    }
    #endregion

    #region Utilities
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (trait is Burning && traitable.gridTileLocation != null && traitable.gridTileLocation.IsPartOfSettlement(stateComponent.owner.homeSettlement)) {
            if (_fires.Contains(traitable) == false) {
                _fires.Add(traitable);
            }
        }
    }
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Burning) {
            if (_fires.Remove(traitable)) {
                if (currentTarget == traitable && removedBy != stateComponent.owner) { 
                    //only redetermine action if burning was removed by something or someone else
                    currentTarget = null;
                    isDousingFire = false;
                    DetermineAction();
                }
            }
        }
    }
    private bool HasWater() {
        return stateComponent.owner.HasItem(TILE_OBJECT_TYPE.WATER_FLASK);
    }
    private bool NeedsWater() {
        return true; 
    }
    private bool StillHasFire() {
        return _fires.Count > 0;
    }
    private bool GetWater() {
        if (isFetchingWater) {
            return true;
        }
        List<TileObject> targets = RuinarchListPool<TileObject>.Claim();
        stateComponent.owner.currentRegion.PopulateTileObjectsOfType(targets, TILE_OBJECT_TYPE.WATER_WELL);
        TileObject nearestWater = null;
        float nearestDist = 0f;
        for (int i = 0; i < targets.Count; i++) {
            TileObject currObj = targets[i];
            float dist = Vector2.Distance(stateComponent.owner.gridTileLocation.localLocation, currObj.gridTileLocation.localLocation);
            if (nearestWater == null || dist < nearestDist) {
                nearestWater = currObj;
                nearestDist = dist;
            }
        }
        RuinarchListPool<TileObject>.Release(targets);
        if (nearestWater != null) {
            stateComponent.owner.marker.GoTo(nearestWater.gridTileLocation, ObtainWater);
            isFetchingWater = true;
            return true;
        }
        Debug.LogWarning($"{stateComponent.owner.name} cannot find any sources of water!");
        return false;
    }
    private void ObtainWater() {
        //character gains 3 water buckets
        stateComponent.owner.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
        stateComponent.owner.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
        stateComponent.owner.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
        isFetchingWater = false; 
    }
    private void DouseNearestFire() {
        if (isDousingFire) {
            return;
        }
#if DEBUG_PROFILER
        Profiler.BeginSample("DouseNearestFire");
#endif
        ITraitable nearestFire = null;
        float nearest = 99999f;
        if (currentTarget != null && currentTarget.worldObject != null && currentTarget.traitContainer.GetTraitOrStatus<Burning>("Burning") != null) {
            nearest = Vector2.Distance(stateComponent.owner.worldObject.transform.position, currentTarget.worldObject.transform.position);
            nearestFire = currentTarget;
        }

        for (int i = 0; i < _fires.Count; i++) {
            ITraitable currFire = _fires[i];
            Burning burning = currFire.traitContainer.GetTraitOrStatus<Burning>("Burning");
            if (burning != null && burning.douser == null) {
                //only consider dousing fire that is not yet assigned
                float dist = Vector2.Distance(stateComponent.owner.worldObject.transform.position, currFire.worldObject.transform.position);
                if (dist < nearest) {
                    nearestFire = currFire;
                    nearest = dist;
                }    
            }
        }
        if (nearestFire != null) {
            isDousingFire = true;
            currentTarget = nearestFire;
            Burning burning = nearestFire.traitContainer.GetTraitOrStatus<Burning>("Burning"); 
            Assert.IsNotNull(burning, $"Burning of {nearestFire} is null.");
            burning.SetDouser(stateComponent.owner);
            stateComponent.owner.marker.GoTo(nearestFire, DouseFire);
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void DouseFire() {
        if (currentTarget != null && currentTarget.traitContainer.RemoveTrait(currentTarget, "Burning", removedBy: this.stateComponent.owner)) {
            if (NeedsWater()) {
                TileObject water = this.stateComponent.owner.GetItem(TILE_OBJECT_TYPE.WATER_FLASK);
                if (water != null) {
                    //Reduce water count by 1.
                    this.stateComponent.owner.UnobtainItem(water);
                }
                currentTarget.traitContainer.AddTrait(currentTarget, "Wet", this.stateComponent.owner);    
            }    
        }
        isDousingFire = false;
        currentTarget = null;
    }
    private bool AddFire(IPointOfInterest poi) {
        Burning burning = poi.traitContainer.GetTraitOrStatus<Burning>("Burning");
        if (burning != null) {
            if (_fires.Contains(poi) == false) {
                _fires.Add(poi);
                return true;
            }
        }
        return false;
    }
#endregion
}

#region Save Data
[System.Serializable]
public class DouseFireStateSaveDataCharacterState : SaveDataCharacterState {

    public POIData[] fires;

    public override void Save(CharacterState state) {
        base.Save(state);
        DouseFireState dfState = state as DouseFireState;
        // fires = new POIData[dfState.fires.Sum(x => x.Value.Count)];
        // int count = 0;
        // foreach (KeyValuePair<BurningSource, List<ITraitable>> kvp in dfState.fires) {
        //     for (int i = 0; i < kvp.Value.Count; i++) {
        //         ITraitable poi = kvp.Value[i];
        //         fires[count] = new POIData(poi);
        //         count++;
        //     }
        // }
    }
}
#endregion