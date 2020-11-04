using UnityEngine;
using System.Collections;
using ECS;

public class AvatarCollisionHandler : MonoBehaviour {

    [SerializeField] private CharacterAvatar _parentAvatar;

    #region getters/setters
    public CharacterAvatar parentAvatar {
        get { return _parentAvatar; }
    }
    #endregion

    #region Monobehaviour
    public void OnTriggerEnter2D(Collider2D other) {
        //if (other is EdgeCollider2D) {
        CharacterAvatar otherAvatar = other.GetComponent<AvatarCollisionHandler>().parentAvatar;
        //Debug.Log(parentAvatar.mainCharacter.name + " collided with " + otherAvatar.mainCharacter.name + "'s " + other.GetType().ToString());
        Character combatant1 = parentAvatar.mainCharacter;
        //if (parentAvatar.mainCharacter.party != null) {
        //    combatant1 = parentAvatar.mainCharacter.party;
        //}
        Character combatant2 = otherAvatar.mainCharacter;
        //if (otherAvatar.mainCharacter.party != null) {
        //    combatant2 = otherAvatar.mainCharacter.party;
        //}
        Messenger.Broadcast(Signals.COLLIDED_WITH_CHARACTER, combatant1, combatant2);
        //}
    }
    #endregion
}
