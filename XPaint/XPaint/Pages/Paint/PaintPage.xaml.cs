using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XPaint.Pages.Paint
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaintPage : ContentPage
    {
        Dictionary<long, (SKPath, SKPaint)> inProgressPaths = new Dictionary<long, (SKPath, SKPaint)>();
        List<(SKPath, SKPaint)> completedPaths = new List<(SKPath, SKPaint)>();
        SKPaint currentPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Color.CornflowerBlue.ToSKColor(),
            StrokeWidth = 10f,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public PaintPage()
        {
            InitializeComponent();
        }

        #region Drawing
        private void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(args.Location));
                        inProgressPaths.Add(args.Id, (path, currentPaint.Clone()));
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        (SKPath, SKPaint) path = inProgressPaths[args.Id];
                        path.Item1.LineTo(ConvertToPixel(args.Location));
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        completedPaths.Add(inProgressPaths[args.Id]);
                        inProgressPaths.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        inProgressPaths.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (var path in completedPaths)
            {
                canvas.DrawPath(path.Item1, path.Item2);
            }

            foreach (var path in inProgressPaths.Values)
            {
                canvas.DrawPath(path.Item1, path.Item2);
            }
        }

        SKPoint ConvertToPixel(TouchTrackingPoint pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }
        #endregion

        #region Menu Control
        private void OnShowElement(object sender, EventArgs e)
        {
            HideAllMenuElements();
            var element = ((Button)sender).BindingContext as VisualElement;
            element.IsVisible = true;
        }

        private void HideAllMenuElements()
        {
            foreach (VisualElement element in Menu.Children)
            {
                element.IsVisible = false;
            }
        }
        #endregion

        #region Options
        private void OnModeSelected(object sender, EventArgs e)
        {
            var mode = (DrawingModes)((Button)sender).BindingContext;
            switch (mode)
            {
                case DrawingModes.Normal:
                    currentPaint.MaskFilter = null;
                    break;
                case DrawingModes.Blur:
                    currentPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10);
                    break;
                case DrawingModes.Emboss:
                    break;
                default:
                    goto case DrawingModes.Normal;
            }
        }

        private void OnBrushSizeChanged(object sender, ValueChangedEventArgs args)
        {
            float value = float.Parse(args.NewValue.ToString());
            currentPaint.StrokeWidth = value;
        }

        private void OnChangeColor(object sender, EventArgs e)
        {
            var buttonColor = ((Button)sender).BackgroundColor;
            currentPaint.Color = buttonColor.ToSKColor();
        }

        private void OnEraserSelect(object sender, EventArgs e)
        {
            currentPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeWidth = currentPaint.StrokeWidth,
                Color = SKColors.Transparent,
                BlendMode = SKBlendMode.Src
            };
        }

        private void OnClearAll(object sender, EventArgs e)
        {
            inProgressPaths = new Dictionary<long, (SKPath, SKPaint)>();
            completedPaths = new List<(SKPath, SKPaint)>();
            canvasView.InvalidateSurface();
        }
        #endregion
    }
}