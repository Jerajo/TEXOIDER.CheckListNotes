using System;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Windows.Input;
using SkiaSharp.Views.Forms;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UCSvgButtonImage : ContentView
    {
        public UCSvgButtonImage()
        {
            InitializeComponent();

            Padding = new Thickness(0);
            BackgroundColor = Color.Transparent;

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = ItemTappedCommand
            });
        }

        #region SETTERS AND GETTERS

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
                                                         propertyName: "Source",
                                                         returnType: typeof(string),
                                                         declaringType: typeof(UCSvgButtonImage),
                                                         defaultValue: "",
                                                         defaultBindingMode: BindingMode.OneWay,
                                                         propertyChanged: SourcePropertyChanged);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(UCSvgButtonImage), null);

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create("CommandParameter", typeof(object), typeof(UCSvgButtonImage), null);

        public event EventHandler ItemTapped = (e, a) => { };

        #endregion

        #region Métodos

        private static void SourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (UCSvgButtonImage)bindable;
            control?.canvasView.InvalidateSurface();
        }

        private ICommand ItemTappedCommand
        {
            get
            {
                return new Command(() =>
                {
                    Command?.Execute(CommandParameter);

                    ItemTapped(this, EventArgs.Empty);
                });
            }
        }

        #endregion

        #region Eventos

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {

            SKImageInfo info = args.Info;
            SKCanvas canvas = args.Surface.Canvas;
            SkiaSharp.Extended.Svg.SKSvg SvgFile = new SkiaSharp.Extended.Svg.SKSvg();

            //linpia el canvas
            canvas.Clear();

            // Carga el svg
            if (string.IsNullOrEmpty(Source)) return;

            SvgFile.Load(Source);

            //// calculate the scaling need to fit
            //float canvasMin = Math.Min(info.Width, info.Height);

            //// get the rectangle that the SVG is defined in
            //var svgSize = SvgFile.Picture.CullRect;

            ////calculate the corret scale
            //float scale = 0;
            //if (canvasMin == Width) scale = canvasMin / svgSize.Width;
            //else scale = canvasMin / svgSize.Height;

            //var matrix = SKMatrix.MakeScale(scale, scale);

            ////centraliza el objeto
            //var xPoint = (info.Width / 2f) - ((svgSize.Width / 2f) * scale);
            //var yPoint = (info.Height / 2f) - ((svgSize.Height / 2f) * scale);
            //canvas.Translate(xPoint, yPoint);

            //// draw the svg
            //canvas.DrawPicture(SvgFile.Picture, ref matrix);

            canvas.Translate(info.Width / 2f, info.Height / 2f);

            SKRect bounds = SvgFile.Picture.CullRect;
            float xRatio = info.Width / bounds.Width;
            float yRatio = info.Height / bounds.Height;

            float ratio = Math.Min(xRatio, yRatio);

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            // draw the svg
            canvas.DrawPicture(SvgFile.Picture);
        }

        #endregion
    }
}