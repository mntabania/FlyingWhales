using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBehaviourComponent {

    private List<Character> _isDisabledFor;
    protected BEHAVIOUR_COMPONENT_ATTRIBUTE[] attributes;
    public int priority { get; protected set; }

    public abstract bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob);

    /// <summary>
    /// Contains stuff to do to character when they ADD this behaviour to their list of behaviours.
    /// </summary>
    /// <param name="character">The character concerned.</param>
    public virtual void OnAddBehaviourToCharacter(Character character) { }
    /// <summary>
    /// Contains stuff to do to character when they REMOVE this behaviour from their list of behaviours.
    /// </summary>
    /// <param name="character">The character concerned.</param>
    public virtual void OnRemoveBehaviourFromCharacter(Character character) { }
    
    #region Enabling/Disabling
    private void DisableFor(Character character) {
        if (_isDisabledFor == null) { _isDisabledFor = new List<Character>(); }
        _isDisabledFor.Add(character);
    }
    private void EnableFor(Character character) {
        _isDisabledFor.Remove(character);
    }
    public bool IsDisabledFor(Character character) {
        if (_isDisabledFor != null) {
            return _isDisabledFor.Contains(character);
        }
        return false;
    }
    public bool CanDoBehaviour(Character character) {
        if(HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY)) { //character.specificLocation.region.area.areaMap - will be changed after specificLocation rework
            //if character is not at a npcSettlement map, and the current behaviour requires the character to be at a npcSettlement map, then character cannot do this behaviour
            //EDIT: New mechanic for INSIDE_SETTLEMENT_ONLY - now this attribute simply means "whenever the character is inside his/her home"
            if (!character.IsInHomeSettlement()) {
                return false;
            }
        }
        //else if (character.isAtHomeRegion && HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY)) {
        //    //if character is at a npcSettlement map, and the current behaviour requires the character to NOT be at a npcSettlement map, then character cannot do this behaviour
        //    //EDIT: New mechanic for OUTSIDE_SETTLEMENT_ONLY - now this attribute simply means "whenever the character is outside his/her home"
        //    return false;
        //}
        return true;
    }
    public bool WillContinueProcess() {
        return HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING);
    }
    public void PostProcessAfterSuccessfulDoBehaviour(Character character) {
        if (HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY)) {
            DisableFor(character);

            //schedule enable for start of next day
            GameDate today = GameManager.Instance.Today();
            GameDate nextDay = today.AddDays(1);
            nextDay.SetTicks(1);
            SchedulingManager.Instance.AddEntry(nextDay, () => EnableFor(character), this);
        }
    }
    protected bool HasAttribute(params BEHAVIOUR_COMPONENT_ATTRIBUTE[] passedAttributes) {
        if(attributes != null) {
            for (int i = 0; i < attributes.Length; i++) {
                for (int j = 0; j < passedAttributes.Length; j++) {
                    if (attributes[i] == passedAttributes[j]) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

}
