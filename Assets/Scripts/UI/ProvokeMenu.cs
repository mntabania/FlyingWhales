using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class ProvokeMenu : PopupMenuBase {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;

    private Character targetCharacter;
    private Character actor;

    private bool wasPausedOnOpen;

    public void Open(Character actor, Character targetCharacter) {
        base.Open();

        this.targetCharacter = targetCharacter;
        this.actor = actor;
        instructionLbl.text = $"Provoke {targetCharacter.name}";

        UtilityScripts.Utilities.DestroyChildren(dialogScrollView.content);

        string targetDialogText = string.Empty;
        string actorDialogText = string.Empty;

        ProvokeAction(ref targetDialogText, ref actorDialogText);

        if(this.actor != null) {
            GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
            DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
            actorItem.SetData(this.actor, actorDialogText, DialogItem.Position.Right);
        }

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, targetDialogText);

        wasPausedOnOpen = GameManager.Instance.isPaused;
        GameManager.Instance.SetPausedState(true);
        UIManager.Instance.SetSpeedTogglesState(false);
    }

    private void ProvokeAction(ref string targetText, ref string actorText) {
        bool succeedProvoke = false;
        if (targetCharacter.relationshipContainer.GetEnemyCharacters().Count > 0) {
            //succeedProvoke = true;
            // CHARACTER_MOOD currentMood = targetCharacter.currentMoodType;
            // if (currentMood == CHARACTER_MOOD.GREAT) {
            //     int chance = UnityEngine.Random.Range(0, 100);
            //     if (chance < 70) {
            //         actorText = "You should take revenge on your enemies.";
            //         targetText = "I am too happy right now to even care about my enemies.";
            //     } else {
            //         succeedProvoke = true;
            //     }
            // } else if (currentMood == CHARACTER_MOOD.GOOD) {
            //     int chance = UnityEngine.Random.Range(0, 2);
            //     if (chance == 0) {
            //         actorText = "You should take revenge on your enemies.";
            //         targetText = "I am too happy right now to even care about my enemies.";
            //     } else {
            //         succeedProvoke = true;
            //     }
            // } else {
            //     succeedProvoke = true;
            // }
        } else {
            actorText = "You should take revenge on your enemies.";
            if(targetCharacter.traitContainer.HasTrait("Diplomatic")) {
                targetText = "Sorry, I don't do that. I am a very peaceful person.";
            } else {
                targetText = "Sorry, I don't have any.";
            }
        }

        bool succeedUndermine = false;
        Character chosenCharacter = null;
        if (succeedProvoke) {
            List<Character> enemyCharacters = targetCharacter.relationshipContainer.GetEnemyCharacters();
            while (chosenCharacter == null && enemyCharacters.Count > 0) {
                int index = UnityEngine.Random.Range(0, enemyCharacters.Count);
                Character character = enemyCharacters[index];
                if (character.HasJobTargetingThis(JOB_TYPE.UNDERMINE) || targetCharacter.jobQueue.HasJob(JOB_TYPE.UNDERMINE, character)) {
                    enemyCharacters.RemoveAt(index);
                } else {
                    chosenCharacter = character;
                }
            }
            if (chosenCharacter == null) {
                actorText = "You should take revenge on your enemies.";
                if (targetCharacter.jobQueue.HasJob(JOB_TYPE.UNDERMINE)) {
                    targetText = "That's exactly what I'm doing!"; //Don't tell me what to do!
                } else {
                    targetText = "I should, but I rather let them fight each other.";
                }
            } else {
                actorText =
                    $"{chosenCharacter.name} is living {UtilityScripts.Utilities.GetPronounString(chosenCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)} best life. Are you just gonna let your enemy be happy?";
                targetText =
                    $"I will not allow it! I'll take {UtilityScripts.Utilities.GetPronounString(chosenCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)} down with me!";

                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = chosenCharacter });
                //job.SetCannotOverrideJob(true);
                //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                //targetCharacter.jobQueue.AddJobInQueue(job, false);
                //targetCharacter.jobQueue.ProcessFirstJobInQueue(targetCharacter);
                succeedUndermine = true;
                targetCharacter.jobComponent.CreateUndermineJob(chosenCharacter, "provoke");

                //Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "provoke");
                //addLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //addLog.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //addLog.AddLogToInvolvedObjects();

                //PlayerManager.Instance.player.ShowNotification(addLog);
            }
        }

        // if (succeedUndermine) {
        //     Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_provoke");
        //     log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //     log.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     log.AddLogToInvolvedObjects();
        //     PlayerManager.Instance.player.ShowNotification(log);
        // } else {
        //     Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_provoke_fail");
        //     log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //     log.AddLogToInvolvedObjects();
        //     PlayerManager.Instance.player.ShowNotification(log);
        // }
    }

    public override void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        base.Close();
        UIManager.Instance.SetSpeedTogglesState(true);
        GameManager.Instance.SetPausedState(wasPausedOnOpen);
    }
}
