using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CheckListNotes.Pages.Behaviors
{
    public class ListViewCorrectDeselectItemBehavior : Behavior<ListView>
    {
        protected override void OnAttachedTo(ListView bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.ItemSelected += BindableOnItemSelected;
        }

        protected override void OnDetachingFrom(ListView bindable)
        {
            bindable.ItemSelected -= BindableOnItemSelected;

            base.OnDetachingFrom(bindable);
        }

        private void BindableOnItemSelected(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs)
        {
            var listView = (ListView)sender;
            listView.SelectedItem = null;
        }
    }
}
