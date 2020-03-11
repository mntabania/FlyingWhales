using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareIntelMenu : PopupMenuBase {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;
    [SerializeField] private TextMeshProUGUI endOfConversationLbl;

    [Header("Intel")]
    [SerializeField] private GameObject intelGO;
    [SerializeField] private IntelItem[] intelItems;

    private Character targetCharacter;
    private Character actor;

    private bool wasPausedOnOpen;
    public void Open(Character targetCharacter) {
        //UIManager.Instance.SetCoverState(true);
        //UIManager.Instance.Pause();
        //UIManager.Instance.SetSpeedTogglesState(false);
        base.Open();

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(Signals.ON_OPEN_SHARE_INTEL);

        this.targetCharacter = targetCharacter;
        this.actor = PlayerManager.Instance.player.minions.FirstOrDefault()?.character ?? null;
        instructionLbl.text = $"Share Intel with {targetCharacter.name}";
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        UtilityScripts.Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, "I am here to share information with you.", DialogItem.Position.Right);

        UpdateIntel(PlayerManager.Instance.player.allIntel);
    }
    public void Open(Character targetCharacter, Character actor, Intel intelToShare) {
        //UIManager.Instance.SetCoverState(true);
        //UIManager.Instance.Pause();
        //UIManager.Instance.SetSpeedTogglesState(false);
        base.Open();

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(Signals.ON_OPEN_SHARE_INTEL);

        this.targetCharacter = targetCharacter;
        this.actor = actor;
        instructionLbl.text = $"Share Intel with {targetCharacter.name}";
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        UtilityScripts.Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, UtilityScripts.Utilities.LogReplacer(intelToShare.node.descriptionLog), DialogItem.Position.Right);

        DirectlyShowIntelReaction(intelToShare);
    }
    private void DirectlyShowIntelReaction(Intel intel) {
        HideIntel();
        ReactToIntel(intel);
    }

    private void UpdateIntel(List<Intel> intelToShow) {
        intelGO.SetActive(true);
        SetIntelButtonsInteractable(true);
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            Intel intel = intelToShow.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
            if (intel != null) {
                currItem.SetClickAction(ReactToIntel);
            }
        }
    }
    private void HideIntel() {
        intelGO.SetActive(false);
    }
    private void SetIntelButtonsInteractable(bool state) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.GetComponent<Button>().interactable = state;
        }
    }
    public override void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        base.Close();
        UIManager.Instance.SetSpeedTogglesState(true);
        GameManager.Instance.SetPausedState(wasPausedOnOpen);
        Messenger.Broadcast(Signals.ON_CLOSE_SHARE_INTEL);
    }

    private void ReactToIntel(Intel intel) {
        closeBtn.interactable = false;
        //HideIntel();
        //UpdateIntel(new List<Intel>() { intel });
        //intelItems[0].SetClickedState(true);
        //SetIntelButtonsInteractable(false);

        //GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        //DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        //actorItem.SetData(actor, Utilities.LogReplacer(intel.intelLog), DialogItem.Position.Right);

        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.BaseActivate(targetCharacter);
        //List<string> reactions = targetCharacter.ShareIntel(intel);
        //StartCoroutine(ShowReactions(reactions));
        string response = targetCharacter.ShareIntel(intel);
        if (string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response)) {
            response = CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, targetCharacter, intel.node.actor, REACTION_STATUS.INFORMED);
        }
        StartCoroutine(ShowReaction(response, intel));
    }
    string[] randomNothings = new string[] {
        "I really don't care",
        "And I should care, why?",
        "Should this mean something to me?",
        "Stop wasting my time!",
        "And the point of sharing this with me is...?"
    };
    private IEnumerator ShowReactions(List<string> reactions) {
        Debug.Log($"Showing Reactions of {targetCharacter.name}: {reactions.Count}");
        if (reactions.Count == 0) {
            //character had no reaction
            reactions.Add(randomNothings[Random.Range(0, randomNothings.Length)]);
        }
        for (int i = 0; i < reactions.Count; i++) {
            GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
            DialogItem item = targetDialog.GetComponent<DialogItem>();
            item.SetData(targetCharacter, reactions[i]);
            if (i + 1 == reactions.Count) {
                endOfConversationLbl.transform.SetParent(dialogScrollView.content);
                endOfConversationLbl.gameObject.SetActive(true);
                //UIManager.Instance.ScrollRectSnapTo(dialogScrollView, endOfConversationLbl.transform as RectTransform);
            } else {
                //UIManager.Instance.ScrollRectSnapTo(dialogScrollView, item.characterDialogParent);
            }
            
            //yield return new WaitForSeconds(0.5f);
            //dialogScrollView.verticalNormalizedPosition = 0f;

        }
        closeBtn.interactable = true;
        dialogScrollView.verticalNormalizedPosition = 0f;
        yield return null;
        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.DeactivateAction();
    }
    private IEnumerator ShowReaction(string reaction, Intel intel) {
        if (reaction == string.Empty) {
            //character had no reaction
            if (intel.node.actor == targetCharacter) {
                reaction = "I know what I did.";
            } else {
                reaction = "A proper response to this information has not been implemented yet.";
            }
        } else {
            if (reaction == "aware") {
                reaction = "I already know this.";
            } else {
                string[] emotionsToActorAndTarget = reaction.Split('/');
                string finalReaction = string.Empty;
                for (int i = 0; i < emotionsToActorAndTarget.Length; i++) {
                    string[] words = emotionsToActorAndTarget[i].Split(' ');
                    if(words != null) {
                        string responses = string.Empty;
                        for (int j = 0; j < words.Length; j++) {
                            string currWord = words[j];
                            if(string.IsNullOrEmpty(currWord) || string.IsNullOrWhiteSpace(currWord)){ continue; }
                            if(responses != string.Empty) {
                                responses += ", ";
                            }
                            responses += currWord;
                        }
                        if (responses != string.Empty) {
                            if (finalReaction != string.Empty && i > 0) {
                                finalReaction += "\n";
                            }
                            finalReaction +=
                                $"I feel {responses} towards {(i == 0 ? intel.node.actor.name : intel.node.poiTarget.name)}.";
                        }
                    }
                }
                if (finalReaction != string.Empty) {
                    reaction = finalReaction;
                } else {
                    reaction = "I processed no emotions. I am a rock, I am an i-i-i-island.";
                }
            }
        }
        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, reaction);
        endOfConversationLbl.transform.SetParent(dialogScrollView.content);
        endOfConversationLbl.gameObject.SetActive(true);
        closeBtn.interactable = true;
        dialogScrollView.verticalNormalizedPosition = 0f;
        yield return null;
        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.DeactivateAction();
    }
}
