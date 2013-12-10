//  XibFree - http://www.toptensoftware.com/xibfree/
//
//  Copyright 2013  Copyright © 2013 Topten Software. All Rights Reserved
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using MonoTouch.UIKit;
using System.Drawing;

namespace XibFree
{
	/// <summary>LayoutParameters declare how a view should be laid out by it's parent view group.</summary>
	public class LayoutParameters
	{
		private UIEdgeInsets _margins;

		/// <summary>Initializes a new instance of the <see cref="XibFree.LayoutParameters"/> class.</summary>
		public LayoutParameters() : this(Dimension.WrapContent, Dimension.WrapContent) {}

		/// <summary>Initializes a new instance of the <see cref="XibFree.LayoutParameters"/> class.</summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="weight">Weight.</param>
		public LayoutParameters(Dimension width, Dimension height, int weight = 1)
		{
			Width = width;
			Height = height;
			Margins = UIEdgeInsets.Zero;
			Weight = weight;
			Gravity = Gravity.Default;
		}

		/// <summary>Gets or sets the width dimension (value and unit of measurement)</summary>
		public Dimension Width { get; set; }

		/// <summary>Gets or sets the height dimension (value and unit of measurement)</summary>
		public Dimension Height { get; set; }

		/// <summary>Gets or sets the weight of a AutoSize.FillParent view relative to its sibling views</summary>
		public int Weight { get; set; }

		/// <summary>Gets or sets the whitepsace margins that should be left around a view</summary>
		public UIEdgeInsets Margins
		{
			get { return _margins; }
			set { _margins = value; }
		}

		/// <summary>Gets or sets the left margin.</summary>
		public float MarginLeft
		{
			get { return Margins.Left; }
			set { _margins.Left = value; }
		}
		
		/// <summary>Gets or sets the right margin.</summary>
		public float MarginRight
		{
			get { return Margins.Right; }
			set { _margins.Right = value; }
		}
		
		/// <summary>Gets or sets the top margin.</summary>
		public float MarginTop
		{
			get { return Margins.Top; }
			set { _margins.Top = value; }
		}
		
		/// <summary>Gets or sets the bottom margin.</summary>
		public float MarginBottom
		{
			get { return Margins.Bottom; }
			set { _margins.Bottom = value; }
		}
		
		/// <summary>Gets or sets the gravity for this view within it's parent subview</summary>
		public Gravity Gravity { get; set; }

		/// <summary>Gets or sets the visibility of this view</summary>
		public Visibility Visibility { get; set; }

		/// <summary>Gets or sets the minimum width.</summary>
		public float? MinWidth { get; set; }

		/// <summary>Gets or sets the maximum width.</summary>
		public float? MaxWidth { get; set; }

		/// <summary>Gets or sets the minimum height.</summary>
		public float? MinHeight { get; set; }

		/// <summary>Gets or sets the max height.</summary>
		public float? MaxHeight { get; set; }

		private static SizeF GetScreenSize()
		{
			var orientation = UIApplication.SharedApplication.StatusBarOrientation;
			if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
			{
				return UIScreen.MainScreen.Bounds.Size;
			}
			else
			{
				var temp = UIScreen.MainScreen.Bounds.Size;
				return new SizeF(temp.Height, temp.Width);
			}
		}

		internal SizeF GetHostSize(View view)
		{
			// Get the host
			var host = view.Host;
			if (host == null) return GetScreenSize();

			var hostView = host.GetUIView();

			// Use outer scroll view if present
			var parent = hostView.Superview;
			if (parent is UIScrollView) hostView = parent;

			// Return size
			return hostView.Bounds.Size;
		}

		private static float? TryResolve(Dimension dimension, float? parentSize)
		{
			float? d = null;
			switch (dimension.Unit)
			{
				case Units.Absolute:
					d = dimension.Value;
					break;
				case Units.ParentRatio:
					if (parentSize.HasValue) d = parentSize.Value * dimension.Value;
					break;
				default:
					break;
			}
			return d;
		}

		internal float? TryResolveWidth(View view, float? parentWidth)
		{
			if (Width.Unit == Units.HostRatio) return GetHostSize(view).Width * Width.Value;
			if (Width.Unit == Units.ScreenRatio) return GetScreenSize().Width * Width.Value;

			return TryResolve(Width, parentWidth);
		}
	
		internal float? TryResolveHeight(View view, float? parentHeight)
		{
			if (Height.Unit == Units.HostRatio) return GetHostSize(view).Height * Height.Value;
			if (Height.Unit == Units.ScreenRatio) return GetScreenSize().Height * Height.Value;

			return TryResolve(Height, parentHeight);
		}

		internal SizeF ResolveSize(float? width, float? height, SizeF sizeMeasured)
		{
			var size = SizeF.Empty;

			// Resolve measured size
			size.Width = width ?? sizeMeasured.Width;
			size.Height = height ?? sizeMeasured.Height;

			// Resolve Content Ratios
			if (Width.Unit == Units.ContentRatio) size.Width *= Width.Value;
			if (Height.Unit == Units.ContentRatio) size.Height *= Height.Value;

			// Finally, resolve aspect ratios
			if (Width.Unit == Units.AspectRatio) size.Width = size.Height * Width.Value;
			if (Height.Unit == Units.AspectRatio) size.Height = size.Width * Height.Value;

			return size;
		}
	}
}

