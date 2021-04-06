﻿using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class PartyStructure : DemonicStructure, Party.PartyEventsIListener {
        public override Type serializedData => typeof(SaveDataPartyStructure);
        public virtual List<IStoredTarget> allPossibleTargets { get; }
        protected bool m_isUndeployUserAction;
        public Party party;
        public PartyStructureData partyData = new PartyStructureData();

        private bool m_isInitialized = false;

        public virtual void InitTargets() { }
        public bool IsAvailableForTargeting() {
            if (this is Maraud || this is DefensePoint) {
                return true;
            }
            bool isOccupied = charactersHere.Count > 0;
            int deadCount = 0;
            charactersHere.ForEach((eachCharacter) => {
                if (eachCharacter.isDead) {
                    deadCount++;
                }
            });
            if (charactersHere.Count > deadCount) {
                isOccupied = true;
            }
            return !isOccupied;
        }

        public PartyStructure(STRUCTURE_TYPE structure, Region location) : base(structure, location) {
            Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnTargetRemoved);
        }
        public PartyStructure(Region location, SaveDataDemonicStructure data) : base(location, data) {
            Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnTargetRemoved);
        }

        private void OnGameLoaded() {
            InitializeTeam();
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataPartyStructure saveData = saveDataLocationStructure as SaveDataPartyStructure;
            if (!string.IsNullOrEmpty(saveData.partyID)) {
                party = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(saveData.partyID);
                PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
                ListenToParty();
            }
        }
        #endregion

        public void InitializeTeam() {
            m_isUndeployUserAction = false;
            if (!m_isInitialized) {
                m_isInitialized = true;
                if (party != null) {
                    party.members.ForEach((eachMember) => {
                        if (eachMember is Summon summon) {
                            partyData.deployedSummons.Add(eachMember);
                            SummonSettings ss = CharacterManager.Instance.GetSummonSettings(summon.summonType);
                            partyData.deployedSummonUnderlings.Add(PlayerManager.Instance.player.underlingsComponent.GetSummonUnderlingChargesBySummonType((eachMember as Summon).summonType));
                        } else if (eachMember.minion != null) {
                            MinionPlayerSkill sd = PlayerSkillManager.Instance.GetMinionPlayerSkillData(eachMember.minion.minionPlayerSkillType);
                            partyData.deployedMinions.Add(eachMember);
                            partyData.deployedMinionUnderlings.Add(PlayerManager.Instance.player.underlingsComponent.GetMinionUnderlingChargesByMinionType(sd.minionType));
                        }
                    });
                    if (party.currentQuest != null && party.currentQuest.target != null) {
                        partyData.deployedTargets.Add(party.currentQuest.target as IStoredTarget);
                    }
                }
            }
        }

        public void RemoveItemOnRight(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                partyData.deployedSummonUnderlings.Remove(p_itemUI.obj);
            } else {
                partyData.deployedMinionUnderlings.Remove(p_itemUI.obj);
            }
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                partyData.deployedSummonUnderlings.Add(p_itemUI.obj);
                AddSummonOnCharacterList(p_itemUI.deployedCharacter);
                p_itemUI.deployedCharacter.SetDestroyMarkerOnDeath(true);
            } else {
                partyData.deployedMinionUnderlings.Add(p_itemUI.obj);
                AddMinionOnCharacterList(p_itemUI.deployedCharacter);
                p_itemUI.deployedCharacter.SetDestroyMarkerOnDeath(true);
            }
        }

        public void AddMinionOnCharacterList(Character p_newSummon) {
            partyData.deployedMinions.Add(p_newSummon);
        }

        public void AddSummonOnCharacterList(Character p_newSummon) {
            partyData.deployedSummons.Add(p_newSummon);
        }

        public void AddTargetOnCurrentList(IStoredTarget p_newTarget) {
            if (!partyData.deployedTargets.Contains(p_newTarget)) {
                partyData.deployedTargets.Add(p_newTarget);
            }
        }

        public void RemoveTargetOnCurrentList(IStoredTarget p_newTarget) {
            if (partyData.deployedTargets.Contains(p_newTarget)) {
                partyData.deployedTargets.Remove(p_newTarget);
            }
        }

        public virtual void RemoveCharacterOnList(Character p_removeSummon) {
            partyData.deployedSummons.Remove(p_removeSummon);
        }

        protected void ListenToParty() {
            party.Subscribe(this);
        }

        public void OnQuestFinished() { //both succeed and failed
        
        }

        public virtual void OnCharacterDied(Character p_deadMonster) {
            if (m_isUndeployUserAction) {
                return;
            }
            for (int x = 0; x < partyData.deployedSummons.Count; ++x) {
                if (p_deadMonster == partyData.deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    partyData.deployedSummons.RemoveAt(x);
                    partyData.deployedSummonUnderlings.RemoveAt(x);
                    break;
                }
            }
            for (int x = 0; x < partyData.deployedMinions.Count; ++x) {
                if (p_deadMonster == partyData.deployedMinions[x]) {
                    partyData.deployedMinionUnderlings.RemoveAt(x);
                    partyData.deployedMinions.RemoveAt(x);
                    break;
                }
            }

            if(partyData.deployedSummonCount <= 0 && partyData.deployedMinionCount <= 0) {
                partyData.deployedTargets.Clear();
			}
        }

        void OnTargetRemoved(IStoredTarget p_removedTarget) {
            partyData.deployedTargets.Remove(p_removedTarget);
		}

        public virtual void DeployParty() {
            m_isUndeployUserAction = false;
        }

        public virtual void UnDeployAll() {
            m_isUndeployUserAction = true;
            partyData.deployedSummons.ForEach((eachSummon) => {
                party.RemoveMember(eachSummon);
            });
            List<Character> deployed = RuinarchListPool<Character>.Claim();
            if (partyData.deployedSummons.Count > 0) {
                deployed.AddRange(partyData.deployedSummons);    
            }
            for (int x = 0; x < deployed.Count; x++) {
                deployed[x].Death();
            }
            if (partyData.deployedMinions.Count > 0) {
                party.RemoveMember(partyData.deployedMinions[0]);
                partyData.deployedMinions[0].Death();    
            }
            RuinarchListPool<Character>.Release(deployed);
            if (partyData.deployedTargets.Count > 0) {
                partyData.deployedTargets[0].isTargetted = false;
            }
            partyData.ClearAllData();
            Messenger.Broadcast(PartySignals.UNDEPLOY_PARTY, party);
            Debug.Log($"Un Deployed party at {name}. Party was {party?.name}");
            party = null;
        }

        public void ResetExistingCharges() {
            //Do not add charge anymore because when a minion dies the charge is automatically added
            //partyData.deployedMinions.ForEach((eachMinion) => {
            //    PlayerSkillManager.Instance.GetMinionPlayerSkillData(eachMinion.minion.minionPlayerSkillType).AdjustCharges(1);
            //});
            partyData.deployedSummons.ForEach((eachSummon) => {
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((eachSummon as Summon).summonType, 1);
            });
        }
        #region Party.EventsIListener
        public void OnQuestSucceed() {
            if (!m_isUndeployUserAction) {
                ResetExistingCharges();
                party.Unsubscribe(this);
                UnDeployAll();
            }
        }
        public void OnQuestFailed() {
            if (!m_isUndeployUserAction) {
                ResetExistingCharges();
                party.Unsubscribe(this);
                UnDeployAll();
            }
        }
        #endregion
    }

    public class SaveDataPartyStructure : SaveDataDemonicStructure {
        public string partyID;

        public override void Save(LocationStructure structure) {
            base.Save(structure);
            PartyStructure ps = structure as PartyStructure;
            if (ps.party != null) {
                partyID = ps.party.persistentID;
            }
        }
    }
}
