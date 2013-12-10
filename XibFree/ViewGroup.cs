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
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreAnimation;
using System.Drawing;

namespace XibFree
{
	/// <summary>Base class for all views that can layout a set of subviews</summary>
	public abstract class ViewGroup : View
	{
		// Fields
		private readonly List<View> _subViews = new List<View>();

		protected ViewGroup()
		{
			LayoutParameters.Width = Dimension.FillParent;
			LayoutParameters.Height = Dimension.FillParent;
		}

		public override sealed UIView InnerView
		{
			get { return base.InnerView; }
			set
			{
				if (base.InnerView == value) return;
				base.InnerView = value;
				foreach (var subView in SubViews)
				{
					subView.InnerView.RemoveFromSuperview();
					base.InnerView.AddSubview(subView.InnerView);
				}
			}
		}

		/// <summary>Gets or sets the padding that should be applied around the subviews</summary>
		public UIEdgeInsets Padding { get; set; }

		public int Tag { get; set; }

		/// <summary>Gets or sets all the subviews of this view group</summary>
		public IEnumerable<View> SubViews
		{
			get { return _subViews; }
			set
			{
				// Check that none of the child subviews already have parents
				if (value.Any(c => c.Parent != null)) throw new InvalidOperationException("View is already a child of another ViewGroup");

				// Clear current subviews
				foreach (var subView in _subViews) RemoveSubView(subView);

				// Add new subviews
				foreach (var c in value) AddSubView(c);
			}
		}

		/// <summary>Insert a new subview at a specified position</summary>
		/// <param name="position">Zero-based index of where to insert the new subview.</param>
		/// <param name="view">The native subview to insert.</param>
		/// <param name="lp">Layout parameters for the subview.</param>
		public void InsertSubView(int position, UIView view, LayoutParameters lp)
		{
			InsertSubView(-1, new NativeView(view, lp));
		}

		/// <summary>Insert a new subview at the end of the subview collection</summary>
		/// <param name="view">The native subview to insert.</param>
		/// <param name="lp">Layout parameters for the subview.</param>
		public void AddSubView(UIView view, LayoutParameters lp)
		{
			InsertSubView(-1, new NativeView(view, lp));
		}

		/// <summary>Insert a new subview at the end of the subview collection</summary>
		/// <param name="view">The subview to add</param>
		public void AddSubView(View view)
		{
			InsertSubView(-1, view);
		}

		/// <summary>Remove a subview from the subview collection</summary>
		/// <param name="view">The subview to remove.</param>
		public void RemoveSubView(UIView view)
		{
			var subView = _subViews.FirstOrDefault(s => s.InnerView == view);
			if (subView != null) RemoveSubView(subView);
		}

		/// <summary>Insert a new subview at a specified position</summary>
		/// <param name="position">Zero-based index of where to insert the new subview.</param>
		/// <param name="view">The subview to add.</param>
		public void InsertSubView(int position, View view)
		{
			if (view.Parent != null) throw new InvalidOperationException("View is already a child of another ViewGroup");
			view.Parent = this;

			if (position < 0) position = _subViews.Count;
			InnerView.InsertSubview(view.InnerView, position);
			_subViews.Insert(position, view);
		}

		internal void ReplaceInnerView(View view, UIView newInnerView)
		{
			var index = _subViews.IndexOf(view);
			if (index == -1) return;

			InnerView.Subviews[index].RemoveFromSuperview();
			if (newInnerView != null) InnerView.InsertSubview(newInnerView, index);
		}

		/// <summary>Remove the specified subview</summary>
		/// <param name="view">The subview to remove</param>
		public void RemoveSubView(View view)
		{
			view.Parent = null;
			view.InnerView.RemoveFromSuperview();
			_subViews.Remove(view);
		}

		protected override void OnLayout(RectangleF newPosition, bool parentHidden)
		{
			if (!parentHidden && Visible)
			{
				InnerView.Frame = newPosition;
				InnerView.Hidden = false;
				return;
			}

			InnerView.Hidden = true;
			InnerView.Frame = newPosition;

			// Hide all subviews
			foreach (var v in SubViews) v.Layout(RectangleF.Empty, false);
		}

		internal override View LayoutViewWithTag(int tag)
		{
			if (Tag == tag) return this;

			return _subViews.Select(v => v.LayoutViewWithTag(tag)).FirstOrDefault(result => result != null);
		}

		internal override UIView UIViewWithTag(int tag)
		{
			return _subViews.Select(v => v.UIViewWithTag(tag)).FirstOrDefault(result => result != null);
		}

		public override View FindNativeView(UIView view)
		{
			if (InnerView == view) return this;
			return _subViews.Select(v => v.FindNativeView(view)).FirstOrDefault(result => result != null);
		}
	}
}

