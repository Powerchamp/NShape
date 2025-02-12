﻿/******************************************************************************
  Copyright 2009-2022 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;


namespace Dataweb.NShape.Advanced {

	/// <ToBeCompleted></ToBeCompleted>
	public abstract class DiameterShapeBase : CaptionedShapeBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the top right control point.</summary>
			public const int TopRightControlPoint = 3;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 8;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 9;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			// Copy size if the source is a DiameterShape
			if (source is DiameterShapeBase)
				_internalDiameter = ((DiameterShapeBase)source).DiameterInternal;
			else {
				// If not, try to calculate the size a good as possible
				Rectangle srcBounds = Geometry.InvalidRectangle;
				if (source is PathBasedPlanarShape) {
					PathBasedPlanarShape src = (PathBasedPlanarShape)source;
					// Calculate the bounds of the (unrotated) resize handles because with 
					// GetBoundingRectangle(), we receive the bounds including the children's bounds
					List<Point> pointBuffer = new List<Point>();
					int centerX = src.X; int centerY = src.Y;
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(-src.Angle);
					foreach (ControlPointId id in source.GetControlPointIds(ControlPointCapabilities.Resize))
						pointBuffer.Add(Geometry.RotatePoint(centerX, centerY, angleDeg, source.GetControlPointPosition(id)));
					Geometry.CalcBoundingRectangle(pointBuffer, out srcBounds);
				} else {
					// Generic approach: try to fit into the bounding rectangle
					srcBounds = source.GetBoundingRectangle(true);
				}
				//
				// Calculate new size
				if (Geometry.IsValid(srcBounds)) {
					float scale = Geometry.CalcScaleFactor(DiameterInternal, DiameterInternal, srcBounds.Width, srcBounds.Height);
					DiameterInternal = (int)Math.Round(DiameterInternal * scale);
				}
			}
		}


		#region IPersistable Members


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.DiameterShapeBase" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in CaptionedShapeBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Diameter", typeof(Int32));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			_internalDiameter = reader.ReadInt32();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(_internalDiameter);
		}

		#endregion


		#region [Public] Methods

		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) > 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) > 0
								&& IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.MiddleCenterControlPoint:
				case ControlPointId.Reference:
				    return ((controlPointCapability & ControlPointCapabilities.Rotate) > 0
				            || (controlPointCapability & ControlPointCapabilities.Reference) > 0)
				            || ((controlPointCapability & ControlPointCapabilities.Connect) > 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			if (relativePosition == RelativePosition.Empty) throw new ArgumentOutOfRangeException(nameof(relativePosition));
			// The RelativePosition of a RectangleBased shape is:
			// A = Tenths of percent of Width
			// B = Tenths of percent of Height
			Point result = Point.Empty;
			result.X = (int)Math.Round((X - DiameterInternal / 2f) + (DiameterInternal * (relativePosition.A / 1000f)));
			result.Y = (int)Math.Round((Y - DiameterInternal / 2f) + (DiameterInternal * (relativePosition.B / 1000f)));
			result = Geometry.RotatePoint(Center, Geometry.TenthsOfDegreeToDegrees(Angle), result);
			return result;
		}


		/// <override></override>
		public override RelativePosition CalculateRelativePosition(int x, int y) {
			if (!Geometry.IsValid(x, y)) throw new ArgumentOutOfRangeException("x / y");
			// The RelativePosition of a RectangleBased shape is:
			// A = Tenths of percent of Width
			// B = Tenths of percent of Height
			RelativePosition result = RelativePosition.Empty;
			if (Angle != 0) {
				float ptX = x;
				float ptY = y;
				Geometry.RotatePoint(X, Y, Geometry.TenthsOfDegreeToDegrees(-Angle), ref x, ref y);
			}
			if (DiameterInternal != 0) {
				result.A = (int)Math.Round((x - (X - DiameterInternal / 2f)) / (this.DiameterInternal / 1000f));
				result.B = (int)Math.Round((y - (Y - DiameterInternal / 2f)) / (this.DiameterInternal / 1000f));
			} else {
				result.A = x - X;
				result.B = y - Y;
			}
			return result;
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			// Calculate bounds (including children)
			Rectangle bounds = GetBoundingRectangle(true);
			// Calculate the shape's offset relative to the bounds
			float offsetX = (X - DiameterInternal / 2f) - bounds.X;
			float offsetY = (Y - DiameterInternal / 2f) - bounds.Y;
			// Calculate the scaling factor and the new position
			float scale = Geometry.CalcScaleFactor(bounds.Width, bounds.Height, width, height);
			float dstX = x + (width / 2f) + (offsetX * scale);
			float dstY = y + (height / 2f) + (offsetY * scale);
			// Move to new position and apply scaling
			MoveTo((int)Math.Round(dstX), (int)Math.Round(dstY));
			DiameterInternal = (int)Math.Floor(DiameterInternal * scale);
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException(nameof(graphics));
			UpdateDrawCache();
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
			base.Draw(graphics);
		}

		#endregion


		#region [Protected] Properties

		/// <override></override>
		[Browsable(false)]
		protected internal override int ControlPointCount {
			get { return 9; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected int DiameterInternal {
			get { return _internalDiameter; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
				Invalidate();
				if (Owner != null) Owner.NotifyChildResizing(this);
				int delta = value - _internalDiameter;

				_internalDiameter = value;
				InvalidateDrawCache();

				if (ChildrenCollection != null) ChildrenCollection.NotifyParentSized(delta, delta);
				if (Owner != null) Owner.NotifyChildResized(this);
				ControlPointsHaveMoved();
				Invalidate();
			}
		}


		/// <override></override>
		protected override int DivFactorX { get { return 2; } }


		/// <override></override>
		protected override int DivFactorY { get { return 2; } }

		#endregion


		#region [Protected] Methods

		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			_internalDiameter = 40;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal DiameterShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal DiameterShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int size = DiameterInternal;
			int hSize, vSize;
			// Diameter shapes always have to be resized with "MaintainAspect"!
			modifiers |= ResizeModifiers.MaintainAspect;
			switch (pointId) {
				// Top Left
				case ControlPointIds.TopLeftControlPoint:
					result = (transformedDeltaX == transformedDeltaY);
					if (!Geometry.MoveRectangleTopLeft(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				// Top Center
				case ControlPointIds.TopCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleTop(size, size, 0, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = vSize;
					break;
				// Top Right
				case ControlPointIds.TopRightControlPoint:
					result = (transformedDeltaX == -transformedDeltaY);
					if (!Geometry.MoveRectangleTopRight(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				// Middle left
				case ControlPointIds.MiddleLeftControlPoint:
					result = (transformedDeltaY == 0);
					if (!Geometry.MoveRectangleLeft(size, size, transformedDeltaX, 0, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = hSize;
					break;
				// Middle right
				case ControlPointIds.MiddleRightControlPoint:
					result = (transformedDeltaY == 0);
					if (!Geometry.MoveRectangleRight(size, size, transformedDeltaX, 0, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = hSize;
					break;
				// Bottom left
				case ControlPointIds.BottomLeftControlPoint:
					result = (-transformedDeltaX == transformedDeltaY);
					if (!Geometry.MoveRectangleBottomLeft(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				// Bottom Center
				case ControlPointIds.BottomCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleBottom(size, size, 0, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = vSize;
					break;
				// Bottom right
				case ControlPointIds.BottomRightControlPoint:
					result = (transformedDeltaX == transformedDeltaY);
					if (!Geometry.MoveRectangleBottomRight(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
			}
			if (_internalDiameter != size || dx != 0 || dy != 0) {
				// Set field in order to avoid the shape being inserted into the owner's shape map
				_internalDiameter = size;
				MoveByCore(dx, dy);
				ControlPointsHaveMoved();
			}

			return result;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
			captionBounds = Rectangle.Empty;
			captionBounds.X = captionBounds.Y = (int)Math.Round(-DiameterInternal / 2f);
			captionBounds.Width = captionBounds.Height = DiameterInternal;
		}


		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdDiameter:
					DiameterInternal = propertyMapping.GetInteger();
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}

		#endregion


		#region Fields

		// PropertyId constant
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdDiameter = 7;

		private int _internalDiameter = 0;

		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public abstract class SquareBase : DiameterShapeBase {

		/// <ToBeCompleted></ToBeCompleted>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_SquareBase_Size")]
		[LocalizedDescription("PropDesc_SquareBase_Size")]
		[PropertyMappingId(PropertyIdDiameter)]
		[RequiredPermission(Permission.Layout)]
		public int Size {
			get { return base.DiameterInternal; }
			set { base.DiameterInternal = value; }
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (Size >= 0) {
				result.X = X - (int)Math.Round(Size / 2f);
				result.Y = Y - (int)Math.Round(Size / 2f);
				result.Width = result.Height = Size;
				if (Angle % 900 != 0) {
					Point tl, tr, bl, br;
					Geometry.RotateRectangle(result, Center, Geometry.TenthsOfDegreeToDegrees(Angle), out tl, out tr, out br, out bl);
					Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
				}
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal SquareBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal SquareBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Size / 2f);
			int top = (int)Math.Round(-Size / 2f);
			int right = left + Size;
			int bottom = top + Size;

			// top row (left to right)			
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;

			// middle row (left to right)
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;

			// bottom row (left to right)
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;

			// rotate handle
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.RectangleContainsPoint(X - DiameterInternal / 2, Y - DiameterInternal / 2, DiameterInternal, DiameterInternal, Geometry.TenthsOfDegreeToDegrees(Angle), x, y, true);
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;

			if (Angle % 900 == 0) {
				Rectangle bounds = Rectangle.Empty;
				bounds.X = X - (Size / 2);
				bounds.Y = Y - (Size / 2);
				bounds.Width = bounds.Height = Size;
				return Geometry.RectangleIntersectsWithRectangle(rectangle, bounds);
			} else {
				if (_rotatedBounds.Length != 4)
					Array.Resize<PointF>(ref _rotatedBounds, 4);
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				float ptX, ptY;
				float halfSize = Size / 2f;
				ptX = X - halfSize;		// left
				ptY = Y - halfSize;	// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				_rotatedBounds[0].X = ptX;
				_rotatedBounds[0].Y = ptY;

				ptX = X + halfSize;		// right
				ptY = Y - halfSize;		// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				_rotatedBounds[1].X = ptX;
				_rotatedBounds[1].Y = ptY;

				ptX = X + halfSize;		// right
				ptY = Y + halfSize;	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				_rotatedBounds[2].X = ptX;
				_rotatedBounds[2].Y = ptY;

				ptX = X - halfSize;		// left
				ptY = Y + halfSize;	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				_rotatedBounds[3].X = ptX;
				_rotatedBounds[3].Y = ptY;

				return Geometry.PolygonIntersectsWithRectangle(_rotatedBounds, rectangle);
			}
		}


		/// <override></override>
		protected internal override int ControlPointCount {
			get { return 9; }
		}


		#region Fields

		private PointF[] _rotatedBounds = new PointF[4];
		
		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public abstract class CircleBase : DiameterShapeBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the top right control point.</summary>
			public const int TopRightControlPoint = 3;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 8;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 9;
			/// <summary>ControlPointId of the top left connection point.</summary>
			public const int TopLeftConnectionPoint = 10;
			/// <summary>ControlPointId of the top right connection point.</summary>
			public const int TopRightConnectionPoint = 11;
			/// <summary>ControlPointId of the bottom left connection point.</summary>
			public const int BottomLeftConnectionPoint = 12;
			/// <summary>ControlPointId of the bottom right connection point.</summary>
			public const int BottomRightConnectionPoint = 13;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_CircleBase_Diameter")]
		[LocalizedDescription("PropDesc_CircleBase_Diameter")]
		[PropertyMappingId(PropertyIdDiameter)]
		[RequiredPermission(Permission.Layout)]
		public int Diameter {
			get { return base.DiameterInternal; }
			set { base.DiameterInternal = value; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
				case ControlPointIds.TopLeftConnectionPoint:
				case ControlPointIds.TopRightConnectionPoint:
				case ControlPointIds.BottomLeftConnectionPoint:
				case ControlPointIds.BottomRightConnectionPoint:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			Point p = Geometry.IntersectCircleWithLine(X, Y, (int)Math.Round(Diameter / 2f), startX, startY, X, Y, false);
			return Geometry.IsValid(p) ? p : Center;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal CircleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal CircleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (tight) {
				if (DiameterInternal >= 0) {
					// No need to rotate the tight bounding rectangle of a circle
					result.X = X - (int)Math.Round(Diameter / 2f);
					result.Y = Y - (int)Math.Round(Diameter / 2f);
					result.Width = result.Height = Diameter;
				}
			} else result = base.CalculateBoundingRectangle(tight);
			if (Geometry.IsValid(result))
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
			return result;
		}


		/// <override></override>
		protected internal override int ControlPointCount { 
			get { return 13; } 
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.CircleContainsPoint(X, Y, Diameter / 2f, x, y, 0);
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle r = Rectangle.Empty;
			r.X = x;
			r.Y = y;
			r.Width = width;
			r.Height = height;
			return Geometry.CircleIntersectsWithRectangle(r, Center, Diameter / 2f);
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Diameter / 2f);
			int top = (int)Math.Round(-Diameter / 2f);
			int right = left + Diameter;
			int bottom = top + Diameter;

			// Top left
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			// Top
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			// Top right
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;
			// Left
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			// Right
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;
			// Bottom left
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			// Bottom
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			// Bottom right
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;
			// Center
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;

			if (ControlPointCount > 9) {
				double angle = Geometry.DegreesToRadians(45);
				int dx = (int)Math.Round((Diameter / 2f) - ((Diameter / 2f) * Math.Cos(angle)));
				int dy = (int)Math.Round((Diameter / 2f) - ((Diameter / 2f) * Math.Sin(angle)));
				// Top left
				ControlPoints[9].X = left + dx;
				ControlPoints[9].Y = top + dy;
				// Top right
				ControlPoints[10].X = right - dx;
				ControlPoints[10].Y = top + dy;
				// Bottom left
				ControlPoints[11].X = left + dx;
				ControlPoints[11].Y = bottom - dy;
				// Bottom right
				ControlPoints[12].X = right - dx;
				ControlPoints[12].Y = bottom - dy;
			}
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
			captionBounds = Rectangle.Empty;
			captionBounds.X = (int)Math.Round((-Diameter / 2f) + (Diameter / 8f));
			captionBounds.Y = (int)Math.Round((-Diameter / 2f) + (Diameter / 8f));
			captionBounds.Width = (int)Math.Round(Diameter - (Diameter / 4f));
			captionBounds.Height = (int)Math.Round(Diameter - (Diameter / 4f));
		}

	}

}
