using System.Collections.Generic;

public class BookmarkCategory {

    #region IListener
    public interface IListener {
        void OnBookmarkAdded(IBookmarkable p_bookmarkable);
    }
    #endregion
    
    public string displayName { get; }
    public BOOKMARK_CATEGORY bookmarkCategory { get; }
    public List<IBookmarkable> bookmarked { get; }

    private System.Action<IBookmarkable> _onBookmarkAdded;
    
    public BookmarkCategory(BOOKMARK_CATEGORY p_category) {
        bookmarkCategory = p_category;
        bookmarked = new List<IBookmarkable>();
        displayName = p_category.ToString();
    }

    public void AddBookmark(IBookmarkable p_bookmarkable) {
        bookmarked.Add(p_bookmarkable);
        ExecuteBookmarkAdded(p_bookmarkable);
    }
    public void RemoveBookmark(IBookmarkable p_bookmarkable) {
        bookmarked.Remove(p_bookmarkable);
        p_bookmarkable.eventDispatcher.ExecuteBookmarkRemovedEvent(p_bookmarkable);
    }

    #region Listeners
    public void SubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded += p_listener.OnBookmarkAdded;
    }
    public void UnsubscribeToEvents(IListener p_listener) {
        _onBookmarkAdded -= p_listener.OnBookmarkAdded;
    }
    private void ExecuteBookmarkAdded(IBookmarkable p_bookmarkable) {
        _onBookmarkAdded?.Invoke(p_bookmarkable);
    }
    #endregion
}