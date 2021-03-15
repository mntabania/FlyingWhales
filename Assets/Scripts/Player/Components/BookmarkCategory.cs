using System.Collections.Generic;

public class BookmarkCategory {

    #region IListener
    public interface IListener {
        void OnBookmarkAdded(IBookmarkable p_bookmarkable);
        void OnBookmarkRemoved(IBookmarkable p_bookmarkable);
    }
    #endregion
    
    public string displayName { get; }
    public BOOKMARK_CATEGORY bookmarkCategory { get; }
    public List<IBookmarkable> bookmarked { get; }

    private System.Action<IBookmarkable> _onBookmarkAdded;
    private System.Action<IBookmarkable> _onBookmarkRemoved;
    
    public BookmarkCategory(BOOKMARK_CATEGORY p_category) {
        bookmarkCategory = p_category;
        bookmarked = new List<IBookmarkable>();
        displayName = p_category.ToString();
    }

    public void AddBookmark(IBookmarkable p_bookmarkable) {
        bookmarked.Add(p_bookmarkable);
    }
    public void RemoveBookmark(IBookmarkable p_bookmarkable) {
        bookmarked.Remove(p_bookmarkable);
    }

    #region Listeners
    public void SubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded += p_listener.OnBookmarkAdded;
        _onBookmarkRemoved += p_listener.OnBookmarkRemoved;
    }
    public void UnsubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded -= p_listener.OnBookmarkAdded;
        _onBookmarkRemoved -= p_listener.OnBookmarkRemoved;
    }
    #endregion
}