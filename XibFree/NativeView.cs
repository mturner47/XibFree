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
	/// <summary>
	/// NativeView provides a wrapper around a native view control allowing it to partipate
	/// it the XibFree's layout logic
	/// </summary>
	public class NativeView : View
	{
		public NativeView(UIView view, LayoutParameters lp)
		{
			InnerView = view;
			LayoutParameters = lp;
		}

		public NativeView() { }

		/// <summary>Custom Measurer to override the calculated size for the passed UIView</summary>
		public Func<UIView, SizeF, SizeF> Measurer;

		public override sealed UIView InnerView
		{
			get { return base.InnerView; }
			set { base.InnerView = value; }
		}

		/// <summary>
		/// Sets a an action to be immediately called.  Provided to allowing execution of code inline
		/// with the layout of the view hierarchy.  See examples.
		/// </summary>
		public Action<NativeView> Init
		{
			set { value(this); }
		}

		/// <summary>Get the hosted view, casting to the specified type</summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T As<T>() where T : UIView
		{
			return InnerView as T;
		}

		public override View FindNativeView(UIView v)
		{
			return (InnerView == v) ? this : null;
		}

		/// <summary>Overridden to set the position of the native view</summary>
		/// <param name="newPosition">New position.</param>
		/// <param name="parentHidden">If Parent is Hidden </param>
		protected override void OnLayout(RectangleF newPosition, bool parentHidden)
		{
			if (InnerView == null) return;

			// Simple, just reposition the view!
			if (parentHidden || !Visible) InnerView.Hidden = true;
			InnerView.Frame = newPosition;
		}

		/// <summary>Overridden to provide measurement support for this native view</summary>
		/// <param name="parentWidth">Parent width.</param>
		/// <param name="parentHeight">Parent height.</param>
		protected override void OnMeasure(float? parentWidth, float? parentHeight)
		{
			// Resolve width for absolute and parent ratio
			var width = LayoutParameters.TryResolveWidth(this, parentWidth);
			var height = LayoutParameters.TryResolveHeight(this, parentHeight);

			// Do we need to measure our content?
			var sizeMeasured = SizeF.Empty;
			if (!width.HasValue || !height.HasValue)
			{
				var sizeToFitWidth = width ?? 0;
				var sizeToFitHeight = height ?? 0;
				var sizeToFit = new SizeF(sizeToFitWidth, sizeToFitHeight);
				if (Measurer != null) sizeMeasured = Measurer(InnerView, sizeToFit);
				else
				{
					sizeMeasured = InnerView.SizeThatFits(sizeToFit);
				}
			}

			// Set the measured size
			SetMeasuredSize(LayoutParameters.ResolveSize(width, height, sizeMeasured));
		}

		internal override UIView UIViewWithTag(int tag)
		{
			return (InnerView != null) ? InnerView.ViewWithTag(tag) : null;
		}

		internal override View LayoutViewWithTag(int tag)
		{
			return (InnerView != null && InnerView.Tag == tag) ? this : null;
		}
	}
}

