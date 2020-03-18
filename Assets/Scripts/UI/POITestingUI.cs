using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class POITestingUI : MonoBehaviour {
    //This script is used to test characters and actions
    //Most of the functions here will only work if there is a currently clicked/active character
    public RectTransform rt;
    public IPointOfInterest poi { get; private set; }
    public LocationGridTile gridTile { get; private set; }

    #region Utilities
    public void ShowUI(IPointOfInterest poi) {
        this.poi = poi;
        UIManager.Instance.HideSmallInfo();
        UIManager.Instance.PositionTooltip(gameObject, rt, rt);
        gameObject.SetActive(true);
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);
    }
    public void ShowUI(LocationGridTile gridTile) {
        if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
            this.gridTile = gridTile;
            UIManager.Instance.HideSmallInfo();
            UIManager.Instance.PositionTooltip(gameObject, rt, rt);
            gameObject.SetActive(true);
            Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);
        }
    }
    public void HideUI() {
        gameObject.SetActive(false);
        this.poi = null;
        this.gridTile = null;
        Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);
    }
    private void OnKeyPressed(KeyCode keyCode) {
        if (keyCode == KeyCode.Mouse0 && UIManager.Instance.IsMouseOnUI() == false) {
            HideUI();
        } else if (keyCode == KeyCode.Escape) {
            HideUI();
        }
    }
    #endregion

    #region Character Testing
    public void KnockoutThisCharacter() {
        if(poi is Character) {
            CreateKnockoutJob(UIManager.Instance.characterInfoUI.activeCharacter, poi as Character);
        } else if (poi is Bed) {
            Bed bed = poi as Bed;
            if(bed.users[0] != null) {
                CreateKnockoutJob(UIManager.Instance.characterInfoUI.activeCharacter, bed.users[0]);
            }else if (bed.users[1] != null) {
                CreateKnockoutJob(UIManager.Instance.characterInfoUI.activeCharacter, bed.users[1]);
            }
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    public bool CreateKnockoutJob(Character character, Character targetCharacter) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, character);
        character.jobQueue.AddJobInQueue(job);
        character.logComponent.PrintLogIfActive(
            $"Added a KNOCKOUT Job to {this.name} with target {targetCharacter.name}");
        return true;
    }
    public void ChatWithThisCharacter() {
        if (poi is Character) {
            Character source = UIManager.Instance.characterInfoUI.activeCharacter;
            Character target = poi as Character;
            if(!source.isConversing && !target.isConversing) {
                source.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, poi);
            }
            //UIManager.Instance.characterInfoUI.activeCharacter.nonActionEventsComponent.ForceChatCharacter(poi as Character);
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    public void InviteToMakeLove() {
        if (poi is Character) {
            Character target = poi as Character;
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.MAKE_LOVE, target, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
            //if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.AFFAIR)) {
            //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, target);
            //    job.SetCannotOverrideJob(true);
            //    UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
            //} else {
            //    Debug.LogError("Must be affair or lover!");
            //}
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    public void StealFromThisCharacter() {
        if (poi is Character) {
            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.STEAL_FROM_CHARACTER, poi);
            //job.SetCannotOverrideJob(true);
            //UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    public void DrinkBlood() {
        if (poi is Character) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.DRINK_BLOOD, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    public void Feed() {
        if (poi is Character) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.FEED, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a character!");
        }
        HideUI();
    }
    #endregion

    #region Tile Object Testing
    public void PoisonTable() {
        if (poi is Table) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE, INTERACTION_TYPE.POISON, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a table!");
        }
        HideUI();
    }
    public void EatAtTable() {
        if (poi is Table) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, INTERACTION_TYPE.EAT, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a table!");
        }
        HideUI();
    }
    public void Sleep() {
        if (poi is Bed) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError($"{poi.name} is not a bed!");
        }
        HideUI();
    }
    #endregion

    #region Grid Tile Testing
    public void GoHere() {
        //STRUCTURE_TYPE[] _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
        UIManager.Instance.characterInfoUI.activeCharacter.marker.GoTo(this.poi.gridTileLocation/*, notAllowedStructures: _notAllowedStructures*/);
        HideUI();
    }
    public void AddRandomArtifact() {
        //STRUCTURE_TYPE[] _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
        ARTIFACT_TYPE[] types = UtilityScripts.CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
        bool hasAdded = false;
        while (!hasAdded) {
            ARTIFACT_TYPE chosenType = types[UnityEngine.Random.Range(0, types.Length)];
            if (chosenType != ARTIFACT_TYPE.None) {
                hasAdded = true;
                LocationGridTile tile = poi.gridTileLocation.GetNearestUnoccupiedTileFromThis();
                Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(chosenType);
                tile.structure.AddPOI(artifact, tile);
            }
        }
        HideUI();
    }
    #endregion
}
