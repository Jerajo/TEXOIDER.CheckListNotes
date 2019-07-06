using System;
using Xamarin.Forms;

namespace CheckListNotes.Models
{
    public class ButtonModel : IDisposable
    {
        #region Atributes

        ImageSource icon;
        Color? textColor;
        bool? isDisposing;
        Color? backgroundColor;
        ButtonModelTypes? type;

        #endregion

        #region Constructors

        public ButtonModel(ButtonModelTypes type) : this()
        {
            switch (type)
            {
                case ButtonModelTypes.ButtonOk: Init(ButtonOk); break;
                case ButtonModelTypes.ButtonEdit: Init(ButtonEdit); break;
                case ButtonModelTypes.ButtonDelete: Init(ButtonDelete); break;
                case ButtonModelTypes.ButtonSave: Init(ButtonSave); break;
                case ButtonModelTypes.ButtonCancel: Init(ButtonCancel); break;
                case ButtonModelTypes.None:
                default: this.type = type; break;
            }
        }

        public ButtonModel(string text, Color? textColor = null, Color? backgroundColor = null, ImageSource icon = null) : this()
        {
            this.Text = text;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            this.icon = icon;
        }

        private ButtonModel() => this.isDisposing = false;

        #endregion

        #region Statics

        public static ButtonModel ButtonOk
        {
            get => new ButtonModel()
            {
                type = ButtonModelTypes.ButtonOk,
                backgroundColor = Color.Transparent,
                Text = AppResourcesLisener.Languages["ButtonOkText"],
                textColor = AppResourcesLisener.Themes["FontColor"]
            };
        }

        public static ButtonModel ButtonCancel
        {
            get => new ButtonModel()
            {
                type = ButtonModelTypes.ButtonCancel,
                backgroundColor = Color.Transparent,
                Text = AppResourcesLisener.Languages["ButtonCancelText"],
                textColor = AppResourcesLisener.Themes["FontColor"],
            };
        }

        public static ButtonModel ButtonEdit
        {
            get => new ButtonModel()
            {
                type = ButtonModelTypes.ButtonEdit,
                backgroundColor = Color.Transparent,
                icon = AppResourcesLisener.Images.GetImageSource("Data-Edit-Icon", AppResourcesLisener.Themes["ContentPrimary"], 26),
                Text = AppResourcesLisener.Languages["ButtonEditText"],
                textColor = AppResourcesLisener.Themes["ContentPrimary"],
            };
        }

        public static ButtonModel ButtonDelete
        {
            get => new ButtonModel()
            {
                type = ButtonModelTypes.ButtonDelete,
                backgroundColor = Color.Transparent,
                icon = AppResourcesLisener.Images.GetImageSource("Garbage-Closed-Icon", AppResourcesLisener.Themes["Error"], 26),
                Text = AppResourcesLisener.Languages["ButtonDeleteText"],
                textColor = AppResourcesLisener.Themes["Error"],
            };
        }

        public static ButtonModel ButtonSave
        {
            get => new ButtonModel()
            {
                type = ButtonModelTypes.ButtonSave,
                backgroundColor = Color.Transparent,
                icon = AppResourcesLisener.Images.GetImageSource("Add-New-Icon", AppResourcesLisener.Themes["Good"], 26),
                Text = AppResourcesLisener.Languages["ButtonSaveText"],
                textColor = AppResourcesLisener.Themes["Good"],
            };
        }

        #endregion

        #region Base Atributes

        public string Text { get; set; }

        public ImageSource Icon { get => icon; }

        public ButtonModelTypes Type { get => type ?? ButtonModelTypes.None; }

        public Color BackgroundColor { get => backgroundColor ?? Color.Transparent; }

        public Color TextColor { get => textColor ?? AppResourcesLisener.Themes["FontColor"]; }

        #endregion

        #region Methods

        private void Init(ButtonModel model)
        {
            this.Text = model.Text;
            this.textColor = model.textColor;
            this.backgroundColor = model.backgroundColor;
            this.icon = model.icon;
        }

        #endregion

        #region Dispose

        ~ButtonModel()
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
            Text = null;
            icon = null;
            type = null;
            textColor = null;
            backgroundColor = null;
            isDisposing = null;
        }

        #endregion
    }

    #region Auxiliary Items

    public enum ButtonModelTypes
    {
        None = -1,
        ButtonOk = 0,
        ButtonEdit = 1,
        ButtonDelete = 2,
        ButtonSave = 3,
        ButtonCancel = 4
    }

    #endregion
}
