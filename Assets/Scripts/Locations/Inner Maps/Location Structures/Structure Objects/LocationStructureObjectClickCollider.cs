using System;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    /// <summary>
    /// This is used for click interaction with an UNBUILT LocationStructureObject.
    /// This is so that the player can click on the structure and check its status, even though it has not been fully built.
    /// This is necessary since LocationStructure instances are created once the structure has been fully built.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class LocationStructureObjectClickCollider : MonoBehaviour {

        [SerializeField] private BoxCollider2D clickCollider;
        
        public LocationStructureObject structureObject;
        
        private void Awake() {
            gameObject.tag = "Location Structure Object";
        }
        public void Enable() {
            enabled = true;
            clickCollider.enabled = true;
        }
        public void Disable() {
            enabled = false;
            clickCollider.enabled = false;
        }
    }
}