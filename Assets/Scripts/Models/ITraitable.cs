using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Interface for objects that can have traits.
    /// </summary>
    public interface ITraitable : IDamageable {
        new string name { get; }
        ITraitContainer traitContainer { get; } 
        TraitProcessor traitProcessor { get; }
        Transform worldObject { get; }

        void CreateTraitContainer();
        void AddAdvertisedAction(INTERACTION_TYPE actionType);
        void RemoveAdvertisedAction(INTERACTION_TYPE actionType);
    }
    
    public delegate void TraitableCallback(ITraitable traitable);
}

