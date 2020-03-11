using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarkerHostilityCollision : MonoBehaviour {

    public CharacterMarker parentMarker;

    #region Triggers
    //public void OnTriggerEnter2D(Collider2D collision) {
    //    POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
    //    if (collidedWith != null && collidedWith.poi != null
    //        && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
    //        && collidedWith.poi != parentMarker.character) {
    //        //&& collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure
    //        parentcombatComponent.AddHostileInRange(collidedWith.poi as Character);
    //    }
    //}
    public void OnTriggerExit2D(Collider2D collision) {
        //if (!parentMarker.character.IsInOwnParty()) {
        //    return;
        //}
        //POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
        //if (collidedWith != null 
        //    && collidedWith.poi != null
        //    && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
        //    && collidedWith.poi != parentMarker.character) {
        //    Character target = collidedWith.poi as Character;
        //    //if (!target.IsInOwnParty()) {
        //    //    return;
        //    //}
        //    //parentcombatComponent.RemoveHostileInRange(collidedWith.poi as Character);
        //    parentcombatComponent.RemoveAvoidInRange(collidedWith.poi as Character);
        //}
    }
    //public void OnTriggerStay2D(Collider2D collision) {
    //    POICollisionTrigger collidedWith = collision.gameObject.GetComponent<POICollisionTrigger>();
    //    if (collidedWith != null
    //        && collidedWith.poi != null
    //        && collidedWith.poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER
    //        && collidedWith.poi != parentMarker.character) {

    //        if (collidedWith.gridTileLocation.structure == parentMarker.character.currentStructure) {
    //            parentcombatComponent.AddHostileInRange(collidedWith.poi as Character);
    //        } else if (collidedWith.gridTileLocation.structure != parentMarker.character.currentStructure) {
    //            parentcombatComponent.RemoveHostileInRange(collidedWith.poi as Character);
    //        }

    //    }
    //}
    #endregion

    //#region For Testing
    //private void FixedUpdate() {
    //    List<Character> charactersInRange = new List<Character>();
    //    Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 5);
    //    for (int i = 0; i < colliders.Length; i++) {
    //        Collider2D currCollider = colliders[i];
    //        CharacterCollisionTrigger cct = currCollider.gameObject.GetComponent<CharacterCollisionTrigger>();
    //        if (cct != null && cct.poi is Character) {
    //            charactersInRange.Add(cct.poi as Character);
    //        }
    //    }
    //    for (int i = 0; i < parentcombatComponent.hostilesInRange.Count; i++) {
    //        Character hostileInRange = parentcombatComponent.hostilesInRange[i];
    //        if (!charactersInRange.Contains(hostileInRange)) { //there is a hostile in range that is not actually in range
    //            Debug.LogWarning("Inconsistent characters in range for " + parentMarker.name + " (" + hostileInRange.name + ").");
    //            UIManager.Instance.Pause();
    //        }
    //    }
    //}
    //#endregion
}
