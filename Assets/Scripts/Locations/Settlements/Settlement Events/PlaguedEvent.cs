using System;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Components;
using Locations.Settlements.Components;
using Plague.Transmission;
using UnityEngine;
using UtilityScripts;
namespace Locations.Settlements.Settlement_Events {
    public class PlaguedEvent : SettlementEvent, FactionEventDispatcher.IListener, NPCSettlementEventDispatcher.IListener, IPlagueTransmissionListener {
        
        public override SETTLEMENT_EVENT eventType => SETTLEMENT_EVENT.Plagued_Event;
        private PLAGUE_EVENT_RESPONSE _rulerDecision;
        private GameDate _endDate;
        private string _endScheduleTicket;

        #region getters
        public PLAGUE_EVENT_RESPONSE rulerDecision => _rulerDecision;
        public GameDate endDate => _endDate;
        #endregion

        public PlaguedEvent(NPCSettlement location) : base(location) {
            _rulerDecision = PLAGUE_EVENT_RESPONSE.Undecided;
        }
        public PlaguedEvent(SaveDataPlaguedSettlementEvent data) : base(data) {
            LoadEnd(data.endDate);
            _rulerDecision = data.rulerDecision;
        }
        
        public override void ActivateEvent(NPCSettlement p_settlement) {
            Character leaderThatWillDecide = GetLeaderThatWillDecideResponse(p_settlement);
            DetermineLeaderResponse(leaderThatWillDecide, p_settlement);
            SubscribeListeners(p_settlement);
            ScheduleEnd(p_settlement);
            //p_settlement.settlementClassTracker.AddNeededClass("Druid");
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}Started Plagued Event at {location.name} initial end date is {_endDate.ToString()}");
#endif
        }
        public override void DeactivateEvent(NPCSettlement p_settlement) {
            if (p_settlement.owner != null) { RevertFactionEffects(p_settlement.owner); }
            
            List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
            p_settlement.PopulateJobsOfType(jobs, JOB_TYPE.PLAGUE_CARE, JOB_TYPE.QUARANTINE);
            for (int i = 0; i < jobs.Count; i++) {
                jobs[i].ForceCancelJob("Settlement no longer in Quarantine");
            }
            RuinarchListPool<JobQueueItem>.Release(jobs);
            UnsubscribeListeners(p_settlement);
            //p_settlement.settlementClassTracker.RemoveNeededClass("Druid");
            if (!string.IsNullOrEmpty(_endScheduleTicket)) { SchedulingManager.Instance.RemoveSpecificEntry(_endScheduleTicket); }
        }
        public override SaveDataSettlementEvent Save() {
            SaveDataPlaguedSettlementEvent saveData = new SaveDataPlaguedSettlementEvent();
            saveData.Save(this);
            return saveData;
        }

        private void SubscribeListeners(NPCSettlement p_settlement) {
            p_settlement.npcSettlementEventDispatcher.SubscribeToFactionOwnerChangedEvent(this);
            p_settlement.npcSettlementEventDispatcher.SubscribeToSettlementRulerChangedEvent(this);
            p_settlement.owner?.factionEventDispatcher.SubscribeToFactionLeaderChangedEvent(this);
            AirborneTransmission.Instance.SubscribeToTransmission(this);
            ConsumptionTransmission.Instance.SubscribeToTransmission(this);
            PhysicalContactTransmission.Instance.SubscribeToTransmission(this);
            CombatRateTransmission.Instance.SubscribeToTransmission(this);
        }
        private void UnsubscribeListeners(NPCSettlement p_settlement) {
            p_settlement.npcSettlementEventDispatcher.UnsubscribeToFactionOwnerChangedEvent(this);
            p_settlement.npcSettlementEventDispatcher.UnsubscribeToSettlementRulerChangedEvent(this);
            p_settlement.owner?.factionEventDispatcher.UnsubscribeToFactionLeaderChangedEvent(this);
            AirborneTransmission.Instance.UnsubscribeToTransmission(this);
            ConsumptionTransmission.Instance.UnsubscribeToTransmission(this);
            PhysicalContactTransmission.Instance.UnsubscribeToTransmission(this);
            CombatRateTransmission.Instance.UnsubscribeToTransmission(this);
        }
        
