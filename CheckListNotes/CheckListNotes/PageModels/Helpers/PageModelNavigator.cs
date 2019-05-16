using System;
using FreshMvvm;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace CheckListNotes.PageModels.Helpers
{
    public partial class PageModelNavigator : NavigationPage, IFreshNavigationService
    {
        public PageModelNavigator(Page page)
            : this(page, Constants.DefaultNavigationServiceName)
        {
        }

        public PageModelNavigator(Page page, string navigationPageName)
            : base(page)
        {
            var pageModel = page.GetModel();
            if (pageModel == null)
                throw new InvalidCastException("BindingContext was not a FreshBasePageModel on this Page");

            pageModel.CurrentNavigationServiceName = navigationPageName;
            NavigationServiceName = navigationPageName;
            RegisterNavigation();
        }

        #region SETTERS AND GETTERS

        public string NavigationServiceName { get; }

        #endregion

        #region Methods

        #region Navigaion Methods

        public Task PushPage(Page page, FreshBasePageModel model, bool modal = false, bool animate = true)
        {
            if (modal)
                return base.Navigation.PushModalAsync(CreatePageSafe(page), animate);
            return base.Navigation.PushAsync(page, animate);
        }

        public Task PopPage(bool modal = false, bool animate = true)
        {
            if (modal)
                return base.Navigation.PopModalAsync(animate);
            return base.Navigation.PopAsync(animate);
        }

        public Task PopToRoot(bool animate = true)
        {
            return Navigation.PopToRootAsync(animate);
        }

        #endregion

        #region Auxiliary Methods

        protected void RegisterNavigation()
        {
            FreshIOC.Container.Register<IFreshNavigationService>(this, NavigationServiceName);
        }

        public Page CreatePageSafe(Page page)
        {
            if (page is NavigationPage || page is MasterDetailPage || page is TabbedPage)
                return page;

            return new NavigationPage(page);
        }
        #endregion

        #region Not implemented Methods

        public void NotifyChildrenPageWasPopped()
        {
            throw new NotImplementedException();
        }

        public Task<FreshBasePageModel> SwitchSelectedRootPageModel<T>() where T : FreshBasePageModel
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
