using Foundation;
using System;
using System.Diagnostics;
using UIKit;

namespace SelectionSample
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.


            Button.TouchUpInside += async (sender, args) =>
            {
                var items = new[]
                {
                    new SampleItem("Roxanne Bailey"),
                    new SampleItem("Jack Clayton"),
                    new SampleItem("Ron Lynch"),
                    new SampleItem("Pat Garrett"),
                    new SampleItem("Irma Norton"),
                    new SampleItem("Kathy Dennis")
                };

                var result = await SelectionViewController.GetSelectedItemsAsync(this, items, Switch.On);

                Debug.WriteLine("Selected items:");
                foreach (var item in result)
                {
                    Debug.WriteLine($"\t{item.Name}");
                }
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}