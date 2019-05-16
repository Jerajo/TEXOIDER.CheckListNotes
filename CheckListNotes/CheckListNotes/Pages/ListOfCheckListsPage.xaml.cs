using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Collections.Generic;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListOfCheckListsPage : ContentPage
    {
        public ListOfCheckListsPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}