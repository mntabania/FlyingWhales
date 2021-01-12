using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Inner_Maps.Map_Objects.Map_Object_Visuals {
    public class TreeGameObject : TileObjectGameObject {
        [SerializeField] private StructureConnector _structureConnector;
        public StructureConnector structureConnector => _structureConnector;
    }
}