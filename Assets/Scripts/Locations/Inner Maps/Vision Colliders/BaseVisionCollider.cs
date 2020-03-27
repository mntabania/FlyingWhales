using UnityEngine;
using UtilityScripts;

/// <summary>
/// Base class for all colliders that are used to see other objects
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseVisionCollider : MonoBehaviour {

    private int _filterVisionVotes;
    
    protected abstract void OnTriggerEnter2D(Collider2D collider2D);
    protected abstract void OnTriggerExit2D(Collider2D collider2D);

    #region Layers
    public void VoteToFilterVision() {
        _filterVisionVotes += 1;
        DetermineVisionLayerGivenVotes();
    }
    public void VoteToUnFilterVision() {
        _filterVisionVotes -= 1;
        DetermineVisionLayerGivenVotes();
    }
    private void DetermineVisionLayerGivenVotes() {
        if (_filterVisionVotes > 0) {
            FilterVision();
        } else {
            UnFilterVision();
        }
    }
    /// <summary>
    /// Make this collider only see objects that are part of the filtered layer.
    /// </summary>
    private void FilterVision() {
        gameObject.layer = LayerMask.NameToLayer(GameUtilities.Filtered_Vision_Layer);
    }
    /// <summary>
    /// Make this collider see all objects.
    /// </summary>
    private void UnFilterVision() {
        gameObject.layer = LayerMask.NameToLayer(GameUtilities.Unfiltered_Vision_Layer);
    }
    #endregion
}
