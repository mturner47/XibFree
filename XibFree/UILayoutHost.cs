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
using System;

namespace XibFree
{
	/// <summary>UILayoutHost is the native UIView that hosts that XibFree layout</summary>
	public sealed class UILayoutHost : UIView
	{
		private ViewGroup _layout;

		/// <summary>Initializes a new instance of the <see cref="XibFree.UILayoutHost"/> class.</summary>
		/// <param name="layout">Root of the view hierarchy to be hosted by this layout host</param>
		/// <param name="frame">Frame for the UIView</param>
		public UILayoutHost(ViewGroup layout, RectangleF frame) : base(frame)
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			Layout = layout;
		}

		public UILayoutHost(ViewGroup layout = null) : this(layout, RectangleF.Empty) { }

		/// <summary>The ViewGroup declaring the layout to hosted</summary>
		public ViewGroup Layout
		{
			get { return _layout; }
			set
			{
				if (_layout == value) return;

				if (_layout != null) 
				{
					_layout.Host = null;
					_layout.InnerView.RemoveFromSuperview();
				}

				_layout = value;

				if (_layout != null) 
				{
					_layout.Host = this;
					AddSubview(_layout.InnerView);
				}
			}
		}

		/// <summary>Finds the NativeView associated with a UIView</summary>
		/// <returns>The native view.</returns>
		/// <param name="view">View.</param>
		public View FindNativeView(UIView view)
		{
			return _layout.FindNativeView(view);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			if (_layout == null) return new SizeF(0, 0);

			// Measure the layout
			_layout.Measure(size.Width, size.Height);
			return _layout.GetMeasuredSize();
		}

		/// <Docs>Lays out subviews.</Docs>
		/// <summary>Called by iOS to update the layout of this view</summary>
		public override void LayoutSubviews()
		{
			if (_layout == null) return;
			// Remeasure
			_layout.Measure(Bounds.Width, Bounds.Height);

			// Position it
			_layout.Layout(_layout.MeasuredFrame(Bounds), false);
		}

		/// <summary>Provide the hosting view</summary>
		public UIView GetUIView()
		{
			return this;
		}
	}
}

