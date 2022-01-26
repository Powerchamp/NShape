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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Displays a text within a shape.
	/// </summary>
	/// <status>reviewed</status>
	public class Caption {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Caption" />.
		/// </summary>
		public Caption() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Caption" />.
		/// </summary>
		public Caption(string text) {
			this.Text = text;
		}


		/// <summary>
		/// Specifies the text of the caption.
		/// </summary>
		public string Text {
			get { return _captionText; }
			set {
				if (string.IsNullOrEmpty(value)) {
					GdiHelpers.DisposeObject(ref _textPath);
					_captionTextSuffix = null;
				}
				_captionText = value;
				if (!string.IsNullOrEmpty(_captionText)) {
					_captionTextSuffix = _captionText.EndsWith("\n") ? "\n" : null;
					if (_textPath == null) _textPath = new GraphicsPath();
				}
			}
		}


		/// <summary>
		/// Gets or sets whether this caption text is visible.
		/// </summary>
		public bool IsVisible {
			get { return _isTextVisible; }
			set { _isTextVisible = value; }
		}


		/// <summary>
		/// Calculates the current text's area within the given caption bounds.
		/// </summary>
		public Rectangle CalculateTextBounds(Rectangle captionBounds, ICharacterStyle characterStyle, 
			IParagraphStyle paragraphStyle, IDisplayService displayService) {
			Rectangle textBounds = Rectangle.Empty;
			Debug.Assert(characterStyle != null);
			Debug.Assert(paragraphStyle != null);

			// measure the text size
			//if (float.IsNaN(dpiY)) dpiY = gfx.DpiY;
			if (displayService != null) {
				textBounds.Size = TextMeasurer.MeasureText(displayService.InfoGraphics, string.IsNullOrEmpty(_captionText)
						? "Ig" : _captionText, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);
			} else textBounds.Size = TextMeasurer.MeasureText(string.IsNullOrEmpty(_captionText)
				? "Ig" : _captionText, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);

			// clip text bounds if too large
			if (textBounds.Width > captionBounds.Width)
				textBounds.Width = captionBounds.Width;
			if (textBounds.Height > captionBounds.Height)
				textBounds.Height = captionBounds.Height;

			// set horizontal alignment
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					textBounds.X = captionBounds.X;
					break;
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					textBounds.X = captionBounds.X + (int)Math.Round((captionBounds.Width - textBounds.Width) / 2f);
					break;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					textBounds.X = captionBounds.Right - textBounds.Width;
					break;
				default: Debug.Assert(false, "Unhandled switch case"); break;
			}
			// set vertical alignment
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomRight:
					textBounds.Y = captionBounds.Bottom - textBounds.Height;
					break;
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleRight:
					textBounds.Y = captionBounds.Top + (int)Math.Round((captionBounds.Height - textBounds.Height) / 2f);
					break;
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopRight:
					textBounds.Y = captionBounds.Top;
					break;
				default: Debug.Assert(false, "Unhandled switch case"); break;
			}
			return textBounds;
		}


		/// <summary>
		/// Resets the caption path.
		/// </summary>
		public void InvalidatePath() {
			if (_textPath != null) _textPath.Reset();
		}


		/// <summary>
		/// Calculates the caption path in the untransformed state.
		/// </summary>
		/// <param name="layoutX">X coordinate of the layout rectangle</param>
		/// <param name="layoutY">Y coordinate of the layout rectangle</param>
		/// <param name="layoutW">Width of the layout rectangle</param>
		/// <param name="layoutH">Height of the layout rectangle</param>
		/// <param name="characterStyle">Character style of the caption</param>
		/// <param name="paragraphStyle">Paragraph style of the caption</param>
		/// <returns></returns>
		public bool CalculatePath(int layoutX, int layoutY, int layoutW, int layoutH, ICharacterStyle characterStyle, IParagraphStyle paragraphStyle) {
			if (characterStyle == null) throw new ArgumentNullException("charStyle");
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			if (string.IsNullOrEmpty(_captionText))
				return true;
			else if (_textPath != null /*&& layoutW > 0 && layoutH > 0*/) {
				// Collect objects for calculating text layout
				Font font = ToolCache.GetFont(characterStyle);
				StringFormat formatter = ToolCache.GetStringFormat(paragraphStyle);
				Rectangle textBounds = Rectangle.Empty;
				textBounds.X = layoutX + paragraphStyle.Padding.Left;
				textBounds.Y = layoutY + paragraphStyle.Padding.Top;
				textBounds.Width = Math.Max(1, layoutW - paragraphStyle.Padding.Horizontal);
				textBounds.Height = Math.Max(1, layoutH - paragraphStyle.Padding.Vertical);
				// Create text path
				_textPath.Reset();
				_textPath.StartFigure();
				_textPath.AddString(PathText, font.FontFamily, (int)font.Style, characterStyle.Size, textBounds, formatter);
				_textPath.CloseFigure();
#if DEBUG_DIAGNOSTICS
				if (_textPath.PointCount == 0 && PathText.Trim() != string.Empty) {
					Size textSize = TextMeasurer.MeasureText(PathText, font, textBounds.Size, paragraphStyle);
					Debug.Print("Failed to create TextPath - please check if the caption bounds are too small for the text.");
				}
#endif
				return true;
			}
			return false;
		}


		/// <summary>
		/// Transforms the text's <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.
		/// </summary>
		/// <param name="matrix"></param>
		public void TransformPath(Matrix matrix) {
			if (matrix == null) throw new ArgumentNullException("matrix");
			if (_textPath != null) _textPath.Transform(matrix);
		}


		/// <summary>
		/// Draws the caption.
		/// </summary>
		public void Draw(Graphics graphics, ICharacterStyle characterStyle, IParagraphStyle paragraphStyle) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (characterStyle == null) throw new ArgumentNullException("charStyle");
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			if (_textPath != null && _textPath.PointCount > 0) {
				Brush brush = ToolCache.GetBrush(characterStyle.ColorStyle);
				graphics.FillPath(brush, _textPath);
			}
		}


		private string PathText {
			get {
				if (_captionText == null) return _captionText;
				else return _captionText + _captionTextSuffix;
			}
		}


		#region Fields

		// The caption's text
		private string _captionText = string.Empty;
		// As a trailing line break will always be ignored when creating a graphics path or when 
		// measuring the text we add an other 'dummy' line break in case the text ends with a line break.
		private string _captionTextSuffix = null;
		// Graphics path of the text
		private GraphicsPath _textPath;
		private bool _isTextVisible = true;

		#endregion
	}


	#region CaptionedShape interface

	/// <summary>
	/// Represents a shape with one or more captions in it.
	/// </summary>
	/// <status>reviewed</status>
	public interface ICaptionedShape {

		/// <summary>
		/// The number of captions the shape currently contains.
		/// </summary>
		int CaptionCount { get; }

		/// <summary>
		/// Finds a caption which contains the given point.
		/// </summary>
		/// <returns>Caption index of -1 if none found.</returns>
		int FindCaptionFromPoint(int x, int y);

		/// <summary>
		/// Retrieves the transformed bounds of the caption in diagram coordinates. These 
		/// bounds define the maximum area the caption text can occupy.
		/// </summary>
		/// <param name="index">Index of the caption</param>
		/// <param name="topLeft">The top left corner of the transformed rectangle defining 
		/// the bounds of the caption</param>
		/// <param name="topRight">The top right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomRight">The bottom right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomLeft">The top bottom left of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <returns>
		/// True if a non-empty text exists for the specified caption, otherwise false. 
		/// If the caption's text is empty, placeholder bounds are calculated.
		/// </returns>
		bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight,
			out Point bottomLeft);

		/// <summary>
		/// Retrieves the character style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		ICharacterStyle GetCaptionCharacterStyle(int index);

		/// <summary>
		/// Retrieves the paragraph style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		IParagraphStyle GetCaptionParagraphStyle(int index);

		/// <summary>
		/// Retrieves the text of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string GetCaptionText(int index);

		/// <summary>
		/// Retrieves the transformed bounds of the caption's text in diagram coordinates. 
		/// These bounds define the current area of the text in the caption.
		/// </summary>
		/// <param name="index">Index of the caption</param>
		/// <param name="topLeft">The top left corner of the transformed rectangle defining 
		/// the bounds of the caption</param>
		/// <param name="topRight">The top right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomRight">The bottom right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomLeft">The top bottom left of the transformed rectangle 
		/// defining the bounds of the caption</param>
		bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft);

		/// <summary>
		/// Sets the character style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="characterStyle"></param>
		void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle);

		/// <summary>
		/// Sets the paragraph style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="paragraphStyle"></param>
		void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle);

		/// <summary>
		/// Sets the text of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="text"></param>
		void SetCaptionText(int index, string text);

		/// <summary>
		/// Shows the specified caption text if hidden.
		/// </summary>
		/// <remarks>For internal use only.</remarks>
		void ShowCaptionText(int index);

		/// <summary>
		/// Hides the specified caption text.
		/// </summary>
		/// <remarks>For internal use only.</remarks>
		void HideCaptionText(int index);

	}

    #endregion


    #region CaptionedShapeBase Class

	/// <summary>
	/// Shape having one caption.
	/// </summary>
	/// <status>reviewed</status>
	public abstract class CaptionedShapeBase : PathBasedPlanarShape, ICaptionedShape {

		/// <summary>
		/// The text of the <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		[CategoryData()]
        [LocalizedDisplayName("PropName_ICaptionedShape_Text")]
		[LocalizedDescription("PropDesc_ICaptionedShape_Text")]
        [PropertyMappingId(PropertyIdText)]
		[RequiredPermission(Permission.Data)]
		[Editor("Dataweb.NShape.WinFormsUI.TextUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public virtual string Text {
			get { return GetCaptionText(0); }
			set {
				if (value != Text)
					SetCaptionText(0, value);
			}
		}


		/// <summary>Gets or sets the <see cref="T:Dataweb.NShape.ICharacterStyle" /> that defines the visual appearance of the shape's text.</summary>
		[CategoryAppearance()]
        [LocalizedDisplayName("PropName_ICaptionedShape_CharacterStyle")]
        [LocalizedDescription("PropDesc_ICaptionedShape_CharacterStyle")]
		[PropertyMappingId(PropertyIdCharacterStyle)]
		[RequiredPermission(Permission.Present)]
		public virtual ICharacterStyle CharacterStyle {
			get { return GetCaptionCharacterStyle(0); }
			set { SetCaptionCharacterStyle(0, value); }
		}


		/// <summary>Gets or sets the <see cref="T:Dataweb.NShape.IParagraphStyle" /> that defines the layout of the shape's text.</summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ICaptionedShape_ParagraphStyle")]
		[LocalizedDescription("PropDesc_ICaptionedShape_ParagraphStyle")]
		[RequiredPermission(Permission.Present)]
		[PropertyMappingId(PropertyIdParagraphStyle)]
		public virtual IParagraphStyle ParagraphStyle {
			get { return GetCaptionParagraphStyle(0); }
			set { SetCaptionParagraphStyle(0, value); }
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			CharacterStyle = styleSet.CharacterStyles.Normal;
			ParagraphStyle = styleSet.ParagraphStyles.Title;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is ICaptionedShape) {
				ICaptionedShape src = (ICaptionedShape)source;
				// Copy first caption
				ICharacterStyle charStyle = src.GetCaptionCharacterStyle(0);
				_privateCharacterStyle = (Template != null && charStyle == ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0)) ? null : charStyle;
				IParagraphStyle paragraphStyle = src.GetCaptionParagraphStyle(0);
				_privateParagraphStyle = (Template != null && paragraphStyle == ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0)) ? null : paragraphStyle;
				string txt = src.GetCaptionText(0);
				if (!string.IsNullOrEmpty(txt)) {
					if (_caption == null) _caption = new Caption(txt);
					else _caption.Text = txt;
				} else _caption = null;

				// Copy remaining captions
				int cnt = Math.Min(CaptionCount, src.CaptionCount);
				for (int i = 1; i < cnt; ++i) {
					SetCaptionCharacterStyle(i, src.GetCaptionCharacterStyle(i));
					SetCaptionParagraphStyle(i, src.GetCaptionParagraphStyle(i));
					SetCaptionText(i, GetCaptionText(i));
				}
			}
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			_privateCharacterStyle = styleSet.GetPreviewStyle(CharacterStyle);
			_privateParagraphStyle = styleSet.GetPreviewStyle(ParagraphStyle);
		}


		/// <override></override>
		public override bool HasStyle(IStyle style) {
			if (IsStyleAffected(ParagraphStyle, style) || IsStyleAffected(CharacterStyle, style))
				return true;
			else return base.HasStyle(style);
		}


		#region ICaptionedShape Members

		/// <summary>Gets the number of captions of this shape.</summary>
		[Browsable(false)]
		public virtual int CaptionCount {
			get { return 1; }
		}


		/// <summary>
		/// Calculates four points that define the (rotated) maximum area the specified caption's text may occupy.
		/// </summary>
		public virtual bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			//if (caption == null) {
			//   topLeft = topRight = bottomLeft = bottomRight = Center;
			//   return false;
			//} else {
				// calc transformed layout caption bounds
				Rectangle captionBounds = Rectangle.Empty;
				CalcCaptionBounds(index, out captionBounds);
				Geometry.TransformRectangle(Center, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
				return true;
			//}
		}


		/// <summary>
		/// Calculates four points that define the (rotated) bounds the specified caption's text actually occupies.
		/// If the caption's text is empty, place holder bounds will be calculated.
		/// </summary>
		public virtual bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			bool result;
			Rectangle captionBounds = Rectangle.Empty;
			CalcCaptionBounds(index, out captionBounds);
			Rectangle textBounds = Rectangle.Empty;
			if (_caption != null) {
				textBounds = _caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
				result = true;
			} else {
				// Calculate placeholder bounds
				textBounds.Size= TextMeasurer.MeasureText("Iq", CharacterStyle, captionBounds.Size, ParagraphStyle);
				textBounds.X = (int)Math.Round(captionBounds.X + (captionBounds.Width / 2f) - textBounds.Width / 2f);
				textBounds.Y = (int)Math.Round(captionBounds.Y + (captionBounds.Height / 2f) - textBounds.Height / 2f);
				result = false;
			}
			Geometry.TransformRectangle(Center, Angle, textBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
			return result;
		}


		/// <summary>Returns the text of the caption at the specified index.</summary>
		public virtual string GetCaptionText(int index) {
			if (index != 0) throw new NShapeException(string.Format("Invalid caption index: {0}.", index));
			return (_caption != null) ? _caption.Text : string.Empty;
		}


		/// <summary>Returns the <see cref="T:Dataweb.NShape.ICharacterStyle" /> of the caption at the specified index.</summary>
		public virtual ICharacterStyle GetCaptionCharacterStyle(int index) {
			if (index != 0) throw new NShapeException(string.Format("Invalid caption index: {0}.", index));
			return _privateCharacterStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0);
		}


		/// <summary>Returns the <see cref="T:Dataweb.NShape.IParagraphStyle" /> of the caption at the specified index.</summary>
		public virtual IParagraphStyle GetCaptionParagraphStyle(int index) {
			if (index != 0) throw new NShapeException(string.Format("Invalid caption index: {0}.", index));
			return _privateParagraphStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0);
		}


		/// <summary>Sets the text of the caption at the specified index.</summary>
		public virtual void SetCaptionText(int index, string text) {
			if (index != 0) throw new NShapeException(string.Format("Invalid caption index: {0}.", index));
			Invalidate();

			// Create or update caption object 
			if (_caption == null) {
				if (!string.IsNullOrEmpty(text))
					_caption = new Caption(text);
			} else {
				_caption.Text = text;
			}
			// Delete caption object for lower memory footprint if it does not contain any vital information
			if (_caption != null && _caption.IsVisible && string.IsNullOrEmpty(_caption.Text))
				_caption = null;

			InvalidateDrawCache();
			Invalidate();
		}


		/// <summary>Returns the <see cref="T:Dataweb.NShape.ICharacterStyle" /> of the caption at the specified index.</summary>
		public virtual void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			if (index != 0) throw new NShapeException(string.Format("Invalid label index: {0}.", index));
			Invalidate();
			_privateCharacterStyle = (Template != null && characterStyle == ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0)) ? null : characterStyle;
			InvalidateDrawCache();
			Invalidate();
		}


		/// <summary>Returns the <see cref="T:Dataweb.NShape.IParagraphStyle" /> of the caption at the specified index.</summary>
		public virtual void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			if (index != 0) throw new NShapeException(string.Format("Invalid label index: {0}.", index));
			Invalidate();
			_privateParagraphStyle = (Template != null && paragraphStyle == ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0)) ? null : paragraphStyle;
			InvalidateDrawCache();
			Invalidate();
		}


		/// <summary>Returns the index of the caption at the specified (diagram) coordinates.</summary>
		public virtual int FindCaptionFromPoint(int x, int y) {
			for (int i = CaptionCount - 1; i >= 0; --i) {
				Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
				GetCaptionTextBounds(i, out tl, out tr, out br, out bl);
				if (Geometry.QuadrangleContainsPoint(tl, tr, br, bl, x, y))
					return i;
			}
			return -1;
		}


		/// <override></override>
		public virtual void HideCaptionText(int index) {
			if (index != 0) throw new IndexOutOfRangeException();
			// Create caption object in order to store the 'IsVisible' information but also because 
			// it is most likely that text is entered and stored while the caption is hidden.
			// If this is not the case, the caption will be deleted when setting the (empty) text.
			if (_caption == null)
				_caption = new Caption();
			_caption.IsVisible = false;
			Invalidate();
		}


		/// <override></override>
		public virtual void ShowCaptionText(int index) {
			if (index != 0) throw new IndexOutOfRangeException();
			if (_caption != null) {
				_caption.IsVisible = true;
				Invalidate();
			}
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.CaptionedShapeBase" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in PathBasedPlanarShape.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("CharacterStyle", typeof(object));
			yield return new EntityFieldDefinition("ParagraphStyle", typeof(object));
			yield return new EntityFieldDefinition("Text", typeof(string));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);

			// ILabel members
			this._privateCharacterStyle = reader.ReadCharacterStyle();
			this._privateParagraphStyle = reader.ReadParagraphStyle();

			string txt = reader.ReadString();
			if (_caption == null) _caption = new Caption(txt);
			else _caption.Text = txt;
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			// ILabel members
			writer.WriteStyle(_privateCharacterStyle);
			writer.WriteStyle(_privateParagraphStyle);
			writer.WriteString(Text);
		}

		#endregion


		/// <summary>
		/// Gets or sets whether the text should be flipped when its rotation angle is between 90° and 270°.
		/// </summary>
		protected bool MaintainTextAngle {
			get { return _maintainTextAngle; }
			set {
				Invalidate();
				_maintainTextAngle = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.CaptionedShape" />.
		/// </summary>
		protected internal CaptionedShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.CaptionedShape" />.
		/// </summary>
		protected internal CaptionedShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdText: 
					Text = propertyMapping.GetString(); 
					break;
				case PropertyIdCharacterStyle: 
					// assign private stylebecause if the style matches the template's style, it would not be assigned.
					CharacterStyle = propertyMapping.GetStyle() as ICharacterStyle;
					Invalidate();
					break;
				case PropertyIdParagraphStyle:
					// assign private stylebecause if the style matches the template's style, it would not be assigned.
					ParagraphStyle = propertyMapping.GetStyle() as IParagraphStyle;
					Invalidate();
					break;
				default: 
					base.ProcessExecModelPropertyChange(propertyMapping); 
					break;
			}
		}


		/// <summary>
		/// Calculates the untransformed area in which the caption's text is layouted.
		/// </summary>
		/// <remarks>The caller has to rotate and offset the rectangle around/by X|Y before using it.</remarks>
		protected abstract void CalcCaptionBounds(int index, out Rectangle captionBounds);


		/// <override></override>
		protected override bool CalculatePath() {
			if (_caption == null) return true;
			bool result = false;
			// Calculate the bounds for the text (defines location and size)
			Rectangle layoutRectangle = Rectangle.Empty;
			CalcCaptionBounds(0, out layoutRectangle);
			// Based on the calculated layout rectangle, calculate the text's graphics path
			result = _caption.CalculatePath(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, CharacterStyle, ParagraphStyle);
			if (MaintainTextAngle && Angle > 900 && Angle < 2700) {
				// Flip text in order to maintain its orientation (we don't want the text to be drawn upside down)
				Matrix.Reset();
				PointF rotationCenter = PointF.Empty;
				rotationCenter.X = layoutRectangle.X + (layoutRectangle.Width / 2f);
				rotationCenter.Y = layoutRectangle.Y + (layoutRectangle.Height / 2f);
				Matrix.RotateAt(180, rotationCenter, MatrixOrder.Append);
				_caption.TransformPath(Matrix);
			}
			return result;
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			// transform DrawCache only if the drawCache is valid, otherwise it will be recalculated
			// at the correct position/size
			if (!DrawCacheIsInvalid && _caption != null) 
				_caption.TransformPath(Matrix);
		}


		/// <summary>
		/// Draws the <see cref="T:Dataweb.NShape.Advanced.Caption" /> of the <see cref="T:Dataweb.NShape.Advanced.CaptionShape" />.
		/// </summary>
		protected void DrawCaption(Graphics graphics) {
			if (_caption != null && _caption.IsVisible) 
				_caption.Draw(graphics, CharacterStyle, ParagraphStyle);
		}


		#region Fields

		/// <summary>Property id of the Text property. Used for model property mappings.</summary>
		protected const int PropertyIdText = 4;
		/// <summary>Property id of the CharacterStyle property. Used for model property mappings.</summary>
		protected const int PropertyIdCharacterStyle = 5;
		/// <summary>Property id of the ParagraphStyle property. Used for model property mappings.</summary>
		protected const int PropertyIdParagraphStyle = 6;
		
		private bool _maintainTextAngle = true;
		private Caption _caption;
		// private styles
		private ICharacterStyle _privateCharacterStyle = null;
		private IParagraphStyle _privateParagraphStyle = null;

		#endregion
	}

	#endregion
}
