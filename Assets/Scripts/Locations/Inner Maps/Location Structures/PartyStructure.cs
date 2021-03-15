using System;
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

        public List<IStoredTarget> allPossibleTargets = new List<IStoredTarget>();

        public bool IsAvailableForTargeting() {
            bool isOccupied = charactersHere.Count > 0;
            charactersHere.ForEach((eachCharacters) => isOccupied &= !eachCharacters.isDead);
            return isOccupied;
        }

        public PartyStructure(STRUCTURE_TYPE structure, Region location) : base(structure, location) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        public PartyStructure(Region location, SaveDataDemonicStructure data) : base(location, data) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataPartyStructure saveData = saveDataLocationStructure as SaveDataPartyStructure;
            if (!string.IsNullOrEmpty(saveData.partyID)) {
                party = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(saveData.partyID);
                ListenToParty();
            }
        }
        #endregion

        public Party party;
        public PartyStructureData partyData = new PartyStructureData();
        private bool m_isInitialized = false;

        public void InitializeTeam() {
			if (!m_isInitialized) {
                m_isInitialized = true;
                if (party != null) {
                    party.membersThatJoinedQuest.ForEach((eachMember) => {
                        Debug.LogError(eachMember.name);
                        if (eachMember is Summon) {
                            partyData.deployedSummons.Add(eachMember);
                            SummonSettings ss = CharacterManager.Instance.GetSummonSettings((eachMember as Summon).summonType);
                            partyData.deployedSummonSettings.Add(ss);
                            partyData.deployedCSummonlass.Add(CharacterManager.Instance.GetCharacterClass(ss.className));
                        } else {
                            foreach (PLAYER_SKILL_TYPE eachSkill in PlayerSkillManager.Instance.allMinionPlayerSkills) {
                                if (eachMember.minion.minionPlayerSkillType == eachSkill) {
                                    SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill);
                                    MinionSettings settings = CharacterManager.Instance.GetMintionSettings((skillData as MinionPlayerSkill).minionType);
                                    CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
                                    partyData.deployedMinions.Add(eachMember);
                                    partyData.deployedMinionsSkillType.Add(eachMember.minion.minionPlayerSkillType);
                                    partyData.deployedMinionClass.Add(cClass);
                                }
                            }
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
                partyData.deployedCSummonlass.Remove(p_itemUI.characterClass);
                partyData.deployedSummonSettings.Remove(p_itemUI.summonSettings);
            } else {
                partyData.deployedMinionClass.Remove(p_itemUI.characterClass);
                partyData.deployedMinionsSkillType.Remove(p_itemUI.playerSkillType);
            }
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                partyData.deployedCSummonlass.Add(p_itemUI.characterClass);
                partyData.deployedSummonSettings.Add(p_itemUI.summonSettings);
                AddSummonOnCharacterList(p_itemUI.deployedCharacter);
                p_itemUI.deployedCharacter.SetDestroyMarkerOnDeath(true);
            } else {
                partyData.deployedMinionClass.Add(p_itemUI.characterClass);
                partyData.deployedMinionsSkillType.Add(p_itemUI.playerSkillType);
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

        public virtual void UnDeployAll() {
            partyData.deployedSummons.ForEach((eachSummon) => {
                party.RemoveMember(eachSummon);
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((eachSummon as Summon).summonType, 1);
                party.RemoveMemberThatJoinedQuest(eachSummon);
            });
            for(int x = 0; x < partyData.deployedSummons.Count; ++x) {
                partyData.deployedSummons[x].Death();
			}
            party.RemoveMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedMinions[0].Death();
            partyData.ClearAllData();
            Messenger.Broadcast(PartySignals.UNDEPLOY_PARTY, party);
        }

        public virtual void OnCharacterDied(Character p_deadMonster) {
            for (int x = 0; x < partyData.deployedSummons.Count; ++x) {
                if (p_deadMonster == partyData.deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    partyData.deployedSummons.RemoveAt(x);
                    partyData.deployedSummonSettings.RemoveAt(x);
                    partyData.deployedCSummonlass.RemoveAt(x);
                    break;
                }
            }
            for (int x = 0; x < partyData.deployedMinions.Count; ++x) {
                if (p_deadMonster == partyData.deployedMinions[x]) {
                    partyData.deployedMinionClass.RemoveAt(x);
                    partyData.deployedMinions.RemoveAt(x);
                    partyData.deployedMinionsSkillType.RemoveAt(x);
                    break;
                }
            }

            if(partyData.deployedSummonCount <= 0 && partyData.deployedMinionCount <= 0) {
                partyData.deployedTargets.Clear();
			}
        }

        public virtual void DeployParty() { }

        #region Party.EventsIListener
        public void OnQuestEnds() { UnDeployAll(); party.Unsubscribe(this); }
        public void OnQuestDropped() { UnDeployAll(); party.Unsubscribe(this); }
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
