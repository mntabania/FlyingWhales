using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact Data", menuName = "Scriptable Objects/Artifact Data")]
public class ArtifactData : ScriptableObject {
    [SerializeField] private ARTIFACT_TYPE _type;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private ArtifactUnlockable[] _unlocks;
    
    #region getters
    public ARTIFACT_TYPE type => _type;
    public Sprite sprite => _sprite;
    public ArtifactUnlockable[] unlocks => _unlocks;
    public Sprite portrait => _portrait;
    #endregion
}

[Serializable]
public class ArtifactUnlockable {
    public ARTIFACT_UNLOCKABLE_TYPE unlockableType;
    public string identifier;
}