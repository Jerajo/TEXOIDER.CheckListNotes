using System;
using FreshMvvm;
using System.Collections;

namespace CheckListNotes.PageModels.Helpers
{
    public class TinyIOCBultIn : IFreshIOC
    {
        internal static IoCContainer Current
        {
            get => IoCContainer.Current;
        }

        #region Register

        public IRegisterOptions Register<RegisterType>(RegisterType instance) where RegisterType : class
        {
            return IoCContainer.Current.Register(instance);
        }

        public IRegisterOptions Register<RegisterType>(RegisterType instance, string name) where RegisterType : class
        {
            return IoCContainer.Current.Register(instance, name);
        }

        IRegisterOptions IFreshIOC.Register<RegisterType, RegisterImplementation>()
        {
            return IoCContainer.Current.Register<RegisterType, RegisterImplementation>();
        }

        #endregion

        #region Selsolvers

        public object Resolve(Type resolveType)
        {
            return IoCContainer.Current.Resolve(resolveType);
        }

        public ResolveType Resolve<ResolveType>() where ResolveType : class
        {
            throw new NotImplementedException();
            //return IoCContainer.Current.Resolve<ResolveType>();
        }

        public ResolveType Resolve<ResolveType>(string name) where ResolveType : class
        {
            return IoCContainer.Current.Resolve<ResolveType>(name);
        }

        #endregion

        #region Register and Unregister

        public void Unregister<RegisterType>()
        {
            throw new NotImplementedException();
        }

        public void Unregister<RegisterType>(string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class IoCContainer
    {
        #region Singleton

        internal static IoCContainer _current;

        internal static IoCContainer Current
        {
            get => _current ?? (_current = new IoCContainer());
        }

        private Stack Container { get; set; }

        #endregion

        #region Methods

        #region Register

        internal IRegisterOptions Register<RegisterType>(RegisterType instance, string name = null) where RegisterType : class
        {
            Container.Push(instance);
            throw new NotImplementedException();
        }

        internal IRegisterOptions Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Resolve

        internal object Resolve(Type resolveType)
        {
            return Container.Pop();
        }

        internal ResolveType Resolve<ResolveType>() where ResolveType : new()
        {
            var pageModel = new ResolveType();
            Container.Push(pageModel);
            return pageModel;
        }

        internal ResolveType Resolve<ResolveType>(string name) where ResolveType : class
        {
            return Container.Pop() as ResolveType;
        }

        #endregion

        #endregion
    }
}
