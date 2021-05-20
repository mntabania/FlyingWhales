using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Talent Data", menuName = "Scriptable Objects/Character Talent Data")]
public class CharacterTalentData : ScriptableObject {
    [SerializeField] private CHARACTER_TALENT _type;

    public void OnLevelUp(Character p_character, int level) { }
}