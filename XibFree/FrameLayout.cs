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
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace XibFree
{
	public sealed class FrameLayout : ViewGroup
	{
		public Action<FrameLayout> Init
		{
			set { value(this); }
		}

		protected override void OnMeasure(float? parentWidth, float? parentHeight)
		{
			var unresolved = new List<View>();

			var width = LayoutParameters.TryResolveWidth(this, parentWidth);
			var height = LayoutParameters.TryResolveHeight(this, parentHeight);

			// Remove padding
			if (width.HasValue) width -= Padding.TotalWidth();
			if (height.HasValue) height -= Padding.TotalHeight();

			// Measure all subviews where both dimensions can be resolved
			var haveResolvedSize = false;
			var maxWidth = 0f;
			var maxHeight = 0f;

			foreach (var v in SubViews.Where(x => !x.Gone))
			{
				// Try to resolve subview width
				float? subViewWidth = null;
				var lp = v.LayoutParameters;
				if (lp.Width.Unit == Units.ParentRatio)
				{
					if (!width.HasValue)
					{
						unresolved.Add(v);
						continue;
					}
					else
					{
						subViewWidth = width - lp.Margins.TotalWidth();
					}
				}

				// Try to resolve subview height
				float? subViewHeight = null;
				if (lp.Height.Unit == Units.ParentRatio)
				{
					if (!height.HasValue)
					{
						unresolved.Add(v);
						continue;
					}
					else
					{
						subViewHeight = height - lp.Margins.TotalHeight();
					}
				}

				// Measure it
				v.Measure(subViewWidth, subViewHeight);

				maxWidth = Math.Max(maxWidth, v.GetMeasuredSize().Width + v.LayoutParameters.Margins.TotalWidth());
				maxHeight = Math.Max(maxHeight, v.GetMeasuredSize().Height + v.LayoutParameters.Margins.TotalHeight());
				haveResolvedSize = true;
			}

			// Now resolve the unresolved subviews by either using the dimensions of the view
			// that were resolved, or none were, use their natural size
			foreach (var v in unresolved)
			{
				float? subViewWidth = null;
				if (v.LayoutParameters.Width.Unit == Units.ParentRatio && haveResolvedSize)
				{
					subViewWidth = maxWidth - v.LayoutParameters.Margins.TotalWidth();
				}

				float? subViewHeight = null;
				if (v.LayoutParameters.Height.Unit == Units.ParentRatio && haveResolvedSize)
				{
					subViewHeight = maxHeight - v.LayoutParameters.Margins.TotalHeight();
				}

				// Measure it
				v.Measure(subViewWidth, subViewHeight);
			}

			var sizeMeasured = SizeF.Empty;

			if (!width.HasValue)
			{
				sizeMeasured.Width = SubViews.Max(x => x.GetMeasuredSize().Width + x.LayoutParameters.Margins.TotalWidth()) + Padding.TotalWidth();
			}

			if (!height.HasValue)
			{
				sizeMeasured.Height = SubViews.Max(x=>x.GetMeasuredSize().Height + x.LayoutParameters.Margins.TotalHeight()) + Padding.TotalHeight();
			}

			// Done!
			SetMeasuredSize(LayoutParameters.ResolveSize(width, height, sizeMeasured));
		}

		protected override void OnLayout(RectangleF newPosition, bool parentHidden)
		{
			base.OnLayout(newPosition, parentHidden);

			// Make room for padding
			newPosition = newPosition.ApplyInsets(Padding);
			var startingRect = new RectangleF(0, 0, newPosition.Width, newPosition.Height);

			if (parentHidden || !Visible) return;

			// Position each view according to it's gravity
			foreach (var v in SubViews)
			{
				v.Layout(v.MeasuredFrame(startingRect), false);
			}
		}
	}
}

