using System;
using PolyLib;
using PolyLib.Views.iOS;
using UIKit;
using Foundation;
using PolyLib.Animation;
using System.Collections.Generic;

namespace PolyLibiOS
{
    public partial class ViewController : UIViewController
    {
        Triangulation _currentTriangulation;
        PolyLibView _polyLibViewRef;

        UITapGestureRecognizer _tapGestureRecognizer;
        UIPanGestureRecognizer _panGestureRecognizer;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            _polyLibViewRef = polylibView;
            //set inputs in pixel width/height
            widthInput.Text = $"{_polyLibViewRef.Frame.Width * UIScreen.MainScreen.Scale}";
            heightInput.Text = $"{_polyLibViewRef.Frame.Height * UIScreen.MainScreen.Scale}";
            varInput.Text = ".75";
            cellSizeInput.Text = "150";

            _tapGestureRecognizer = new UITapGestureRecognizer(HandleTap);
            _panGestureRecognizer = new UIPanGestureRecognizer(HandlePanTouch);

            widthInput.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            heightInput.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            varInput.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            cellSizeInput.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            generateButton.TouchUpInside += Generate;


            _polyLibViewRef.AddGestureRecognizer(_tapGestureRecognizer);
            _polyLibViewRef.AddGestureRecognizer(_panGestureRecognizer);

            if (_currentTriangulation == null)
            {
                _currentTriangulation = _polyLibViewRef.CurrentTriangulation;
            }
        }

        void Generate(object sender, EventArgs e)
        {
            //convert pixel height/width to point
            var boundsWidth = (int)(Int32.Parse(widthInput.Text) / UIScreen.MainScreen.Scale);
            var boundsHeight = (int)(Int32.Parse(heightInput.Text) / UIScreen.MainScreen.Scale);

            var variance = float.Parse(varInput.Text);
            var cellSize = int.Parse(cellSizeInput.Text);

            if (!(boundsWidth == _polyLibViewRef.Frame.Width) || !(boundsHeight == _polyLibViewRef.Frame.Height))
            {
                _polyLibViewRef = _polyLibViewRef.ResizeView(boundsWidth, boundsHeight, new List<UIGestureRecognizer>{ _tapGestureRecognizer, _panGestureRecognizer});
                _currentTriangulation = _polyLibViewRef.CurrentTriangulation;
            }
            //set props on triangulation
            _currentTriangulation.Variance = variance;
            _currentTriangulation.CellSize = cellSize;

            var colors = Triangulation.getRandomColorBruColors(6);
            var shader = Triangulation.GetRandomGradientShader(colors, _currentTriangulation.BoundsWidth,
                _currentTriangulation.BoundsHeight);

            _currentTriangulation.GradientShader = shader;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #region Touch
        void HandleTap(UITapGestureRecognizer recognizer)
        {
            //state doesnt matter here, only executed if tap gesture
            var loc = recognizer.LocationOfTouch(0, recognizer.View);
            var touchAnimation = new RandomTouch(_currentTriangulation, 6, (float)(loc.X * UIScreen.MainScreen.Scale), (float)(loc.Y * UIScreen.MainScreen.Scale), 250);
            _polyLibViewRef.AddAnimation(touchAnimation);
        }

        void HandlePanTouch(UIPanGestureRecognizer recognizer)
        {
            bool startingAnim = false;

            if (recognizer.State == UIGestureRecognizerState.Changed)
            {
                startingAnim = true;
            }

            if (startingAnim)
            {
                var loc = recognizer.LocationOfTouch(0, recognizer.View);
                var touchAnimation = new RandomTouch(_currentTriangulation, 6, (float)(loc.X * UIScreen.MainScreen.Scale), (float)(loc.Y * UIScreen.MainScreen.Scale), 250);
                _polyLibViewRef.AddAnimation(touchAnimation);
            }
        }
        #endregion
    }
}
