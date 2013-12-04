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

namespace XibFree
{
	public enum Units
	{
		Absolute,			// Absolute pixel dimension
		ParentRatio,		// Ratio of parent size
		ContentRatio,		// Ratio of content size
		AspectRatio,		// Ratio of adjacent dimension size
		ScreenRatio,		// Ratio of the current screen size
		HostRatio,			// Ratio of the current UIViewHost window size
	}

	public class Dimension
	{
		public float Value { get; private set; }
		public Units Unit { get; private set; }

		public Dimension(float value, Units units = Units.Absolute)
		{
			Value = value;
			Unit = units;
		}

		public static Dimension FillParent
		{
			get { return ParentRatio(1.0f); }
		}

		public static Dimension WrapContent
		{
			get { return ContentRatio(1.0f); }
		}

		public static Dimension ParentRatio(float value)
		{
			return new Dimension(value, Units.ParentRatio);
		}

		public static Dimension AspectRatio(float value)
		{
			return new Dimension(value, Units.AspectRatio);
		}

		public static Dimension ContentRatio(float value)
		{
			return new Dimension(value, Units.ContentRatio);
		}

		public static Dimension Absolute(float value)
		{
			return new Dimension(value);
		}

		public float Ratio 
		{
			get  { return (Unit == Units.Absolute) ? 1 : Value; } 
		}
	}
}

