using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExplorationParty : Party {

    public LocationStructure targetStructure { get; private set; }

    private List<LocationStructure> alreadyExplored;
    private bool isExploring;
    private int currentChance;

    #region getters
    public override IPartyTarget target => targetStructure;
    #endregion

    public ExplorationParty() : base(PARTY_TYPE.Exploration) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(ExploreBehaviour);
        jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
        alreadyExplored = new List<LocationStructure>();
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        currentChance = 100;
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        ProcessExplorationOrDisbandment();
        for (int i = 0; i < members.Count; i++) {
            Character member = members[i];
            member.traitContainer.AddTrait(member, "Travelling");
        }
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
    }
    #endregion

    #region General
    private void ProcessExplorationOrDisbandment() {
        if(UnityEngine.Random.Range(0, 100) < currentChance) {
            ProcessSettingTargetStructure();
        } else {
            DisbandParty();
        }
    }
    private void ProcessSettingTargetStructure() {
        LocationStructure target = leader.currentRegion.GetRandomSpecialStructureExcept(alreadyExplored);
        if(target != null) {
            SetTargetStructure(target);
        } else {
            List<Region> adjacentRegions = leader.currentRegion.AdjacentRegions();
            if(adjacentRegions != null) {
                for (int i = 0; i < adjacentRegions.Count; i++) {
                    target = adjacentRegions[i].GetRandomSpecialStructureExcept(alreadyExplored);
                    if(target != null) {
                        SetTargetStructure(target);
                        break;
                    }
                }
            }
        }
        if(target == null) {
            DisbandParty();
        }
    }
    private void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if (targetStructure != null) {
                alreadyExplored.Add(targetStructure);
            }
        }
    }
    #endregion

    #region Exploration Timer
    private void StartExplorationTimer() {
        if (!isExploring) {
            isExploring = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
            SchedulingManager.Instance.AddEntry(dueDate, DoneExplorationTimer, this);
        }
    }
    private void DoneExplorationTimer() {
        if (isExploring) {
            currentChance -= 20;
            ProcessExplorationOrDisbandment();
            isExploring = false;
        }
    }
    #endregion

    #region Listeners
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (targetStructure == structure) {
            if (IsMember(character)) {
                StartExplorationTimer();
            }
        }
    }
    #endregion
}
