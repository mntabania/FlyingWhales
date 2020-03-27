using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is the base class for all vision triggers of IPointOfInterest's
/// </summary>
public abstract class POIVisionTrigger : BaseVisionTrigger {
    
    public IPointOfInterest poi { get; private set; }
    public override void Initialize(IDamageable damageable) {
        base.Initialize(damageable);
        poi = damageable as IPointOfInterest;
    }
    /// <summary>
    /// Does this collision trigger ignore structure differences when something sees it.
    /// </summary>
    /// <returns>True or false</returns>
    public abstract bool IgnoresStructureDifference();
}