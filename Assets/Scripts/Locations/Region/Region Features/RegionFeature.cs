namespace Locations.Region_Features {
    public abstract class RegionFeature {
        
        /// <summary>
        /// Stuff to do to regions with this feature after first landmark generation.
        /// </summary>
        /// <param name="region">The region that has this feature</param>
        public virtual void SpecialStructureGenerationSecondPassActions(Region region){ }
        /// <summary>
        /// Stuff to do to regions with this feature when all structures have been generated
        /// </summary>
        /// <param name="region">The region that has this feature</param>
        public virtual void ActivateFeatureInWorldGen(Region region){ }
    }
}