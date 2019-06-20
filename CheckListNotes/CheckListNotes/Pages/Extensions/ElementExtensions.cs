using Xamarin.Forms;

namespace CheckListNotes.Pages.Extensions
{
    public static class ElementExtensions
    {
        public static Rectangle Bounds(this Element value)
        {
            if (!(value is VisualElement parent)) return new Rectangle();
            else return parent.Bounds;
        }

        public static Rectangle BoundsWithOutPadding(this Element value)
        {
            if (!(value is VisualElement parent)) return new Rectangle();

            var padding = value.Padding();
            var parentBounds = parent.Bounds;

            parentBounds.Width -= padding.HorizontalThickness;
            parentBounds.Height -= padding.VerticalThickness;

            return parentBounds;
        }

        public static Thickness Padding(this Element value)
        {
            if (!(value is Layout parent)) return new Thickness();
            else return parent.Padding;
        }
    }
}
