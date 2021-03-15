using System.Collections.Generic;
using System.Linq;

public class BookmarkComponent {
    
    public Dictionary<BOOKMARK_CATEGORY, BookmarkCategory> bookmarkedObjects { get; }

    public BookmarkComponent() {
        bookmarkedObjects = new Dictionary<BOOKMARK_CATEGORY, BookmarkCategory>();
    }

    public void AddBookmark(IBookmarkable p_bookmarkable) {
        if (!bookmarkedObjects.ContainsKey(p_bookmarkable.bookmarkCategory)) {
            bookmarkedObjects.Add(p_bookmarkable.bookmarkCategory, new BookmarkCategory(p_bookmarkable.bookmarkCategory));
        }
        bookmarkedObjects[p_bookmarkable.bookmarkCategory].AddBookmark(p_bookmarkable);
    }
    public void RemoveBookmark(IBookmarkable p_bookmarkable) {
        if (bookmarkedObjects.ContainsKey(p_bookmarkable.bookmarkCategory)) {
            bookmarkedObjects[p_bookmarkable.bookmarkCategory].RemoveBookmark(p_bookmarkable);
        }
    }
}

public class SaveDataBookmarkComponent : SaveData<BookmarkComponent>{
    //TODO:
}