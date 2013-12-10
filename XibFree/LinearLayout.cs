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
using System.Drawing;
using System.Linq;

namespace XibFree
{
	/// <summary>Specifies vertical or horizontal orientation.</summary>
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

	public sealed class LinearLayout : ViewGroup
	{
		// Fields
		private readonly Orientation _orientation;

		/// <summary>Initializes a new instance of the <see cref="XibFree.LinearLayout"/> class.</summary>
		/// <param name="orientation">Specifies the horizontal or vertical orientation of this layout.</param>
		public LinearLayout(Orientation orientation)
		{
			_orientation = orientation;
		}

		/// <summary>Explicitly specify the total weight of the sub views that have size of FillParent</summary>
		/// <description>If not specified, the total weight is calculated by adding the LayoutParameters.Weight of
		/// each subview that has a size of FillParent.</description>
		public int TotalWeight { get; set; }

		/// <summary>Gets or sets the spacing between stacked subviews</summary>
		public float Spacing { get; set; }

		/// <summary>Sets an init Action that allows performing actions on the View</summary>
		public Action<LinearLayout> Init
		{
			set { value(this); }
		}

		// Overridden to provide layout measurement
		protected override void OnMeasure(float? parentWidth, float? parentHeight)
		{
			if (_orientation == Orientation.Vertical) MeasureVertical(parentWidth, parentHeight);
			else MeasureHorizontal(parentWidth, parentHeight);
		}

