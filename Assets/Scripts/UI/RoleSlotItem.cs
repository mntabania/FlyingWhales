using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleSlotItem : MonoBehaviour, IDragParentItem {

    public Minion minion { get; private set; }

    public CharacterPortrait portrait;

    //public JOB slotJob { get; private set; }
    [SerializeField] private Image jobIcon;
    [SerializeField] private TextMeshProUGUI jobNameLbl;
    [SerializeField] private Image cooldownProgress;
    [SerializeField] private HoverHandler portraitHover;


    [SerializeField] private GameObject validPortraitGO;
    [SerializeField] private GameObject invalidPortraitGO;
    [SerializeField] private GameObject tooltipGO;
    [SerializeField] private TextMeshProUGUI tooltipLbl;

    public object associatedObj {
        get { return minion.character; }
    }

    //public void SetSlotJob(JOB job) { //This should only be called once!
    //    slotJob = job;
    //    UpdateVisuals();
    //    AddListeners();
    //}
    public void Initialize() {
        AddListeners();
    }

    private void AddListeners() {
        //Messenger.AddListener<JOB, Minion>(Signals.MINION_ASSIGNED_TO_JOB, OnMinionAssignedToJob);
        //Messenger.AddListener<JOB, Minion>(Signals.MINION_UNASSIGNED_FROM_JOB, OnMinionUnassignedFromJob);
        //Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobCooldownActivated);
    }

    public void SetMinion(Minion minion) {
        this.minion = minion;
        //if (minion == null) {
        //    Debug.Log("Setting character in role slot " + slotJob.ToString() + " to null");
        //} else {
        //    Debug.Log("Setting character in role slot " + slotJob.ToString() + " to " + minion.name);
        //}
        
        UpdateVisuals();
        //UpdateActionButtons();
    }
    private void UpdateVisuals() {
        //jobIcon.sprite = CharacterManager.Instance.GetJobSprite(slotJob);
        //jobNameLbl.text = Utilities.NormalizeString(slotJob.ToString());
        if (minion == null) {
            portrait.gameObject.SetActive(false);
        } else {
            portrait.GeneratePortrait(minion.character);
            portrait.SetBaseBGState(false);
            portrait.gameObject.SetActive(true);
        }
    }    
    //private void OnMinionAssignedToJob(JOB job, Minion minion) {
    //    if (slotJob == job) {
    //        SetMinion(minion);
    //    }
    //}
    //private void OnMinionUnassignedFromJob(JOB job, Minion minion) {
    //    if (slotJob == job) {
    //        SetMinion(null);
    //    }
    //}

    //#region Cooldown
    //private void OnJobCooldownActivated(PlayerJobAction action) {
    //    if (PlayerManager.Instance.player.roleSlots[slotJob].activeAction == action) {
    //        //the job that was activated is associated with this slot
    //        if (action.isInCooldown) {
    //            cooldownProgress.fillAmount = 1f;
    //            Messenger.AddListener(Signals.TICK_ENDED, UpdateCooldownProgress);
    //            Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobCooldownDone);
    //        }
    //    }
    //}
    //private void UpdateCooldownProgress() {
    //    PlayerJobAction activeAction = PlayerManager.Instance.player.roleSlots[slotJob].activeAction;
    //    float destinationValue = 1f - ((float)activeAction.ticksInCooldown / (float)activeAction.cooldown);
    //    //float value = Mathf.Lerp(cooldownProgress.fillAmount, destinationValue, Time.deltaTime * 10f);
    //    //cooldownProgress.fillAmount = value;
    //    StartCoroutine(SmoothProgress(cooldownProgress.fillAmount, destinationValue));
    //}
    //IEnumerator SmoothProgress(float start, float end) {
    //    float t = 0f;
    //    while (t < 1) {
    //        if (!GameManager.Instance.isPaused) {
    //            t += Time.deltaTime / GameManager.Instance.progressionSpeed;
    //            cooldownProgress.fillAmount = Mathf.Lerp(start, end, t);
    //        }
    //        yield return null;
    //    }
    //}
    //private void OnJobCooldownDone(PlayerJobAction action) {
    //    if (PlayerManager.Instance.player.roleSlots[slotJob].activeAction == action && !action.isInCooldown) {
    //        Messenger.RemoveListener(Signals.TICK_ENDED, UpdateCooldownProgress);
    //        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobCooldownDone);
    //        cooldownProgress.fillAmount = 0f;
    //    }
    //}
    //#endregion

    #region Hover
    public void ShowHoverTooltip() {
        //string header = Utilities.NormalizeStringUpperCaseFirstLetters(slotJob.ToString()) + ": " + minion.name;
        //string message = string.Empty;
        //switch (slotJob) {
        //    case JOB.SPY:
        //        header += " (1)";
        //        message = "An agent that gathers information about places and characters.";
        //        break;
        //    case JOB.SEDUCER:
        //        header += " (2)";
        //        message = "An agent that corrupts heroes and recruits new minions.";
        //        break;
        //    case JOB.DIPLOMAT:
        //        header += " (3)";
        //        message = "An agent that builds relationships with other characters.";
        //        break;
        //    case JOB.INSTIGATOR:
        //        header += " (4)";
        //        message = "An agent that sows discord and chaos.";
        //        break;
        //    case JOB.DEBILITATOR:
        //        header += " (5)";
        //        message = "An agent that halts unwanted actions and activities.";
        //        break;
        //}
        //UIManager.Instance.ShowSmallInfo(message, PlayerUI.Instance.roleSlotTooltipPos, header);
        //UIManager.Instance.ShowSmallInfo(message, header);
    }
    public void HideTooltip() {
        if (UIManager.Instance != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    private void ShowActionBtnTooltip(string message, string header) {
        string m = string.Empty;   
        if (!string.IsNullOrEmpty(header)) {
            m = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        m += $"<line-height=70%><size=16>{message}";

        m = m.Replace("\\n", "\n");

        //actionBtnTooltipLbl.text = m;
        //actionBtnTooltipGO.gameObject.SetActive(true);
    }
    public void HideActionBtnTooltip() {
        //actionBtnTooltipGO.gameObject.SetActive(false);
    }
    #endregion
}