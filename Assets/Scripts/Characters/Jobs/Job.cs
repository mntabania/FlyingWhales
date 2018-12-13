﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {
    INTERACTION_TYPE[] explorerEvents = new INTERACTION_TYPE[] { //TODO: Put this somwhere else
        INTERACTION_TYPE.INDUCE_WAR,
        INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS,
    };

    protected string _name;
    protected JOB _jobType;
    protected int _actionDuration; //-1 means no limits and no progress
    protected bool _hasCaptureEvent;
    protected Character _character;
    protected Interaction _createdInteraction;
    protected bool _useInteractionTimer;
    //protected INTERACTION_TYPE[] _characterInteractions; //For non-minion characters only
    private WeightedDictionary<RESULT> rateWeights;

    private int _currentTick;
    private bool _isJobActionPaused;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public int actionDuration {
        get { return _actionDuration; }
    }
    public JOB jobType {
        get { return _jobType; }
    }
    public Character character {
        get { return _character; }
    }
    public Interaction createdInteraction {
        get { return _createdInteraction; }
    }
    #endregion

    public Job (Character character, JOB jobType) {
        _jobType = jobType;
        _name = Utilities.NormalizeString(_jobType.ToString());
        _character = character;
        //_characterInteractions = new INTERACTION_TYPE[] { INTERACTION_TYPE.RETURN_HOME };
        rateWeights = new WeightedDictionary<RESULT>();
        _useInteractionTimer = true;
    }

    #region Virtuals
    public virtual void OnAssignJob() {}
    public virtual void CaptureRandomLandmarkEvent() {}
    public virtual void ApplyActionDuration() {}
    public virtual void DoJobAction() {
        Debug.Log(GameManager.Instance.TodayLogString() + " Doing job action: " + character.name + "(" + jobType.ToString() + ")");
    }
    public virtual int GetSuccessRate() { return 0; }
    public virtual int GetFailRate() { return 40; }
    public virtual int GetCritFailRate() { return 12; }
    public virtual WeightedDictionary<RESULT> GetJobRateWeights() {
        rateWeights.Clear();
        rateWeights.AddElement(RESULT.SUCCESS, GetSuccessRate());
        rateWeights.AddElement(RESULT.FAIL, GetFailRate());
        return rateWeights;
    }
    #endregion

    #region Utilities
    public void StartJobAction() {
        ApplyActionDuration();
        _currentTick = 0;
        SetJobActionPauseState(false);
        if(_actionDuration != -1) {
            Messenger.AddListener(Signals.DAY_STARTED, CheckJobAction);
        }
        if (_hasCaptureEvent) {
            Messenger.AddListener(Signals.DAY_ENDED, CatchRandomEvent);
        }
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimerJob(_actionDuration);
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimerJob();
        }
    }

    //Stops Job Action entirely
    //Uses - when a minion is recalled, when job action duration ends
    public void StopJobAction() {
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimerJob();
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimerJob();
        }
        if (_actionDuration != -1) {
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckJobAction);
        }
        if (_hasCaptureEvent) {
            Messenger.RemoveListener(Signals.DAY_ENDED, CatchRandomEvent);
        }
    }
    public void StopCreatedInteraction() {
        if(_createdInteraction != null) {
            _createdInteraction.interactable.landmarkVisual.StopInteractionTimer();
            _createdInteraction.interactable.landmarkVisual.HideInteractionTimer();
            _createdInteraction.TimedOutRunDefault();
        }
    }
    private void CheckJobAction() {
        if (_isJobActionPaused) { return; }
        if (_currentTick >= _actionDuration) {
            StopJobAction();
            DoJobAction();
            return;
        }
        _currentTick++;

    }

    protected void SetJobActionPauseState(bool state) {
        _isJobActionPaused = state;
        if (_useInteractionTimer) {
            _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetTimerPauseStateJob(_isJobActionPaused);
        }
    }
    public void SetCreatedInteraction(Interaction interaction) {
        _createdInteraction = interaction;
        if(_createdInteraction != null) {
            if (!_createdInteraction.hasInitialized) {
                _createdInteraction.Initialize();
            }
            _createdInteraction.SetJobAssociated(this);
        }
    }
    private void CatchRandomEvent() {
        if (_isJobActionPaused) { return; }
        CaptureRandomLandmarkEvent();
    }
    //public void CreateRandomInteractionForNonMinionCharacters() {
        //if(_characterInteractions != null) {
        //    INTERACTION_TYPE type = _characterInteractions[UnityEngine.Random.Range(0, _characterInteractions.Length)];
        //    if (InteractionManager.Instance.CanCreateInteraction(type, character)) {
        //        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(type, character.specificLocation as BaseLandmark);
        //        character.AddInteraction(interaction);
        //    }
        //}
    //}
    public void ForceDefaultAllExistingInteractions() {
        _character.specificLocation.tileLocation.areaOfTile.SetStopDefaultInteractionsState(false);
        _character.specificLocation.tileLocation.areaOfTile.DefaultAllExistingInteractions();
    }
    public int GetSupplyObtained(Area targetArea) {
        //When a raid succeeds, the amount of Supply obtained is based on character level.
        //5% to 15% of location's supply 
        //+1% every other level starting at level 6
        Area characterHomeArea = character.homeLandmark.tileLocation.areaOfTile;
        //Area targetArea = character.specificLocation.tileLocation.areaOfTile;
        int supplyObtainedPercent = Random.Range(5, 16);
        supplyObtainedPercent += (character.level - 5);

        return Mathf.FloorToInt(targetArea.suppliesInBank * (supplyObtainedPercent / 100f));
        //characterHomeArea.AdjustSuppliesInBank(obtainedSupply);
    }
    public Interaction CreateExplorerEvent() {
        List<INTERACTION_TYPE> choices = GetValidExplorerEvents();
        if (choices.Count > 0) {
            Area area = _character.specificLocation.tileLocation.areaOfTile;
            INTERACTION_TYPE chosenType = choices[Random.Range(0, choices.Count)];
            //Get Random Explorer Event
            return InteractionManager.Instance.CreateNewInteraction(chosenType, area.coreTile.landmarkOnTile);
        }
        return null;
    }
    private List<INTERACTION_TYPE> GetValidExplorerEvents() {
        List<INTERACTION_TYPE> validTypes = new List<INTERACTION_TYPE>();
        for (int i = 0; i < explorerEvents.Length; i++) {
            INTERACTION_TYPE type = explorerEvents[i];
            if (InteractionManager.Instance.CanCreateInteraction(type, _character)) {
                validTypes.Add(type);
            }
        }
        return validTypes;
    }
    #endregion
}