using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PortableClasses.Services
{
    public class FileService : IDisposable
    {
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

        public bool IsDisposing { get; private set; }

        #endregion

        #region Métodos

        #region Read

        private object ReadGIF()
        {
            System.IO.Stream fileStream = System.IO.File.Open(Path, System.IO.FileMode.Open);
            return fileStream;
        }

        private object ReadJSON()
        {
            using (System.IO.Stream fileStream = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(fileStream, Encoding.UTF8);

                return (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }
        }

        private object ReadBIN()
        {
            using (System.IO.Stream fileStream = System.IO.File.Open(Path, System.IO.FileMode.Open))
            {
                System.IO.BinaryReader reader = new System.IO.BinaryReader(fileStream, Encoding.UTF8);

                byte[] encryptedText = reader.ReadBytes((int)reader.BaseStream.Length);
                byte[] deEncryptText = SecurityProtocolService.DecryptTripleDES(encryptedText);
                string desEncryptedText = Encoding.UTF8.GetString(deEncryptText);

                return Value = (JObject)JToken.Parse(desEncryptedText);
            }
        }

        #endregion

        #region Write

        private void WriteGIF()
        {
            using (System.IO.StreamWriter file = System.IO.File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Value);
            }
        }

        private void WriteJSON()
        {
            using (System.IO.StreamWriter file = System.IO.File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Value);
            }
        }

        private void WriteBIN()
        {
            using (System.IO.Stream fileStream = System.IO.File.Open(Path, System.IO.FileMode.Create))
            {
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fileStream, Encoding.UTF8);
                var text = ((JObject)Value).ToString();

                byte[] stringInBytes = Encoding.UTF8.GetBytes(text);
                writer.Write(SecurityProtocolService.EncryptTripleDES(stringInBytes));
            }
        }

        #endregion

        #region Métodos Auxiliares

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public T Read<T>(string path = null) => ((JObject)Read(path)).ToObject<T>();

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
                case ".gif":
                    Value = ReadGIF();
                    break;
            }

            return Value;
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
                case ".gif":
                    WriteGIF();
                    break;
            }
        }

        public void Write(object value, string path)
        {
            Value = value; Path = path;
            switch (System.IO.Path.GetExtension(Name))
            {
                case ".json":
                    WriteJSON();
                    break;
                case ".bin":
                    WriteBIN();
                    break;
                case ".gif":
                    WriteGIF();
                    break;
            }
        }

        public void Dispose(bool isDisposing)
        {
            IsDisposing = isDisposing;
            if (IsDisposing) Dispose();
        }

        public void Dispose()
        {
            Path = null;
            Value = null;
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
