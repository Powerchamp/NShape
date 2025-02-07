﻿/**************************************************************************************************
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
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Layouters {

	/// <ToBeCompleted></ToBeCompleted>
	public interface ILayouter {

		/// <summary>
		/// Version and language independent name of the layouter.
		/// </summary>
		string InvariantName { get; }

		/// <summary>
		/// Describes the effect of the layouter.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Specifies all shapes in the diagram, including those that must not be moved.
		/// </summary>
		IEnumerable<Shape> AllShapes { get; set; }

		/// <summary>
		/// Specifies the shapes that are to be layouted.
		/// </summary>
		IEnumerable<Shape> Shapes { get; set; }

		/// <summary>
		/// Internally saves the state of the layouting process.
		/// </summary>
		void SaveState();

		/// <summary>
		/// Restores the state of the layouting process.
		/// </summary>
		void RestoreState();

		/// <summary>
		/// Prepares for layouting.
		/// </summary>
		void Prepare();

		/// <summary>
		/// Resets the layouter to the state before the preparation.
		/// </summary>
		void Unprepare();

		/// <summary>
		/// Executes the layouting operation.
		/// </summary>
		void Execute(int maxSeconds);

		/// <summary>
		/// Executes one step of the layouting operation.
		/// </summary>
		/// <returns>False, if layouting is finished.</returns>
		bool ExecuteStep();

		/// <summary>
		/// Fits all layouted shapes within the given rectangle without destroying the layout.
		/// </summary>
		void Fit(int x, int y, int w, int h);

		/// <summary>
		/// Creates a command for the last layout operation and performs a reset.
		/// </summary>
		/// <returns>The command performs the transition between the last state saved in prepare and the 
		/// current state. The current state is then reset to the last state saved such that the caller
		/// can execute the command.</returns>
		ICommand CreateLayoutCommand();
	}


	/// <summary>
	/// Abstract base class for layouters. Implementations of the ILayouter interface
	/// can but need not derive from this class.
	/// </summary>
	public abstract class LayouterBase {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Layouters.LayouterBase" />.
		/// </summary>
		protected LayouterBase(Project project) {
			if (project == null) throw new ArgumentNullException(nameof(project));
			_project = project;
		}

		#region [Public] Properties

		/// <ToBeCompleted></ToBeCompleted>
		public abstract string InvariantName { get; }


		/// <ToBeCompleted></ToBeCompleted>
		public abstract string Description { get; }


		/// <summary>
		/// All shapes in the diagram.
		/// </summary>
		public IEnumerable<Shape> AllShapes {
			get { return _allShapes; }
			set {
				_allShapes.Clear();
				_allShapes.AddRange(value);
			}
		}


		/// <summary>
		/// The shapes being layouted.
		/// </summary>
		public IEnumerable<Shape> Shapes {
			get { return _selectedShapes; }
			set {
				_selectedShapes.Clear();
				_selectedShapes.AddRange(value);
			}
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void SaveState() {
			SelectedPositions.Clear();
			foreach (Shape s in SelectedShapes)
				if (ExcludeFromFitting(s)) SelectedPositions.Add(Point.Empty);
				else SelectedPositions.Add(new Point(s.X, s.Y));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RestoreState() {
			Debug.Assert(SelectedShapes.Count == SelectedPositions.Count);
			for (int i = 0; i < SelectedShapes.Count; ++i)
				if (!ExcludeFromFitting(SelectedShapes[i]))
					SelectedShapes[i].MoveControlPointTo(ControlPointId.Reference, SelectedPositions[i].X, SelectedPositions[i].Y, ResizeModifiers.None);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void Prepare() {
			if (_selectedShapes.Count <= 0) throw new InvalidOperationException("There is no shape selected");
			SaveState();
			_layoutArea = CalcLayoutArea();
			_stepCount = 0;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract void Unprepare();


		/// <ToBeCompleted></ToBeCompleted>
		public void Execute(int maxSeconds) {
			if (SelectedPositions.Count != _selectedShapes.Count)
				throw new InvalidOperationException("You must call Prepare() before calling Execute().");

			if (maxSeconds <= 0) maxSeconds = int.MaxValue;
			DateTime start = DateTime.Now;
			while (ExecuteStep() && start.AddSeconds(maxSeconds) > DateTime.Now);
		}


		/// <summary>
		/// Executes one step of the optimization algorithm.
		/// </summary>
		/// <returns></returns>
		public bool ExecuteStep() {
			bool result = ExecuteStepCore();
			++_stepCount;
			return result;
		}


		/// <summary>
		/// Creates a multi-move command from the original to the final arrangement.
		/// </summary>
		/// <returns></returns>
		public ICommand CreateLayoutCommand() {
			MoveShapesToCommand cmd = new MoveShapesToCommand();
			for (int i = 0; i < _selectedShapes.Count; ++i)
				cmd.AddMoveTo(_selectedShapes[i], SelectedPositions[i].X, SelectedPositions[i].Y, _selectedShapes[i].X, _selectedShapes[i].Y);
			RestoreState();
			return cmd;
		}


		/// <summary>
		/// Fits the shapes into the given rectangle.
		/// </summary>
		public void Fit(int x, int y, int w, int h) {
			// Calculate current bounding rectangle of all shapes
			int x1 = int.MaxValue;
			int y1 = int.MaxValue;
			int x2 = int.MinValue;
			int y2 = int.MinValue;
			Rectangle bounds = Rectangle.Empty;
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;
				bounds = s.GetBoundingRectangle(true);
				if (bounds.Left < x1) x1 = bounds.Left;
				if (bounds.Top < y1) y1 = bounds.Top;
				if (bounds.Right > x2) x2 = bounds.Right;
				if (bounds.Bottom > y2) y2 = bounds.Bottom;
			}
			if (x1 == int.MaxValue) return; // No selected shape is elegible for fitting.

			// Compact shapes if the their bounding rectangle is bigger than the fitting bounds
			if (x2 - x1 > w) {
				foreach (Shape s in Shapes) {
					if (ExcludeFromFitting(s)) continue; 
					int sx = x1 + (s.X - x1) * w / (x2 - x1);
					int sy = s.Y;
					s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
				}
				x2 = x1 + w;
			}
			if (y2 - y1 > h) {
				foreach (Shape s in Shapes) {
					if (ExcludeFromFitting(s)) continue;

					int sx = s.X;
					int sy = y1 + (s.Y - y1) * h / (y2 - y1);
					s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
				}
				y2 = y1 + h;
			}
			// Move shapes if their bounding rectangle is outside the fitting bounds
			int dx = (x1 < x || x2 > x + w) ? (w - x2 - x1 + 2 * x) / 2 : 0;
			int dy = (y1 < y || y2 > y + h) ? (h - y2 - y1 + 2 * y) / 2 : 0;
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;
				int sx = s.X + dx;
				int sy = s.Y + dy;
				s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
			}
		}

		#endregion


		#region [Protected] Properties

		/// <ToBeCompleted></ToBeCompleted>
		protected Project Project {
			get { return _project; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Rectangle LayoutArea {
			get { return _layoutArea; }
		}


		/// <summary>
		/// The stored positions of the selected shapes
		/// </summary>
		protected List<Shape> SelectedShapes {
			get { return _selectedShapes; }
			set { _selectedShapes = value; }
		}


		/// <summary>
		/// The stored positions of the selected shapes
		/// </summary>
		protected List<Point> SelectedPositions {
			get { return _selectedPositions; }
			set { _selectedPositions = value; }
		}


		/// <summary>
		/// Number of steps executed since last call to Prepare
		/// </summary>
		protected int StepCount {
			get { return _stepCount; }
			set { _stepCount = value; }
		}

		#endregion

		/// <summary>
		/// Actual optimizing step execution to be implemented by derived implementations.
		/// </summary>
		protected abstract bool ExecuteStepCore();


		/// <summary>
		/// Calculates a rectangle containing all reference points of all shapes.
		/// The size of the sapes will be ignored, which results in not idempotent layouts.
		/// </summary>
		protected Rectangle CalcLayoutArea() {
			Rectangle result = Rectangle.Empty;
			int left = _selectedShapes[0].X;
			int top = _selectedShapes[0].Y;
			int right = left;
			int bottom = top;
			foreach (Shape s in _selectedShapes) {
				if (s.X < left) left = s.X;
				else if (s.X > right) right = s.X;
				if (s.Y < top) top = s.Y;
				else if (s.Y > bottom) bottom = s.Y;
			}
			result.X = left;
			result.Y = top;
			result.Width = right - left;
			result.Height = bottom - top;
			return result;
		}
		

		// There are lines without glue points (SwitchBar) and planar shapes with glue points (Label)
		// so we better check on glue points here...
		/// <ToBeCompleted></ToBeCompleted>
		protected bool ExcludeFromFitting(Shape shape) {
			foreach (ControlPointId pointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				if (shape.IsConnected(pointId, null) != ControlPointId.None) return true;
			}
			return false;
		}


		private Project _project;
		private List<Shape> _allShapes = new List<Shape>(1000);
		private List<Shape> _selectedShapes = new List<Shape>(100);
		private List<Point> _selectedPositions = new List<Point>(100);
		private Rectangle _layoutArea;
		private int _stepCount;
	}


	/// <summary>
	/// Distributes shapes such that connected shapes are nearer together and unconnected shapes further 
	/// apart. Uses a physical model with repulsive forces and springs between connected shapes.
	/// </summary>
	public class RepulsionLayouter : LayouterBase, ILayouter {

		/// <ToBeCompleted></ToBeCompleted>
		public RepulsionLayouter(Project project)
			: base(project) {
			springRate = 8;
			repulsion = 3;
			repulsionRange = 500;
			friction = 0;
			mass = 500;
		}


		/// <summary>Gets or sets the friction as constant force that slows down the movement of the shapes.</summary>
		public int Friction {
			get { return friction; }
			set { friction = value; }
		}


		/// <summary>
		/// Gets or sets the mass of the shapes in kg. 
		/// Higher mass means that the shapes move shorter distances per time step.
		/// </summary>
		public int Mass {
			get { return mass; }
			set { mass = value; }
		}


		/// <summary>Gets or sets the repulsion between shapes as constant force.</summary>
		public int Repulsion {
			get { return repulsion; }
			set { repulsion = value; }
		}


		/// <summary>Gets or sets the range of the repulsion force in diagram coordinates.</summary>
		public int RepulsionRange {
			get { return repulsionRange; }
			set { repulsionRange = value; }
		}


		/// <summary>Gets or sets the spring rate of the spring that keeps connected shapes together as constant force.</summary>
		public int SpringRate {
			get { return springRate; }
			set { springRate = value; }
		}


		/// <summary>Gets or sets the time interval for the layout process in seconds.</summary>
		public int TimeInterval {
			get { return timeInterval; }
			set { timeInterval = value; }
		}


		/// <override></override>
		public override string InvariantName {
			get { return "Clusters"; }
		}


		/// <override></override>
		public override string Description {
			get { return Dataweb.NShape.Properties.Resources.CaptionTxt_RepulsionLayouterDescription; }
		}


		/// <override></override>
		public override void Prepare() {
			base.Prepare();
			// Nothing to do!?
		}


		/// <override></override>
		public override void Unprepare() {
			// Nothing to do.
		}


		// Perform calculating and moving sperately in order to keep the symmetry of a layout
		/// <override></override>
		protected override bool ExecuteStepCore() {
			List<Size> displacements = new List<Size>();
			// Calculate movements
			Size z = Size.Empty;
			displacements.Clear();
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;
				CalcDisplacement(s, ref z);
				displacements.Add(z);
			}
			// Execut movements
			int maxDisplacement = 0;
			int i = 0;
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;

				int nx = s.X + displacements[i].Width;
				int ny = s.Y + displacements[i].Height;
				s.MoveTo(nx, ny);
				int displacement = (int)Geometry.DistancePointPoint(displacements[i].Width, displacements[i].Height, 0, 0);
				if (displacement > maxDisplacement) maxDisplacement = displacement;
				++i;
			}
			return maxDisplacement > 0;
		}


		private void CalcDisplacement(Shape shape, ref Size displacement) {
			int totalForceX = 0;
			int totalForceY = 0;

			// Summarize all attracting forces
			foreach (ShapeConnectionInfo sci1 in shape.GetConnectionInfos(ControlPointId.Any, null)) {
				foreach (ShapeConnectionInfo sci2 in sci1.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (sci2.OtherShape == shape) continue;
					// Wenn man hier nicht prüft ob das OtherShape ebenfalls verknüpft ist, dann zieht bspw. ein 
					// mit einer verknüpften Linie verknüpftes Shape unendlich lang einseitig an der Linie und 
					// es kommt letztendlich zu einem "Integer Overflow".
					if (ExcludeFromFitting(sci2.OtherShape)) continue;

					// Shape shape ist über s mit sci2.Shape verknüpft
					int distance = (int)Geometry.DistancePointPoint(sci2.OtherShape.X, sci2.OtherShape.Y, shape.X, shape.Y);
					int force = distance * springRate - friction;
					if (force > 0) {
						totalForceX += (sci2.OtherShape.X - shape.X) * force / distance;
						totalForceY += (sci2.OtherShape.Y - shape.Y) * force / distance;
					}
				}
			}
			// Summarize all repulsive forces
			foreach (Shape s in AllShapes) {
				if (ExcludeFromFitting(s)) continue;
				if (s != shape) {
					int distance = (int)Geometry.DistancePointPoint(s.X, s.Y, shape.X, shape.Y);
					if (distance <= repulsionRange) {
						int force = (repulsionRange - distance) * repulsion - friction;
						// The force cannot become negative due to the friction
						if (force < 0) force = 0;
						if (distance <= 0) {
							// The shapes are located at the same coordinate. Therefore, no direction can be calculated.
							// We take a random normal vector, which should ensure that the other shape is moved elsewhere.
							int directionX = random.Next(100);
							int directionY = (int)Math.Sqrt(10000 - directionX * directionX);
							totalForceX += force * directionX / 100;
							totalForceY += force * directionY / 100;
						} else {
							totalForceX += (shape.X - s.X) * force / distance;
							totalForceY += (shape.Y - s.Y) * force / distance;
						}
					}
				}
			}
			// Calculate movement
			displacement.Width = (totalForceX * TimeInterval * TimeInterval / (2 * Mass));
			displacement.Height = (totalForceY * TimeInterval * TimeInterval / (2 * Mass));
		}


		private int springRate;
		private int repulsion;
		private int repulsionRange;
		private int friction; // As constant force
		private int mass = 100; // In kg. Higher mass means that the shapes move shorter distances per time step
		private int timeInterval = 1; // In seconds

		private Random random = new Random();

	}


	/// <summary>
	/// Expands (or shrinks) the space occupied by the specified shapes while maintaining their distribution proportionally.
	/// </summary>
	public class ExpansionLayouter : LayouterBase, ILayouter {

		/// <ToBeCompleted></ToBeCompleted>
		public ExpansionLayouter(Project project)
			: base(project) {
		}


		/// <summary>
		/// Gets or sets the factor (in percent) for expanding/compacting the shapes in horizontal direction. 
		/// 100% means the layout will remain in its original size, values below will shrink the layout, values above will grow the layout.
		/// </summary>
		public int HorizontalCompression {
			get { return horizontalCompression; }
			set { horizontalCompression = value; }
		}


		/// <summary>
		/// Gets or sets the factor (in percent) for expanding/compacting the shapes in vertical direction. 
		/// 100% means the layout will remain in its original size, values below will shrink the layout, values above will grow the layout.
		/// </summary>
		public int VerticalCompression {
			get { return verticalCompression; }
			set { verticalCompression = value; }
		}


		#region ILayouterBase implementation

		/// <override></override>
		public override string InvariantName {
			get { return "Expansion"; }
		}


		/// <override></override>
		public override string Description {
			get { return Dataweb.NShape.Properties.Resources.CaptionTxt_ExpansionLayouterDescription; }
		}


		/// <override></override>
		public override void Prepare() {
			base.Prepare();
			// Nothing to do
		}


		/// <override></override>
		public override void Unprepare() {
			// Nothing to do
		}


		/// <override></override>
		protected override bool ExecuteStepCore() {
			Rectangle boundingArea = CalcLayoutArea();
			Rectangle layoutArea = boundingArea;
			layoutArea.Width = horizontalCompression * layoutArea.Width / 100;
			layoutArea.Height = verticalCompression * layoutArea.Height / 100;
			layoutArea.X = boundingArea.X - (layoutArea.Width - boundingArea.Width)/2;
			layoutArea.Y = boundingArea.Y - (layoutArea.Height - boundingArea.Height)/2;
			//
			foreach (Shape s in SelectedShapes) {
				int nx = layoutArea.X + horizontalCompression * (s.X - boundingArea.X) / 100;
				int ny = layoutArea.Y + verticalCompression * (s.Y - boundingArea.Y) / 100;
				s.MoveControlPointTo(ControlPointId.Reference, nx, ny, ResizeModifiers.None);
			}
			return false;
		}
		
		#endregion

		// compression as %
		private int horizontalCompression = 0;
		private int verticalCompression = 0;

	}

}
