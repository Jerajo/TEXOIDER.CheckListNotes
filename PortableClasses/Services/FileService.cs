using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PortableClasses.Services
{
    public class FileService : IDisposable
    {
        public bool? isDisposing = false;
        public FileService() { }
        public FileService(string path)
        {
            Path = path;
            Read();
        }

        #region SETTERS AND GETTES

        public string Name { get => System.IO.Path.GetFileName(Path); }

        public string Path { get; set; }

        public object Value { get; set; }

        #endregion

        #region Métodos

        #region Read

        private object ReadJSON()
        {
            using (var fileReader = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                var streamReader = new System.IO.StreamReader(fileReader, Encoding.UTF8);
                var jsonReader = new JsonTextReader(streamReader);
                return JToken.ReadFrom(jsonReader);
            }
        }

        private object ReadBIN()
        {
            using (var fileReader = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                var binaryReader = new System.IO.BinaryReader(fileReader, Encoding.UTF8);

                byte[] encryptedText = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
                byte[] deEncryptText = SecurityProtocolService.DecryptTripleDES(encryptedText);
                string desEncryptedText = Encoding.UTF8.GetString(deEncryptText);

                return JToken.Parse(desEncryptedText);
            }
        }

        #endregion

        #region Write

        private void WriteJSON()
        {
            using (var streamWriter = System.IO.File.CreateText(Path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(streamWriter, Value);
            }
        }

        private void WriteBIN()
        {
            using (var fileReader = System.IO.File.Open(Path, System.IO.FileMode.Create))
            {
                var binaryWriter = new System.IO.BinaryWriter(fileReader, Encoding.UTF8);
                var text = ((JObject)Value).ToString();

                byte[] stringInBytes = Encoding.UTF8.GetBytes(text);
                binaryWriter.Write(SecurityProtocolService.EncryptTripleDES(stringInBytes));
            }
        }

        #endregion

        #region Métodos Auxiliares

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public T Read<T>(string path = null) => ((JToken)Read(path)).ToObject<T>();

        public object Read(string path = null)
        {
            if (path != null) Path = path;
            else if (Path == null) return null;

            switch (System.IO.Path.GetExtension(Name))
            {
                case ".json":
                    Value = ReadJSON();
                    break;
                case ".bin":
                    Value = ReadBIN();
                    break;
            }

            return Value;
        }

        public void Write(object value, string path)
        {
            Value = value; Path = path;
            Write();
        }

        public void Write()
        {
            switch (System.IO.Path.GetExtension(Name))
            {
                case ".json":
                    WriteJSON();
                    break;
                case ".bin":
                    WriteBIN();
                    break;
            }
        }

        #endregion

        #region Dispose

        ~FileService()
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
            Path = null;
            Value = null;
            isDisposing = null;
#if DEBUG
            //Debug.WriteLine($"Object destroyect:[ Name: {nameof(FileService)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion

        #endregion
    }

    #region Implementación

    /* Get File Model
     * var img = Application.Current.Resources?["img.svg"] as FileService;
     * 
     * Get File Value
     * ImageSource = img.Value;
     * 
     * Get a copy of the file
     * var img2 = img.Read();
     * 
     * Update File Value
     * img.Value = ImageSource;
     * 
     * Rite File Document
     * img.Write();
     * 
     */

    #endregion
}
