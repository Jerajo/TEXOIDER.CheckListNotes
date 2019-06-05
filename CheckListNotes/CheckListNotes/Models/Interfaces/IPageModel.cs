namespace CheckListNotes.Models.Interfaces
{
    using FreshMvvm;
    using System.Threading.Tasks;
    public interface IPageModel
    {
        void Init(object initData);
        Task<bool> ShowAlert(string title, string message, string accept, string cancel);
        Task ShowAlert(string title, string message, string cancel);
        Task PushPageModel<T>(object data) where T : FreshBasePageModel;
        Task PopPageModel(object data);
        Task RegisterToast(CheckTaskViewModel task);
        Task UnregisterToast(string toastId);
        Task RefreshUI();
    }
}
