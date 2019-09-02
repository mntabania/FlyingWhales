﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupplyPile : TileObject {
    public LocationStructure location { get; private set; }
    public int suppliesInPile { get; private set; }

    public SupplyPile(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.GET_SUPPLY, INTERACTION_TYPE.DROP_SUPPLY, INTERACTION_TYPE.REPAIR_TILE_OBJECT };
        Initialize(TILE_OBJECT_TYPE.SUPPLY_PILE);
        SetSuppliesInPile(50);
        RemoveTrait("Flammable");
        Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        if(this.state != state) {
            if (!IsAvailable()) {
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckSupply);
            } else {
                Messenger.AddListener(Signals.TICK_STARTED, CheckSupply);
            }
        }
        base.SetPOIState(state);
    }
    #endregion

    private void CheckSupply() {
        if (suppliesInPile < 100) {
            if (!location.location.jobQueue.HasJob(JOB_TYPE.OBTAIN_SUPPLY, this)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_SUPPLY, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = this });
                job.SetCanTakeThisJobChecker(CanCharacterTakeThisJob);
                location.location.jobQueue.AddJobInQueue(job);
            }
        } else {
            location.location.jobQueue.CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION.HAS_SUPPLY, this);
        }
    }
    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    private bool CanCharacterTakeThisJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Supply Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
}