		// Do measurement when in vertical orientation
		private void MeasureVertical(float? parentWidth, float? parentHeight)
		{
			// Work out our width
			var width = LayoutParameters.TryResolveWidth(this, parentWidth);
			var height = LayoutParameters.TryResolveHeight(this, parentHeight);

			// Allow room for padding
			if (width.HasValue) width -= Padding.TotalWidth();

			// Work out the total fixed size
			var totalFixedSize = 0f;
			var visibleViewCount = 0;
			foreach (var v in SubViews.Where(x => !x.Gone))
			{
				if (v.LayoutParameters.Height.Unit != Units.ParentRatio)
				{
					// Lay it out
					v.Measure(AdjustLayoutWidth(width, v), null);
					totalFixedSize += v.GetMeasuredSize().Height;
				}
				
				// Include margins
				totalFixedSize += v.LayoutParameters.Margins.TotalHeight();
				visibleViewCount++;
			}
			
			
			// Also need to include our own padding
			totalFixedSize += Padding.TotalHeight();

			// And spacing between controls
			if (visibleViewCount > 1) totalFixedSize += (visibleViewCount - 1) * Spacing;

			float totalVariableSize = 0;
			if (LayoutParameters.Height.Unit == Units.ContentRatio || !height.HasValue)
			{
				// This is a weird case: we have a height of wrap content, but child items that want to fill parent too.
				// Temporarily switch those items to wrap content and use their natural size
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Height.Unit == Units.ParentRatio))
				{
					v.Measure(AdjustLayoutWidth(width, v), null);
					totalVariableSize += v.GetMeasuredSize().Height;
				}
			}
			else
			{
				// If we've had an explicit weight passed to us, use it, otherwise sum up the weights of the relevant subviews
				var totalWeight = TotalWeight != 0 ? TotalWeight : SubViews.
				                  Where(v => !v.Gone && v.LayoutParameters.Height.Unit == Units.ParentRatio).
				                  Sum(v => v.LayoutParameters.Weight);

				// Work out how much room we've got to share around
				var room = height - totalFixedSize;

				// Layout the fill parent items
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Height.Unit == Units.ParentRatio))
				{
					// Work out size
					if (room < 0) room = 0;
					var size = (totalWeight == 0) ? room : room * (float)v.LayoutParameters.Weight / (float)totalWeight;

					// Measure it
					v.Measure(AdjustLayoutWidth(width, v), size);

					// Update total size
					totalVariableSize += v.GetMeasuredSize().Height;

					// Adjust the weighting calculation in case the view didn't accept our measurement
					room -= v.GetMeasuredSize().Height;
					totalWeight -= v.LayoutParameters.Weight;
				}
			}

			var sizeMeasured = SizeF.Empty;

			if (!width.HasValue)
			{
				// Work out the maximum width of all children that aren't fill parent
				sizeMeasured.Width = 0;
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Width.Unit != Units.ParentRatio))
				{
					var totalChildWidth = v.GetMeasuredSize().Width + v.LayoutParameters.Margins.TotalWidth();
					if (totalChildWidth > sizeMeasured.Width) sizeMeasured.Width = totalChildWidth;
				}
				
				// Set the width of all children that are fill parent
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Width.Unit == Units.ParentRatio))
				{
					v.Measure(sizeMeasured.Width, v.GetMeasuredSize().Height);
				}

				sizeMeasured.Width += Padding.TotalWidth();
			}
			else
			{
				width += Padding.TotalWidth();
			}

			if (!height.HasValue)
			{
				height = totalFixedSize + totalVariableSize;
			}

			// And finally, set our measure dimensions
			SetMeasuredSize(LayoutParameters.ResolveSize(width, height, sizeMeasured));
		}

		// Do measurement when in horizontal orientation
		private void MeasureHorizontal(float? parentWidth, float? parentHeight)
		{
			// Work out our height
			var layoutWidth = LayoutParameters.TryResolveWidth(this, parentWidth);
			var layoutHeight = LayoutParameters.TryResolveHeight(this, parentHeight);

			// Allow room for padding
			if (layoutHeight.HasValue) layoutHeight -= Padding.TotalHeight();

			// Work out the total fixed size
			var totalFixedSize = 0f;
			var visibleViewCount = 0;
			foreach (var v in SubViews.Where(x => !x.Gone))
			{
				if (v.LayoutParameters.Width.Unit != Units.ParentRatio)
				{
					// Lay it out
					v.Measure(null, AdjustLayoutHeight(layoutHeight, v));
					totalFixedSize += v.GetMeasuredSize().Width;
				}
				
				// Include margins
				totalFixedSize += v.LayoutParameters.Margins.TotalWidth();
				visibleViewCount++;
			}

			// Also need to include our own padding
			totalFixedSize += Padding.TotalWidth();

			// And spacing between controls
			if (visibleViewCount > 1) totalFixedSize += (visibleViewCount - 1) * Spacing;
			
			float totalVariableSize = 0;
			if (LayoutParameters.Width.Unit == Units.ContentRatio || !layoutWidth.HasValue)
			{
				// This is a weird case: we have a width of wrap content, but child items that want to fill parent too.
				// Temporarily switch those items to wrap content and use their natural size
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Width.Unit == Units.ParentRatio))
				{
					v.Measure(null, AdjustLayoutHeight(layoutHeight, v));
					totalVariableSize += v.GetMeasuredSize().Width;
				}
			}
			else
			{
				// If we've had an explicit weight passed to us, use it, otherwise sum up the weights of the relevant subviews
				var totalWeight = TotalWeight != 0 ? TotalWeight : SubViews.
				                  Where(v => !v.Gone && v.LayoutParameters.Width.Unit == Units.ParentRatio).
				                  Sum(v => v.LayoutParameters.Weight);

				// Work out how much room we've got to share around
				var room = layoutWidth - totalFixedSize;

				// Layout the fill parent items
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Width.Unit == Units.ParentRatio))
				{
					// Work out size
					if (room < 0) room = 0;
					var size = (totalWeight == 0) ? room : room * v.LayoutParameters.Weight / totalWeight;

					// Measure it
					v.Measure(size, AdjustLayoutHeight(layoutHeight, v));

					// Update total size
					totalVariableSize += v.GetMeasuredSize().Width;

					// Adjust the weighting calculation in case the view didn't accept our measurement
					room -= v.GetMeasuredSize().Width;
					totalWeight -= v.LayoutParameters.Weight;
				}
			}

			var sizeMeasured = SizeF.Empty;

			if (!layoutHeight.HasValue)
			{
				// Work out the maximum height of all children that aren't fill parent
				sizeMeasured.Height = 0;
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Height.Unit != Units.ParentRatio))
				{
					var totalChildHeight = v.GetMeasuredSize().Height + v.LayoutParameters.Margins.TotalHeight();
					if (totalChildHeight > sizeMeasured.Height) sizeMeasured.Height = totalChildHeight;
				}
				
				// Set the height of all children that are fill parent
				foreach (var v in SubViews.Where(x => !x.Gone && x.LayoutParameters.Height.Unit == Units.ParentRatio))
				{
					v.Measure(v.GetMeasuredSize().Width, sizeMeasured.Height);
				}

				sizeMeasured.Height += Padding.TotalHeight();
			}
			else
			{
				layoutHeight += Padding.TotalHeight();
			}

			

			if (!layoutWidth.HasValue)
			{
				layoutWidth = totalFixedSize + totalVariableSize;
			}
			
			// And finally, set our measure dimensions
			SetMeasuredSize(LayoutParameters.ResolveSize(layoutWidth, layoutHeight, sizeMeasured));
		}

		// Overridden to layout the subviews
		protected override void OnLayout(RectangleF newPosition, bool parentHidden)
		{
			base.OnLayout(newPosition, parentHidden);

			if (parentHidden || !Visible) return;

			var basePosition = new RectangleF(0, 0, newPosition.Width, newPosition.Height);
			if (_orientation == Orientation.Vertical) LayoutVertical(basePosition);
			else LayoutHorizontal(basePosition);
		}

		// Do subview layout when in vertical orientation
		private void LayoutVertical(RectangleF newPosition)
		{
			var y = 0f;
			switch (LayoutParameters.Gravity & Gravity.VerticalMask)
			{
				case Gravity.Top:
					y = newPosition.Top + Padding.Top;
					break;
				case Gravity.Bottom:
					y = newPosition.Bottom - GetTotalMeasuredHeight() + Padding.Top;
					break;
				case Gravity.CenterVertical:
					y = (newPosition.Top + newPosition.Bottom)/2 - GetTotalMeasuredHeight()/2 + Padding.Top;
					break;
			}

			var first = true;

			foreach (var v in SubViews)
			{
				// Hide hidden views
				if (v.Gone)
				{
					v.Layout(RectangleF.Empty, false);
					continue;
				}

				if (!first) y += Spacing;
				else first = false;

				y += v.LayoutParameters.Margins.Top;

				var size = v.GetMeasuredSize();

				// Work out horizontal gravity for this control
				var g = v.LayoutParameters.Gravity & Gravity.HorizontalMask;

				var x = 0f;
				switch (g)
				{
					case Gravity.Left:
						x = newPosition.Left + Padding.Left + v.LayoutParameters.Margins.Left;
						break;

					case Gravity.Right:
						x = newPosition.Right - Padding.Right - v.LayoutParameters.Margins.Right - size.Width;
						break;

					case Gravity.CenterHorizontal:
						x = (newPosition.Left + newPosition.Right)/2 - (size.Width + v.LayoutParameters.Margins.TotalWidth())/2;
						break;
				}

				
				v.Layout(new RectangleF(x, y, size.Width, size.Height), false);

				y += size.Height + v.LayoutParameters.Margins.Bottom;
			}
		}

		// Do subview layout when in horizontal orientation
		private void LayoutHorizontal(RectangleF newPosition)
		{
			var x = 0f;
			switch (LayoutParameters.Gravity & Gravity.HorizontalMask)
			{
				case Gravity.Left:
					x = newPosition.Left + Padding.Left;
					break;
					
				case Gravity.Right:
					x = newPosition.Right - GetTotalMeasuredWidth() + Padding.Left;
					break;
					
				case Gravity.CenterHorizontal:
					x = (newPosition.Left + newPosition.Right)/2 - GetTotalMeasuredWidth()/2 + Padding.Left;
					break;
					
			}

			var first = true;

			foreach (var v in SubViews)
			{
				// Hide hidden views
				if (v.Gone)
				{
					v.Layout(RectangleF.Empty, false);
					continue;
				}

				if (!first) x += Spacing;
				else first = false;
				
				x += v.LayoutParameters.Margins.Left;
				
				var size = v.GetMeasuredSize();
				
				// Work out vertical gravity for this control
				var g = v.LayoutParameters.Gravity & Gravity.VerticalMask;
				
				var y = 0f;
				switch (g)
				{
					case Gravity.Top:
						y = newPosition.Top + Padding.Top + v.LayoutParameters.Margins.Top;
						break;
						
					case Gravity.Bottom:
						y = newPosition.Bottom - Padding.Top - v.LayoutParameters.Margins.Bottom - size.Height;
						break;
						
					case Gravity.CenterVertical:
						y = (newPosition.Top + newPosition.Bottom)/2
							- (size.Height + v.LayoutParameters.Margins.TotalHeight())/2;
						break;
				}
				
				
				v.Layout(new RectangleF(x, y, size.Width, size.Height), false);
				
				x += size.Width + v.LayoutParameters.Margins.Right;
			}
		}

		private float GetTotalSpacing()
		{
			var visibleViews = SubViews.Count(x => !x.Gone);
			return visibleViews > 1 ? (visibleViews - 1)*Spacing : 0;
		}
		
		// Helper to get the total measured height of all subviews, including all padding and margins
		private float GetTotalMeasuredHeight()
		{
			var totalHeightOfSubViews = SubViews
				.Where(x => !x.Gone)
				.Sum(x => x.GetMeasuredSize().Height + x.LayoutParameters.Margins.TotalHeight());
			return Padding.TotalWidth() + GetTotalSpacing() + totalHeightOfSubViews;
		}
		
		// Helper to get the total measured width of all subviews, including all padding and margins
		private float GetTotalMeasuredWidth()
		{
			var totalWidthOfSubViews = SubViews
				.Where(x => !x.Gone)
				.Sum(x => x.GetMeasuredSize().Width + x.LayoutParameters.Margins.TotalWidth());
			return Padding.TotalHeight() + GetTotalSpacing() + totalWidthOfSubViews;
		}

		// Helper to adjust the parent width passed down to subviews during measurement
		private static float? AdjustLayoutWidth(float? width, View c)
		{
			return width.HasValue ? width - c.LayoutParameters.Margins.TotalWidth() : width;
		}

		// Helper to adjust the parent height passed down to subviews during measurement
		private static float? AdjustLayoutHeight(float? height, View c)
		{
			return height.HasValue ? height - c.LayoutParameters.Margins.TotalHeight() : height;
		}
	}
}

