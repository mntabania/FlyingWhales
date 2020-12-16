﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationMenu : PopupMenuBase {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;
    [SerializeField] private TextMeshProUGUI endOfConversationLbl;

    private bool wasPausedOnOpen;
    public void Open(List<ConversationData> conversationList, string titleText) {
        base.Open();

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(UISignals.ON_OPEN_CONVERSATION_MENU);

        instructionLbl.text = titleText; // $"Share Intel with {targetCharacter.name}";
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        UtilityScripts.Utilities.DestroyChildren(dialogScrollView.content);

        if(conversationList != null) {
            for (int i = 0; i < conversationList.Count; i++) {
                ConversationData data = conversationList[i];
                CreateDialogItem(data.character, data.text, data.position);
            }
        }

        endOfConversationLbl.transform.SetParent(dialogScrollView.content);
        endOfConversationLbl.gameObject.SetActive(true);
        closeBtn.interactable = true;
        dialogScrollView.verticalNormalizedPosition = 1f;
        //GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        //DialogItem item = targetDialog.GetComponent<DialogItem>();
        //item.SetData(targetCharacter, "What do you want from me?");

        //GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        //DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        //actorItem.SetData(actor, intelToShare.log.logText, DialogItem.Position.Right);

        //DirectlyShowIntelReaction(intelToShare);
    }
    //private void DirectlyShowIntelReaction(IIntel intel) {
    //    ReactToIntel(intel);
    //}
    public override void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        base.Close();
        UIManager.Instance.SetSpeedTogglesState(true);
        GameManager.Instance.SetPausedState(wasPausedOnOpen);
        Messenger.Broadcast(UISignals.ON_CLOSE_CONVERSATION_MENU);
    }

    //private void ReactToIntel(IIntel intel) {
    //    closeBtn.interactable = false;
    //    string response = targetCharacter.ShareIntel(intel);
    //    if ((string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response)) && intel.actor != targetCharacter) {
    //        ActualGoapNode action = null;
    //        if(intel is ActionIntel actionIntel) {
    //            action = actionIntel.node;
    //        }
    //        response = CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, targetCharacter, intel.actor, REACTION_STATUS.INFORMED, action);
    //    }
    //    StartCoroutine(ShowReaction(response, intel, targetCharacter));
    //}
    //private IEnumerator ShowReaction(string reaction, IIntel intel, Character reactor) {
    //    if (reaction == string.Empty) {
    //        //character had no reaction
    //        CreateDialogItem(reactor, intel.actor == targetCharacter ? "I know what I did." : "A proper response to this information has not been implemented yet.");
    //    } else {
    //        if (reaction == "aware") {
    //            CreateDialogItem(reactor, $"{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} already knows this.");
    //        } else {
    //            string[] emotionsToActorAndTarget = reaction.Split('/');

    //            string emotionsTowardsActor = emotionsToActorAndTarget.ElementAtOrDefault(0);
    //            string emotionsTowardsTarget = emotionsToActorAndTarget.ElementAtOrDefault(1);

    //            bool hasReactionToActor = string.IsNullOrEmpty(emotionsTowardsActor) == false;
    //            bool hasReactionToTarget = string.IsNullOrEmpty(emotionsTowardsTarget) == false;
                
    //            if (hasReactionToActor == false && hasReactionToTarget == false) {
    //                //has no reactions to actor and target
    //                CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed Disinterested about this.");
    //            } else {
    //                if (hasReactionToActor) {
    //                    CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsActor, 2)} at {intel.actor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(intel.actor.name)} after receiving the new information.");    
    //                }
    //                if (hasReactionToTarget) {
    //                    if (intel.target is Character intelTarget) {
    //                        CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsTarget, 2)} at {intelTarget.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(intel.target.name)} after receiving the new information.");  
    //                    } else {
    //                        CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsTarget, 2)} at {UtilityScripts.Utilities.ColorizeAndBoldName(intel.target.name)} after receiving the new information.");    
    //                    }
                        
    //                }
    //            }
    //        }
    //    }
    //    // GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
    //    // DialogItem item = targetDialog.GetComponent<DialogItem>();
    //    // item.SetData(targetCharacter, reaction);
    //    endOfConversationLbl.transform.SetParent(dialogScrollView.content);
    //    endOfConversationLbl.gameObject.SetActive(true);
    //    closeBtn.interactable = true;
    //    dialogScrollView.verticalNormalizedPosition = 1f;
    //    yield return null;
    //    //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
    //    //share.DeactivateAction();
    //}

    private void CreateDialogItem(Character character, string reaction, DialogItem.Position position) {
        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(character, reaction, position);
    }
}

public class ConversationData {
    public string text;
    public DialogItem.Position position;
    public Character character;

    #region Object Pool
    public void Reset() {
        text = null;
        position = DialogItem.Position.Left;
        character = null;
    }
    #endregion
}