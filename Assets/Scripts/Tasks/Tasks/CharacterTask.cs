/*
 This is the base class for anything that a character does.
 Quests, Go Home, Rest, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

//[System.Serializable]
public class CharacterTask {

    public delegate void OnTaskInfoChanged();
    public OnTaskInfoChanged onTaskInfoChanged; //For UI, to update quest info if a specific quest changes info

    public delegate void OnTaskLogsChange();
    public OnTaskLogsChange onTaskLogsChange; //For UI, to update when a specific quest adds a new log

    protected TaskCreator _createdBy;
    protected TASK_TYPE _taskType;
	protected string _taskName;
    protected ECS.Character _assignedCharacter;
    protected bool _isDone;
	protected bool _isHalted;
    protected TASK_STATUS _taskStatus;
    protected List<string> _taskLogs; //TODO: Change this to Logs when convenient

    protected STANCE _stance;

	protected bool _forPlayerOnly;
	protected bool _forGameOnly;
	protected ILocation _targetLocation;
	protected int _daysLeft;
	protected int _defaultDaysLeft;
	protected object _specificTarget;
	protected bool _needsSpecificTarget;
	protected string _specificTargetClassification;
	protected TaskFilter[] _filters;

    protected Quest _parentQuest;
    protected List<ACTION_ALIGNMENT> _alignments;

	protected WeightedDictionary<BaseLandmark> _landmarkWeights;
	protected WeightedDictionary<ECS.Character> _characterWeights;

	protected State _currentState;
	protected Dictionary<STATE, State> _states;

	protected bool _canTargetSelf;

	protected int _combatPriority; //combat initiations will be sorted based on this priority

    #region getters/setters
    public TASK_TYPE taskType {
        get { return _taskType; }
    }
    public bool isDone {
        get { return _isDone; }
    }
	public bool isHalted {
		get { return _isHalted; }
	}
    public TASK_STATUS taskStatus {
        get { return _taskStatus; }
    }
    public List<string> taskLogs {
        get { return _taskLogs; }
    }
    public STANCE stance {
        get { return _stance; }
    }
	public bool forPlayerOnly{
		get { return _forPlayerOnly; }
	}
	public bool forGameOnly{
		get { return _forGameOnly; }
	}
	public ILocation targetLocation{
		get { return _targetLocation; }
	}
	public object specificTarget{
		get { return _specificTarget; }
	}
	public bool needsSpecificTarget{
		get { return _needsSpecificTarget; }
	}
	public string specificTargetClassification{
		get { return _specificTargetClassification; }
	}
	public TaskFilter[] filters{
		get { return _filters; }
	}
    public List<ACTION_ALIGNMENT> alignments {
        get { return _alignments; }
    }
    public Quest parentQuest {
        get { return _parentQuest; }
    }
	public State currentState {
		get { return _currentState; }
	}
	public bool canTargetSelf {
		get { return _canTargetSelf; }
	}
    public ECS.Character assignedCharacter {
        get { return _assignedCharacter; }
    }
	public int combatPriority{
		get { return _combatPriority; }
	}
	//public WeightedDictionary<BaseLandmark> landmarkWeights{
	//	get { return _landmarkWeights; }
	//}
	//public WeightedDictionary<ECS.Character> characterWeights{
	//	get { return _characterWeights; }
	//}
    //public string actionString {
    //    get { return _actionString; }
    //}
    #endregion

	public CharacterTask(TaskCreator createdBy, TASK_TYPE taskType, STANCE stance, int defaultDaysLeft = -1, Quest parentQuest = null) {
        _createdBy = createdBy;
        _taskType = taskType;
		_taskName = Utilities.NormalizeStringUpperCaseFirstLetters (_taskType.ToString ());
        _taskLogs = new List<string>();
		_landmarkWeights = new WeightedDictionary<BaseLandmark> ();
		_characterWeights = new WeightedDictionary<ECS.Character> ();
		_states = new Dictionary<STATE, State> ();
		_currentState = null;
		_forPlayerOnly = false;
		_forGameOnly = false;
		SetIsHalted (false);
		_isDone = false;
		_canTargetSelf = false;
        _parentQuest = parentQuest;
        _alignments = new List<ACTION_ALIGNMENT>();
		SetDefaultDaysLeft (defaultDaysLeft);
		SetDaysLeft (defaultDaysLeft);
		SetStance(stance);
		SetCombatPriority (-1);
    }

    #region virtual
    public virtual void OnChooseTask(ECS.Character character) {
//		if(character.isInCombat){
//			character.SetCurrentFunction (() => OnChooseTask (character));
//			return;
//		}
        //TODO: Remove This after testing
        if (!character.previousActions.ContainsKey(this)) {
            character.previousActions.Add(this, StackTraceUtility.ExtractStackTrace());
        }

        _taskStatus = TASK_STATUS.IN_PROGRESS;
        _assignedCharacter = character;
        //character.SetCurrentTask(this);

		//if (character.party != null) {
		//	character.party.SetCurrentTask(this);
		//}
    }
    /*
     Override this to make the character do something when
     he/she chooses to perform this task.
         */
    public virtual void PerformTask() { //Everyday action of the task
		if(!CanPerformTask()){
			return;
		}
		if(_currentState != null){
			_currentState.PerformStateAction ();
		}
		if(_daysLeft == 0){
	        EndTaskSuccess ();
			return;
		}
		ReduceDaysLeft(1);
    }
	public virtual bool CanPerformTask(){
		if(_isHalted || _isDone){
			return false;
		}
		return true;
	}
	public virtual void EndTaskSuccess(){
		EndTask (TASK_STATUS.SUCCESS);
	}
	public virtual void EndTaskFail(){
		EndTask (TASK_STATUS.FAIL);
	}
    public virtual void EndTask(TASK_STATUS taskResult) {
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => EndTask (taskResult));
//			return;
//		}
		if(_isHalted || _isDone){
			return;
		}
        _taskStatus = taskResult;
        switch (taskResult) {
            case TASK_STATUS.SUCCESS:
                TaskSuccess();
                break;
            case TASK_STATUS.FAIL:
                TaskFail();
                break;
            case TASK_STATUS.CANCEL:
                TaskCancel();
                break;
            default:
                break;
        }
    }
    public virtual void TaskSuccess() {
        _isDone = true;
        Messenger.Broadcast(Signals.TASK_SUCCESS, _assignedCharacter, this);
		_assignedCharacter.DetermineAction();
	}
    public virtual void TaskCancel() {
		_assignedCharacter.DetermineAction();
	}
    public virtual void TaskFail() {
		_assignedCharacter.DetermineAction();
	}
	public virtual void ResetTask(){
		_assignedCharacter = null;
		_targetLocation = null;
		_isDone = false;
		SetIsHalted (false);
		_taskStatus = TASK_STATUS.IN_PROGRESS;
		_taskLogs.Clear ();
		_daysLeft = _defaultDaysLeft;
	}
    /*
     This will return the weight (how likely) that this task will be performed
         */
	public virtual int GetSelectionWeight(ECS.Character character){ return 0; }
	public virtual bool CanBeDone(ECS.Character character, ILocation location) { return false; }
    /*
     Can the character currently do this action?/Should it be included in the characters selection weights?
         */
	public virtual bool AreConditionsMet(ECS.Character character) { return false; }
    public virtual CharacterTask CloneTask() {
		CharacterTask clonedTask = new CharacterTask(_createdBy, _taskType, _stance, _defaultDaysLeft, _parentQuest);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
        return clonedTask;
    }
    protected virtual BaseLandmark GetLandmarkTarget(ECS.Character character) {
		_landmarkWeights.Clear ();
		return null; 
	}
    protected virtual ECS.Character GetCharacterTarget(ECS.Character character) {
        _characterWeights.Clear();
        return null;
    }
    protected virtual WeightedDictionary<ECS.Character> GetCharacterTargetWeights(ECS.Character character) { return new WeightedDictionary<ECS.Character>(); }
    public virtual bool HasHostilitiesBecauseOfTask(Character other) { return false; }
    #endregion

    protected void ScheduleTaskEnd(int days, TASK_STATUS result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndTask(result));
    }

    #region Logs
    protected void LogTargetWeights<T>(WeightedDictionary<T> weights) {
        if (UIManager.Instance.characterInfoUI.activeCharacter != null && _assignedCharacter.id == UIManager.Instance.characterInfoUI.activeCharacter.id) {
            weights.LogDictionaryValues(_assignedCharacter.name + "'s " + this.taskType.ToString() + " weights ");
        }
    }
    internal void AddNewLog(string log) {
        _taskLogs.Add(log);
        if (onTaskLogsChange != null) {
            onTaskLogsChange();
        }
    }
    internal void AddNewLogs(List<string> logs) {
        for (int i = 0; i < logs.Count; i++) {
            _taskLogs.Add(logs[i]);
        }
        if (onTaskLogsChange != null) {
            onTaskLogsChange();
        }
    }
    public virtual string GetArriveActionString() {
        return LocalizationManager.Instance.GetLocalizedValue("CharacterTasks", this.GetType().ToString(), "arrive_action");
    }
    public virtual string GetLeaveActionString() {
        return LocalizationManager.Instance.GetLocalizedValue("CharacterTasks", this.GetType().ToString(), "leave_action");
    }
    #endregion

    #region Utilities
    protected void SetStance(STANCE stance) {
        _stance = stance;
    }
	public void SetLocation(ILocation targetLocation){
		_targetLocation = targetLocation;
	}
	public void SetSpecificTarget(object specificTarget){
		_specificTarget = specificTarget;
	}
	public void SetIsHalted (bool state){
		_isHalted = state;
	}
	public void SetDefaultDaysLeft(int days){
		_defaultDaysLeft = days;
	}
	public void SetDaysLeft (int days){
		_daysLeft = days;
	}
	public void ReduceDaysLeft(int days){
		if (!_isHalted && !_isDone) {
			if (_daysLeft > 0) {
				_daysLeft -= days;
			}
		}
	}
	public bool CanMeetRequirements(ECS.Character targetCharacter, ECS.Character assignedCharacter = null){
		for (int i = 0; i < _filters.Length; i++) {
			_filters [i].SetSecondCharacter (assignedCharacter);
			if(!_filters[i].MeetsRequirements(targetCharacter)){
				return false;
			}
		}
		return true;
	}
	public void SetForGameOnly(bool state){
		_forGameOnly = state;
	}
	public void SetForPlayerOnly(bool state){
		_forPlayerOnly = state;
	}
	public void SetCombatPriority(int amount){
		_combatPriority = amount;
	}
    #endregion

	#region States
	public void ChangeStateTo(STATE state, bool isHalted = false){
		if(_states.ContainsKey(state)){
			State newCurrentState = _states [state];
			newCurrentState.OnChooseState (_assignedCharacter);
			_currentState = newCurrentState;
			newCurrentState.SetIsHalted (isHalted);
		}
	}
    public State GetState(STATE state) {
        if (_states.ContainsKey(state)) {
            return _states[state];
        }
        return null;
    }
	#endregion
}
