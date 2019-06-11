using Windows.System;
using System.Diagnostics;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Activation;

namespace CheckListNotes.UWP
{
    public sealed partial class MainPage
    {
        ProtocolForResultsOperation operation = null;
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new CheckListNotes.App());
        }

        #region Override

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ProtocolForResultsActivatedEventArgs protocolForResultsArgs)
            {
                // Set the ProtocolForResultsOperation field.
                operation = protocolForResultsArgs.ProtocolForResultsOperation;

                if (protocolForResultsArgs.Data.ContainsKey("TestData"))
                {
                    string dataFromCaller = protocolForResultsArgs.Data["TestData"] as string;
                    //response to the calling app
                    var result = new ValueSet();
                    result["ReturnedData"] = "The returned result";
                    operation.ReportCompleted(result);
                }
            }
        }

        #endregion

        #region Dispose

        ~MainPage()
        {
            operation = null;
#if DEBUG
            Debug.WriteLine("Object destroyed: [ Id: {1}, Name: {0} ].", this.GetHashCode(), nameof(MainPage));
#endif
        }

        #endregion
    }
}
