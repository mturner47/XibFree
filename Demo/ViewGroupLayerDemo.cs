using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

using XibFree;
using MonoTouch.CoreAnimation;

namespace Demo
{
	public sealed class ViewGroupLayerDemo : UIViewController
	{
		public ViewGroupLayerDemo()
		{
			Title = "ViewGroup Layers";

			// Custom initialization
		}

		public override void LoadView()
		{
			var layout = new LinearLayout(Orientation.Vertical)
			{
				SubViews = new View[] 
				{
					new NativeView
					{
						InnerView = new UIView
						{
							BackgroundColor = UIColor.Blue
						},
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.Absolute(50),
						},
					},
					new LinearLayout(Orientation.Vertical)
					{
						Init = linearLayout =>
						{
							var layer = new CAGradientLayer
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
							};
							linearLayout.InnerView.Layer.AddSublayer(layer);
						},
						SubViews = new View[]
						{
							new NativeView
							{
								InnerView = new UILabel(RectangleF.Empty)
								{
									Text="Hello World",
									Font = UIFont.SystemFontOfSize(24),
									BackgroundColor = UIColor.Clear,
								}
							},
							new NativeView
							{
								InnerView = new UILabel(RectangleF.Empty)
								{
									Text="Goodbye",
									Font = UIFont.SystemFontOfSize(24),
									BackgroundColor = UIColor.Clear,
								}
							}
						},
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.WrapContent,
							Margins = new UIEdgeInsets(20,20,20,20),
						},
					},
					new NativeView
					{
						InnerView = new UIView	{ BackgroundColor = UIColor.Blue },
						LayoutParameters = new LayoutParameters
						{
							Width = Dimension.FillParent,
							Height = Dimension.Absolute(50),
						},
					}
				},
			};

			// We've now defined our layout, to actually use it we simply create a UILayoutHost control and pass it the layout
			View = new UILayoutHost(layout);
			View.BackgroundColor=UIColor.Gray;
		}
	}
}
