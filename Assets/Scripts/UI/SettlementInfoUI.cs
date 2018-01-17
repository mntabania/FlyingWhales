﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SettlementInfoUI : UIMenu {

    internal bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel settlementInfoLbl;

    internal Settlement currentlyShowingSettlement;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateSettlementInfo);
        //tweenPos.AddOnFinished(() => UpdateSettlementInfo());
    }

    public void ShowSettlementInfo() {
        isShowing = true;
        //tweenPos.PlayForward();
        gameObject.SetActive(true);
        UpdateSettlementInfo();
    }
    public void HideSettlementInfo() {
        isShowing = false;
        //tweenPos.PlayReverse();
        gameObject.SetActive(false);
    }

    public void SetSettlementAsActive(Settlement settlement) {
        currentlyShowingSettlement = settlement;
        if (isShowing) {
            UpdateSettlementInfo();
        }
    }

    public void UpdateSettlementInfo() {
        if(currentlyShowingSettlement == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Location:[/b] " + currentlyShowingSettlement.location.name;

        if (currentlyShowingSettlement.owner != null) {
			text += "\n[b]Owner:[/b] " + "[url=" + currentlyShowingSettlement.owner.id + "_faction]" + currentlyShowingSettlement.owner.name + "[/url]" + "/" + currentlyShowingSettlement.owner.race.ToString();
            text += "\n[b]Total Population: [/b] " + currentlyShowingSettlement.totalPopulation.ToString();
            text += "\n[b]Civilian Population: [/b] " + currentlyShowingSettlement.civilians.ToString();
            text += "\n[b]Population Growth: [/b] " + (currentlyShowingSettlement.totalPopulation * currentlyShowingSettlement.location.region.populationGrowth).ToString();
            text += "\n[b]Characters: [/b] ";
            if (currentlyShowingSettlement.charactersOnLandmark.Count > 0) {
                for (int i = 0; i < currentlyShowingSettlement.charactersOnLandmark.Count; i++) {
                    ECS.Character currChar = currentlyShowingSettlement.charactersOnLandmark[i];
					text += "\n" + "[url=" + currChar.id + "_character]" + currChar.name  + "[/url]" + " - " + currChar.characterClass.className + "/" + currChar.role.roleType.ToString();
                    if (currChar.currentQuest != null) {
                        text += " (" + currChar.currentQuest.questType.ToString() + ")";
                    }
                }
            } else {
                text += "NONE";
            }

            text += "\n[b]Active Quests: [/b] ";
            if (currentlyShowingSettlement.owner.internalQuestManager.activeQuests.Count > 0) {
                for (int i = 0; i < currentlyShowingSettlement.owner.internalQuestManager.activeQuests.Count; i++) {
                    Quest currQuest = currentlyShowingSettlement.owner.internalQuestManager.activeQuests[i];
                    text += "\n" + currQuest.questType.ToString();
                    if (currQuest.questType == QUEST_TYPE.EXPLORE_REGION) {
                        text += " " + ((ExploreRegion)currQuest).regionToExplore.centerOfMass.name;
                    }
                    if (currQuest.isAccepted) {
                        text += " - A";
                    } else {
                        text += " - N";
                    }
                    
                }
            } else {
                text += "NONE";
            }
        }
        text += "\n[b]Technologies: [/b] ";
        List<TECHNOLOGY> availableTech = currentlyShowingSettlement.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        if (availableTech.Count > 0) {
            text += "\n";
            for (int i = 0; i < availableTech.Count; i++) {
                TECHNOLOGY currTech = availableTech[i];
                text += currTech.ToString();
                if (i + 1 != availableTech.Count) {
                    text += ", ";
                }
            }
        } else {
            text += "NONE";
        }
        text += "\n[b]Parties: [/b] ";
        List<Party> partiesInSettlement = currentlyShowingSettlement.GetPartiesInSettlement();
        if (PartyManager.Instance.allParties.Count > 0) {
            for (int i = 0; i < PartyManager.Instance.allParties.Count; i++) {
                Party currParty = PartyManager.Instance.allParties[i];
                text += "\n" + currParty.name + " O: " + currParty.isOpen + " F: " + currParty.isFull;
                if(currParty.currentQuest != null) {
                    text += "\n" + Utilities.NormalizeString(currParty.currentQuest.questType.ToString());
                    if (currParty.currentQuest.isDone) {
                        text += "(Done)";
                    } else {
                        if (currParty.isOpen || currParty.currentQuest.isWaiting) {
                            text += "(Forming Party)";
                        } else {
                            text += "(In Progress)";
                        }
                    }
                }
                text += "\n      [url=" + currParty.partyLeader.id + "_character]" + currParty.partyLeader.name + "[/url]";
                for (int j = 0; j < currParty.partyMembers.Count; j++) {
                    ECS.Character currMember = currParty.partyMembers[j];
                    if(!currParty.IsCharacterLeaderOfParty(currMember)) {
                        text += "\n         [url=" + currMember.id + "_character]" + currMember.name + "[/url]";
                    }
                }
            }
        } else {
            text += "NONE";
        }
        settlementInfoLbl.text = text;
    }
}
