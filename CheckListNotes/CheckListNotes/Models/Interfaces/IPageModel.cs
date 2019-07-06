namespace CheckListNotes.Models.Interfaces
{
    using FreshMvvm;
    using System.Threading.Tasks;
    public interface IPageModel
    {
        void Init(object initData);
        Task<bool> ShowAlertQuestion(string title, string message, params ButtonModel[] models);
        Task ShowAlertError(string message);
        Task PushPageModel<T>(object data) where T : FreshBasePageModel;
        Task PopPageModel(object data);
        Task RegisterToast(CheckTaskViewModel task);
        Task UnregisterToast(string toastId);
        Task RefreshUI();
    }
}
