using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PortableClasses.Services
{
    public class FileService : IDisposable
    {
        #region Atributes

        private JObject jObject;
        public bool? isDisposing = false;
        private JsonSerializer serializer;
        private JsonTextReader jsonReader;
        private System.IO.Stream fileReader;
        private System.IO.StreamWriter streamWriter;
        private System.IO.StreamReader streamReader;
        private System.IO.BinaryWriter binaryWriter;
        private System.IO.BinaryReader binaryReader;

        #endregion

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

        private JToken ReadJSON()
        {
            using (fileReader = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                streamReader = new System.IO.StreamReader(fileReader, Encoding.UTF8);
                jsonReader = new JsonTextReader(streamReader);
                return JToken.ReadFrom(jsonReader);
            }
        }

        private JToken ReadBIN()
        {
            using (fileReader = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                binaryReader = new System.IO.BinaryReader(fileReader, Encoding.UTF8);

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
            using (streamWriter = System.IO.File.CreateText(Path))
            {
                serializer = new JsonSerializer();
                serializer.Serialize(streamWriter, Value);
            }
        }

        private void WriteBIN()
        {
            using (fileReader = System.IO.File.Open(Path, System.IO.FileMode.Create))
            {
                binaryWriter = new System.IO.BinaryWriter(fileReader, Encoding.UTF8);
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

        public T Read<T>(string path = null)
        {
            jObject = Read(path) as JObject;
            return jObject.ToObject<T>();
        }

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

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            Path = null;
            Value = null;
            jObject = null;
            fileReader = null;
            jsonReader = null;
            streamReader = null;
            binaryReader = null;
            streamWriter = null;
            binaryWriter = null;
            isDisposing = null;
        }

        ~FileService()
        {
            if (isDisposing == false) Dispose(true);
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
