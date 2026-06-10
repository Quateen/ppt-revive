public class SlideTextItem
{
    public int SlideIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public string ShapeName { get; set; } = string.Empty;
    public bool IsBullet { get; set; }
    public bool IsAuthorOrFooter { get; set; }
    public int Level { get; set; } = 0; // ✅ New property for bullet indentation level

    // Required for deserialization and object initializer support
    public SlideTextItem() { }

    public SlideTextItem(int slideIndex, string text, string shapeName, bool isBullet, bool isAuthorOrFooter, int level = 0)
    {
        SlideIndex = slideIndex;
        Text = text;
        ShapeName = shapeName;
        IsBullet = isBullet;
        IsAuthorOrFooter = isAuthorOrFooter;
        Level = level;
    }
}
