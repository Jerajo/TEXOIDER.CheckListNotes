using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;

namespace CheckListNotes.PageModels.Commands
{
    /// <summary>
    /// Clase MultiBinding: Premite hacer multiples Bindings en una sola linea.
    /// </summary>
    [ContentProperty(nameof(Bindings))]
    public class MultiBindingCommand : IMarkupExtension
    {
        public IList<object> Bindings { get; } = new List<object>();

        public string StringFormat { get; set; }

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
