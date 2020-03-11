using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {
    protected string _name;
    protected JOB _jobType;
    protected Character _character;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public JOB jobType {
        get { return _jobType; }
    }
    public Character character {
        get { return _character; }
    }
    #endregion

    public Job (Character character, JOB jobType) {
        _jobType = jobType;
        _name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(_jobType.ToString());
        _character = character;
    }

    #region Virtuals
    public virtual void OnAssignJob() {}
    #endregion
}