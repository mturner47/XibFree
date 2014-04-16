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

using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace XibFree
{
	/// <summary>Abstract base class for any item in the layout view hierarchy</summary>
	public abstract class View
	{
		private SizeF _measuredSize;
		private bool _measuredSizeValid;
		private ViewGroup _parent;
		private UIView _innerView;
		private UILayoutHost _host;

		protected View()
		{
			LayoutParameters = new LayoutParameters
			{
				Gravity = Gravity.TopLeft
			};
		}

		/// <summary>Gets or sets this view's parent view</summary>
		public ViewGroup Parent
		{
			get { return _parent; }
			internal set { _parent = value; }
		}

		private UIView DefaultInnerView()
		{
			return new UIView(RectangleF.Empty) 
			{
				BackgroundColor = UIColor.Clear,
				AutoresizingMask = UIViewAutoresizing.None,
			};
		}

		public virtual UIView InnerView
		{
			get
			{
				return _innerView ?? (_innerView = DefaultInnerView());
			}
			set
			{
				if (_innerView == value) return;

				var newInnerView = value ?? DefaultInnerView();

				if (_innerView != null)
				{
					if (Parent != null) Parent.ReplaceInnerView(this, newInnerView);
				}

				if (Parent == null && Host != null)
				{
					_innerView.RemoveFromSuperview();
					Host.GetUIView().AddSubview(newInnerView);
				}

				_innerView = newInnerView;

				// Turn off auto-resizing, we'll take care of that thanks
				_innerView.AutoresizingMask = UIViewAutoresizing.None;
			}
		}

		/// <summary>Gets or sets the layout parameters for this view</summary>
		public LayoutParameters LayoutParameters { get; set; }

		// Internal helper to walk the parent view hierachy and find the view that's hosting this view hierarchy
		public UILayoutHost Host
		{
			get
			{
				if (_host != null) return _host;
				return Parent != null ? Parent.Host : null;
			}
			internal set { _host = value; }
		}

		public bool Gone
		{
			get { return LayoutParameters.Visibility == Visibility.Gone; }
			set { LayoutParameters.Visibility = value ? Visibility.Gone : Visibility.Visible; }
		}

		public bool Visible
		{
			get { return LayoutParameters.Visibility == Visibility.Visible; }
			set { LayoutParameters.Visibility = value ? Visibility.Visible : Visibility.Invisible; }
		}

		/// <summary>Layout the subviews in this view using dimensions calculated during the last measure cycle</summary>
		/// <param name="newPosition">The new position of this view</param>
		/// <param name="parentHidden">Whether the parent is hidden</param>
		public void Layout(RectangleF newPosition, bool parentHidden)
		{
			OnLayout(newPosition, parentHidden);
		}

		/// <summary>Overridden by view groups to perform the actual layout process</summary>
		/// <param name="newPosition">New position.</param>
		/// <param name="parentHidden">Whether the parent is hidden</param>
		protected abstract void OnLayout(RectangleF newPosition, bool parentHidden);

		/// <summary>Measures the subviews of this view</summary>
		/// <param name="parentWidth">Available width of the parent view group</param>
		/// <param name="parentHeight">Available height of the parent view group</param>
		public void Measure(float? parentWidth, float? parentHeight)
		{
			_measuredSizeValid = false;
			OnMeasure(parentWidth, parentHeight);
			if (!_measuredSizeValid) throw new InvalidOperationException("onMeasure didn't set measurement before returning");
		}

		/// <summary>Overridden by view groups to perform the actual layout measurement</summary>
		/// <param name="parentWidth">Parent width.</param>
		/// <param name="parentHeight">Parent height.</param>
		protected abstract void OnMeasure(float? parentWidth, float? parentHeight);

		/// <summary>
		/// Called by derived implementations of onMeasure to store the measured dimensions
		/// of this view
		/// </summary>
		/// <param name="size">Size.</param>
		protected void SetMeasuredSize(SizeF size)
		{
			if (LayoutParameters.MinWidth.HasValue && size.Width < LayoutParameters.MinWidth.Value) size.Width = LayoutParameters.MinWidth.Value;
			if (LayoutParameters.MinHeight.HasValue && size.Height < LayoutParameters.MinHeight.Value) size.Height = LayoutParameters.MinHeight.Value;

			if (LayoutParameters.MaxWidth.HasValue && size.Width > LayoutParameters.MaxWidth.Value) size.Width = LayoutParameters.MaxWidth.Value;
			if (LayoutParameters.MaxHeight.HasValue && size.Height > LayoutParameters.MaxHeight.Value) size.Height = LayoutParameters.MaxHeight.Value;

			_measuredSize = size;
			_measuredSizeValid = true;
		}

		/// <summary>Retrieve the measured dimensions of this view</summary>
		/// <returns>The measured size.</returns>
		public SizeF GetMeasuredSize()
		{
			if (!_measuredSizeValid) throw new InvalidOperationException("Attempt to use measured size before measurement");
			return _measuredSize;
		}

		/// <summary>Overridden to locate a UIView</summary>
		/// <returns>The view with tag.</returns>
		/// <param name="tag">Tag.</param>
		internal abstract UIView UIViewWithTag(int tag);

		/// <summary>Overridden to locate a layout hierarchy view</summary>
		/// <returns>The view with tag.</returns>
		/// <param name="tag">Tag.</param>
		internal abstract View LayoutViewWithTag(int tag);

		/// <summary>Locates a view in either the layout or GUI hierarchy</summary>
		/// <returns>The view with tag.</returns>
		/// <param name="tag">Tag.</param>
		/// <typeparam name="T">The type of view to return</typeparam>
		public T ViewWithTag<T>(int tag)
		{
			if (typeof(UIView).IsAssignableFrom(typeof(T))) return (T)(object)UIViewWithTag(tag);
			else return (T)(object)LayoutViewWithTag(tag);
		}

		public abstract View FindNativeView(UIView v);

		public void RemoveFromSuperview()
		{
			if (Parent != null) Parent.RemoveSubView(this);
		}

		internal RectangleF MeasuredFrame(RectangleF startingFrame)
		{
			if (Gone) return RectangleF.Empty;
			var g = LayoutParameters.Gravity;
			var size = GetMeasuredSize();
			return startingFrame.ApplyInsets(LayoutParameters.Margins).ApplyGravity(size, g);
		}
	}
}

