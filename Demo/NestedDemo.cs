using System.Drawing;
using MonoTouch.UIKit;
using XibFree;

namespace Demo
{
	public sealed class NestedDemo : UIViewController
	{
		public NestedDemo()
		{
			Title = "Nested UILayoutViews";

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
					new NativeView
					{
						InnerView = new UILayoutHost
						{
							BackgroundColor = UIColor.Yellow,
							Layout = new LinearLayout(Orientation.Vertical)
							{
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
									Margins = new UIEdgeInsets(13, 13, 13, 13),
								},
							},
						},
						Init = v =>
						{
							v.InnerView.Layer.CornerRadius = 5;
							v.InnerView.Layer.MasksToBounds = true;
						}
					},
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
						}
					}
				},
			};

			// We've now defined our layout, to actually use it we simply create a UILayoutHost control and pass it the layout
			View = new UILayoutHost(layout);
			View.BackgroundColor=UIColor.Gray;
		}
	}
}
