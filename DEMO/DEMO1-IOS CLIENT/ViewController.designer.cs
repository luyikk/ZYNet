// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace IOSTest
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView Out { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Start { get; set; }

        [Action ("Start_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void Start_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (Out != null) {
                Out.Dispose ();
                Out = null;
            }

            if (Start != null) {
                Start.Dispose ();
                Start = null;
            }
        }
    }
}