using System;
using Foundation;
using SelectionMenu.iOS;
using UIKit;

namespace SelectionSample
{
    [Register("SelectionTableViewCell2")]
    public class SelectionTableViewCell2 :SelectionMenuTableViewCell<SampleItem>
    {
        private UIStackView _pm;

        private UIImageView CheckImage { get; set; }
        private UILabel NameLabel { get; set; }

        public SelectionTableViewCell2(IntPtr handle) : base(handle)
        {
            CreateView();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            SelectionStyle = UITableViewCellSelectionStyle.None;
            NameLabel.Text = "";
            CheckImage.Image = null;
        }
        
        private void CreateView()
        {
            //    SelectionStyle = UITableViewCellSelectionStyle.Gray;

            _pm = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMarginsRelativeArrangement = true,
                LayoutMargins = new UIEdgeInsets(10, 20, 10, 10),

                Spacing = 5
            };

            ContentView.AddSubview(_pm);

            NameLabel = new UILabel
            {
                Lines = 0,
                LineBreakMode = UILineBreakMode.WordWrap,
                Font = UIFont.PreferredFootnote, //  PreferredSubheadline,
                BackgroundColor = UIColor.Clear
            };

            //    headingLabel.SetContentHuggingPriority(100, UILayoutConstraintAxis.Horizontal);
            NameLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            var margins = ContentView.SafeAreaLayoutGuide;
            var contraints = new[]
  {
                 NameLabel.WidthAnchor.ConstraintEqualTo(130)
              //   imageView.HeightAnchor.ConstraintEqualTo(30)
      };
            NSLayoutConstraint.ActivateConstraints(contraints);

            CheckImage = new UIImageView
            {

            };


            //  CheckImage.SetContentHuggingPriority(0, UILayoutConstraintAxis.Horizontal);
            CheckImage.TranslatesAutoresizingMaskIntoConstraints = false;

            var contraints3 = new[]
  {
                 CheckImage.WidthAnchor.ConstraintEqualTo(24),
         CheckImage.HeightAnchor.ConstraintEqualTo(24)
      };
            NSLayoutConstraint.ActivateConstraints(contraints3);

            _pm.TranslatesAutoresizingMaskIntoConstraints = false;

            var margins2 = ContentView.SafeAreaLayoutGuide;
            var contraints2 = new[]
 {
                  _pm.LeadingAnchor.ConstraintEqualTo(margins2.LeadingAnchor),
                    _pm.TrailingAnchor.ConstraintEqualTo(margins2.TrailingAnchor),
                    _pm.BottomAnchor.ConstraintEqualTo(margins2.BottomAnchor),
                          _pm.TopAnchor.ConstraintEqualTo(margins2.TopAnchor)
       };

            NSLayoutConstraint.ActivateConstraints(contraints2);

            _pm.AddArrangedSubview(NameLabel);
            _pm.AddArrangedSubview(CheckImage);
        }

        protected override void BindItem(SampleItem selectionMenuItem)
        {
            if (NameLabel != null)
                NameLabel.Text = selectionMenuItem.Name;
        }

        protected override void OnItemSelected(bool isSelected)
        {
            if (CheckImage != null)
                CheckImage.Image = isSelected ? UIImage.FromBundle("ic_check") : null;// UIImage.CheckmarkImage : null;
        }
    }
    
}