using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;

using XibFree;

namespace Demo
{
	public sealed class RecalculateLayoutDemo : UIViewController
	{
		public RecalculateLayoutDemo()
		{
			Title = "Recalc Layout";

			// Custom initialization
		}

		public override void LoadView()
		{
			UILabel label;
			var layout = new LinearLayout(Orientation.Vertical)
			{
				SubViews = new View[] 
				{
					new NativeView
					{
						View = new UIView
						{
							BackgroundColor = UIColor.Blue
						},
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.Absolute(50)
						},
					},
					new LinearLayout(Orientation.Vertical)
					{
						Padding = new UIEdgeInsets(10,10,10,10),
						Layer = new CAGradientLayer
						{
							Colors = new[]
							{
								new MonoTouch.CoreGraphics.CGColor(0.9f, 0.9f, 0.9f, 1f),
								new MonoTouch.CoreGraphics.CGColor(0.7f, 0.7f, 0.7f, 1f)
							},
							Locations = new NSNumber[]
							{
								0.0f,
								1.0f
							},
							CornerRadius = 5,
						},
						SubViews = new View[]
						{
							new NativeView
							{
								View = new UILabel(RectangleF.Empty)
								{
									Text="Hello World",
									Font = UIFont.SystemFontOfSize(24),
									BackgroundColor = UIColor.Clear,
								}
							},
							new NativeView
							{
								View = label = new UILabel(RectangleF.Empty)
								{
									Text="Goodbye",
									Font = UIFont.SystemFontOfSize(24),
									BackgroundColor = UIColor.Clear,
									Lines = 0,
								},
								LayoutParameters = new LayoutParameters
								{
									Width = Dimension.FillParent,
									Height = Dimension.WrapContent
								}
							}
						},
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.WrapContent,
							Margins = new UIEdgeInsets(10,10,10,10),
						},
					},
					new NativeView
					{
						View = new UIView
						{
							BackgroundColor = UIColor.Blue
						},
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.Absolute(50),
						},
					},
					new NativeView
					{
						View = new UIButton(UIButtonType.RoundedRect),
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.WrapContent
						},
						Init = v =>
						{
							v.As<UIButton>().SetTitle("Change Text", UIControlState.Normal);
							v.As<UIButton>().TouchUpInside += (sender, e) => 
							{
								if (label.Text=="ShortString")
								{
									label.Text = "This is a long string that should wrap and cause the layout to change";
									label.GetNativeView().LayoutParameters.Margins = new UIEdgeInsets(10,10,10,10);
								}
								else
								{
									label.Text = "ShortString";
									label.GetNativeView().LayoutParameters.Margins = new UIEdgeInsets(20,20,20,20);
								}
								label.GetLayoutHost().SetNeedsLayout();
								//or: this.View.SetNeedsLayout();
							};
						}
					}
				},
			};

			// We've now defined our layout, to actually use it we simply create a UILayoutHost control and pass it the layout
			View = new UILayoutHost(layout);
			View.BackgroundColor = UIColor.Gray;
		}
	}
}