        #region Scheduled End
        private void ScheduleEnd(NPCSettlement p_settlement) {
            _endDate = GameManager.Instance.Today();
            _endDate = _endDate.AddDays(3);
            _endScheduleTicket = SchedulingManager.Instance.AddEntry(_endDate, () => DeactivateEventBySchedule(p_settlement), this);
        }
        private void RescheduleEnd(NPCSettlement p_settlement) {
            SchedulingManager.Instance.RemoveSpecificEntry(_endScheduleTicket);
            ScheduleEnd(p_settlement);
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}Rescheduled Plagued event end at {p_settlement.name} to {_endDate.ToString()}");
#endif
        }
        private void DeactivateEventBySchedule(NPCSettlement p_settlement) {
            p_settlement.eventManager.DeactivateEvent(this);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Settlement Event", "Plagued", "ended", null, LOG_TAG.Major);
            log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
            if (p_settlement.owner != null) { log.AddInvolvedObjectManual(p_settlement.owner.persistentID); }
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
#endregion
        
#region Leader Response
        private void DetermineLeaderResponse(Character p_leader, NPCSettlement p_settlement) {
            PLAGUE_EVENT_RESPONSE response;
            if (p_leader == null) {
                response = PLAGUE_EVENT_RESPONSE.Do_Nothing;
            } else {
                if (p_leader.traitContainer.HasTrait("Evil", "Psychopath", "Ruthless")) {
                    response = PLAGUE_EVENT_RESPONSE.Slay;    
                } else if (p_leader.traitContainer.HasTrait("Cultist")) {
                    if (p_settlement.owner != null && p_settlement.owner.factionType.type == FACTION_TYPE.Demon_Cult) {
                        if (GameUtilities.RollChance(30)) {
                            response = PLAGUE_EVENT_RESPONSE.Slay;
                        } else if (p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE)) {
                            response = PLAGUE_EVENT_RESPONSE.Quarantine;
                        } else {
                            response = PLAGUE_EVENT_RESPONSE.Exile;
                        }
                    } else {
                        response = PLAGUE_EVENT_RESPONSE.Slay;
                    }
                } else if (p_leader.traitContainer.HasTrait("Diplomatic", "Inspiring")) {
                    response = p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE) ? PLAGUE_EVENT_RESPONSE.Quarantine : PLAGUE_EVENT_RESPONSE.Exile;
                } else if (p_leader.traitContainer.HasTrait("Coward", "Lazy")) {
                    response = PLAGUE_EVENT_RESPONSE.Do_Nothing;
                } else {
                    response = p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE) ? PLAGUE_EVENT_RESPONSE.Quarantine : PLAGUE_EVENT_RESPONSE.Exile;
                }
            }
            if (_rulerDecision != response) {
#if DEBUG_LOG
                Debug.Log($"{p_leader?.name} set plagued event response in {p_settlement.name} to {response}");    
#endif
                RevertEffectsOfLeaderPreviousResponseToPlague(_rulerDecision, p_settlement.owner, p_settlement);
                SetLeaderResponse(response);
                ExecuteEffectsOfLeaderResponseToPlague(response, p_settlement.owner, p_settlement);
                string key = response.ToString();
                if (response == PLAGUE_EVENT_RESPONSE.Do_Nothing && p_leader == null) {
                    key = $"{key}_No_Leader";
                }
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Settlement Event", "Plagued", key, null, LOG_TAG.Major);
                log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
                if (p_leader != null) { log.AddToFillers(p_leader, p_leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER); }
                if (p_settlement.owner != null) { log.AddInvolvedObjectManual(p_settlement.owner.persistentID); }
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
        }
        private void SetLeaderResponse(PLAGUE_EVENT_RESPONSE p_response) {
            _rulerDecision = p_response;
        }
        private void RevertEffectsOfLeaderPreviousResponseToPlague(PLAGUE_EVENT_RESPONSE p_response, Faction p_faction, NPCSettlement p_settlement) {
            switch (p_response) {
                case PLAGUE_EVENT_RESPONSE.Quarantine:
                    List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
                    p_settlement.PopulateJobsOfType(jobs, JOB_TYPE.PLAGUE_CARE, JOB_TYPE.QUARANTINE);
                    for (int i = 0; i < jobs.Count; i++) {
                        jobs[i].ForceCancelJob("Settlement no longer in Quarantine");
                    }
                    RuinarchListPool<JobQueueItem>.Release(jobs);
                    break;
            }
        }
        private void ExecuteEffectsOfLeaderResponseToPlague(PLAGUE_EVENT_RESPONSE p_response, Faction p_faction, NPCSettlement p_settlement) {
            switch (p_response) {
                case PLAGUE_EVENT_RESPONSE.Do_Nothing:
                    p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Infraction);
                    break;
                case PLAGUE_EVENT_RESPONSE.Quarantine:
                    p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Infraction);
                    //NOTE: Job trigger is not yet removed when event is finished since it is expected that plague care will still continue after event if there are still quarantined characters.
                    p_settlement.settlementJobTriggerComponent.AddJobTrigger(p_settlement, SETTLEMENT_JOB_TRIGGER.Plague_Care);
                    break;
                case PLAGUE_EVENT_RESPONSE.Slay:
                    p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Heinous);
                    break;
                case PLAGUE_EVENT_RESPONSE.Exile:
                    p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Serious);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p_response), p_response, null);
            }
            Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, p_faction);
        }
        private Character GetLeaderThatWillDecideResponse(NPCSettlement p_settlement) {
            if (p_settlement.owner != null && p_settlement.owner.leader is Character factionLeader && factionLeader.homeSettlement != null && factionLeader.homeSettlement == p_settlement) {
                return factionLeader;
            } else if (p_settlement.ruler != null) {
                return p_settlement.ruler;
            }
            return null;
        }
