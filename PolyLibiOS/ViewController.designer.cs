// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace PolyLibiOS
{
    [Register("ViewController")]
    partial class ViewController
    {
        [Outlet]
        UIKit.UITextField cellSizeInput { get; set; }


        [Outlet]
        UIKit.UIButton generateButton { get; set; }


        [Outlet]
        UIKit.UITextField heightInput { get; set; }


        [Outlet]
        UIKit.UITextField varInput { get; set; }


        [Outlet]
        UIKit.UITextField widthInput { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        PolyLib.Views.iOS.PolyLibView PolyLibView { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (cellSizeInput != null)
            {
                cellSizeInput.Dispose();
                cellSizeInput = null;
            }

            if (generateButton != null)
            {
                generateButton.Dispose();
                generateButton = null;
            }

            if (heightInput != null)
            {
                heightInput.Dispose();
                heightInput = null;
            }

            if (PolyLibView != null)
            {
                PolyLibView.Dispose();
                PolyLibView = null;
            }

            if (varInput != null)
            {
                varInput.Dispose();
                varInput = null;
            }

            if (widthInput != null)
            {
                widthInput.Dispose();
                widthInput = null;
            }
        }
    }
}