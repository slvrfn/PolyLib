using System;

using UIKit;

namespace LowPolyiOS
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            widthInput.Text = $"{lowPolyView.Frame.Size.Width * UIScreen.MainScreen.Scale}";
            heightInput.Text = $"{lowPolyView.Frame.Size.Height * UIScreen.MainScreen.Scale}";
            varInput.Text = ".75";
            cellSizeInput.Text = "150";

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

        }

        void Generate(object sender, EventArgs e)
        {
            var boundsWidth = Int32.Parse(widthInput.Text);
            var boundsHeight = Int32.Parse(heightInput.Text);

            var variance = float.Parse(varInput.Text);
            var cellSize = int.Parse(cellSizeInput.Text);

            lowPolyView.GenerateNewTriangulation(boundsWidth, boundsHeight, variance, cellSize);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
