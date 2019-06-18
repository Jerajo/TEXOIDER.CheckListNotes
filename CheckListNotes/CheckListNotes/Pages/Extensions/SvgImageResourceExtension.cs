using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckListNotes.Pages.Extensions
{
    [ContentProperty(nameof(Source))]
    public class SvgImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null) return null;
            return AppResourcesLisener.Images[Source];
        }
    }
}
