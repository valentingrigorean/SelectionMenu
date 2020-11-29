// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SelectionSample
{
	[Register ("SelectionTableViewCell")]
	partial class SelectionTableViewCell
	{
		[Outlet]
		UIKit.UIImageView CheckImage { get; set; }

		[Outlet]
		UIKit.UILabel NameLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}

			if (CheckImage != null) {
				CheckImage.Dispose ();
				CheckImage = null;
			}

		}
	}
}
