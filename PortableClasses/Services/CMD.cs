namespace PortableClasses.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    public class CMD : IDisposable
    {
        #region Atributes

        private bool? isDisposing;
        string arguments;
        Process process;
        ProcessStartInfo startInfo;

        #endregion

        public CMD(string arguments) : this() => Arguments = arguments;
        public CMD()
        {
            isDisposing = false;
            process = new Process();
            startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe"
            };
        }

        #region SETTERS AND GETTERS

        public string Arguments
        {
            get => arguments;
            set => arguments = $"/C {value}";
        }

        #endregion

        #region Methods

        public bool Execute(string arguments)
        {
            Arguments = arguments;
            return Execute();
        }

        public bool Execute()
        {
            if (string.IsNullOrEmpty(Arguments))
                throw new ArgumentNullException(nameof(Arguments));

            startInfo.Arguments = Arguments;
            process.StartInfo = startInfo;

            return process.Start();
        }

        public Task<bool> ExecuteAsync(string arguments = null)
        {
            if (arguments != null) Arguments = arguments;
            if (string.IsNullOrEmpty(Arguments))
                throw new ArgumentNullException(nameof(Arguments));

            startInfo.Arguments = Arguments;
            process.StartInfo = startInfo;

            return Task.FromResult(process.Start());
        }

        #endregion

        #region Dispose

        ~CMD()
        {
            if (isDisposing == false) Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            process = null;
            startInfo = null;
            Arguments = null;
            isDisposing = null;
#if DEBUG
            //Debug.WriteLine($"Object destroyect:[ Name: {nameof(CMD)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}

#region Implementation

/* /-----------------------------/ Execute /-----------------------------/
 * var command = "copy /b Image1.jpg + Archive.rar Image2.jpg"
 * using (var service = new CMD(command))
 * {
 *     var resoult = service.Execute();
 * }
 */

/* /--------------------------/ Execute Async /--------------------------/
 * var command = "copy /b Image1.jpg + Archive.rar Image2.jpg"
 * var resoult = await (new CMD()).ExecuteAsync(command);
 */

#endregion
