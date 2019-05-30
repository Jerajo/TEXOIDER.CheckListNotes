namespace PortableClasses.Interfaces
{
    public interface IToastService
    {
        IToast Toast { get; }
        bool ToastExist(IToast toast);
        bool ToastExist(string toastId);
        void ProgramToast();
        void ProgramToast(IToast toast);
        void ShowToast();
        void ShowToast(IToast toast);
        void CancelProgramedToast();
        void CancelProgramedToast(string toastId);
    }
}