#endregion

#region Faction Effects
        private void RevertFactionEffects(Faction p_faction) {
            if (!p_faction.ownedSettlements.Any(s => s is NPCSettlement npcSettlement && npcSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event))) {
                //if faction no longer has any plagued settlements, remove plague as crime. Otherwise retain its current value
                p_faction.factionType.RemoveCrime(CRIME_TYPE.Plagued);
                Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, p_faction);
            }
        }
#endregion
        
#region FactionEventDispatcher.IListener Implementation
        public void OnFactionLeaderChanged(ILeader p_newLeader) {
            if (p_newLeader is Character newFactionLeader && newFactionLeader.homeSettlement != null && newFactionLeader.homeSettlement.eventManager.HasActiveEvent(this)) {
                DetermineLeaderResponse(newFactionLeader, newFactionLeader.homeSettlement);
            }
        }
#endregion

#region NPCSettlementEventDispatcher.IListener Implementation
        public void OnSettlementRulerChanged(Character p_newLeader, NPCSettlement p_settlement) {
            if (p_newLeader != null && GetLeaderThatWillDecideResponse(p_settlement) == p_newLeader) {
                DetermineLeaderResponse(p_newLeader, p_settlement);
            }
        }
        public void OnFactionOwnerChanged(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement) {
            if (p_previousOwner != null) {
                RevertFactionEffects(p_previousOwner);
                p_previousOwner.factionEventDispatcher.UnsubscribeToFactionLeaderChangedEvent(this);    
            }
            p_newOwner?.factionEventDispatcher.SubscribeToFactionLeaderChangedEvent(this);
            if (p_newOwner == null) {
                //settlement no longer has faction owner. End event
                p_settlement.eventManager.DeactivateEvent(this);
            }
        }
#endregion

#region IPlagueTransmissionListener Implementation
        public void OnPlagueTransmitted(IPointOfInterest p_target) {
            if (p_target is Character character && character.homeSettlement != null && character.homeSettlement.eventManager.HasActiveEvent(this)) {
                RescheduleEnd(character.homeSettlement);
            }
        }
#endregion

#region Loading
        private void LoadEnd(GameDate date) {
            _endDate = date;
            _endScheduleTicket = SchedulingManager.Instance.AddEntry(date, () => DeactivateEventBySchedule(location), location);
        }
        public override void LoadAdditionalData(NPCSettlement p_settlement) {
            base.LoadAdditionalData(p_settlement);
            SubscribeListeners(p_settlement);
            if (_rulerDecision == PLAGUE_EVENT_RESPONSE.Quarantine) {
                p_settlement.settlementJobTriggerComponent.AddJobTrigger(p_settlement, SETTLEMENT_JOB_TRIGGER.Plague_Care);
            }
        }
#endregion

#region Utilities
        public static bool HasMinimumAmountOfPlaguedVillagersForEvent(NPCSettlement p_settlement) {
            if (p_settlement.residents.Count >= 1) {
                int totalResidents = p_settlement.residents.Count(r => !r.isDead);
                int plaguedResidents = p_settlement.residents.Count(r => !r.isDead && r.traitContainer.HasTrait("Plagued"));
                float plaguedPercentage = (float)plaguedResidents / (float)totalResidents;
                return plaguedPercentage >= 0.1f;    
            }
            return false;
        }
#endregion

#region For Testing
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info} will end on {endDate.ToString()}({_rulerDecision.ToString()})";
            return info;
        }
#endregion
    }
    
    public class SaveDataPlaguedSettlementEvent : SaveDataSettlementEvent {
        public GameDate endDate;
        public PLAGUE_EVENT_RESPONSE rulerDecision;
        public override void Save(SettlementEvent data) {
            base.Save(data);
            PlaguedEvent e = data as PlaguedEvent;
            endDate = e.endDate;
            rulerDecision = e.rulerDecision;
        }
        public override SettlementEvent Load() {
            return new PlaguedEvent(this);
        }
    }
}

