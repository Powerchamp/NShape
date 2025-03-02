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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.GeneralModelObjects {

	public enum StateEnum { On, Off, Blocked, Defect, Unknown };


	public class ValueDevice : ModelObjectBase {

		public static ValueDevice CreateInstance(ModelObjectType modelObjectType) {
			return new ValueDevice(modelObjectType);
		}


		protected internal ValueDevice(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		protected internal ValueDevice(ValueDevice source)
			: base(source) {
			this.MaxValue = source.MaxValue;
			this.MinValue = source.MinValue;
		}


		/// <override></override>
		public override IModelObject Clone() {
			return new ValueDevice(this);
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs() {
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}


		[Description("The state of the device. This value is represented by the assigned Shape.")]
		public StateEnum State {
			get { return _state; }
			set { _state = value; }
		}


		//[Description("The current value of the Device.")]
		//public double Value {
		//   get { return this.value; }
		//   set { this.value = value; }
		//}


		[Description("The minimum value of the Device.")]
		public double MinValue {
			get { return _minValue; }
			set { _minValue = value; }
		}


		[Description("The maximum value of the Device.")]
		public double MaxValue {
			get { return _maxValue; }
			set { _maxValue = value; }
		}



		/// <override></override>
		public override char SecurityDomainName {
			get { return _securityDomainName; }
			set {
				if (value < 'A' || value > 'Z')
					throw new ArgumentOutOfRangeException(nameof(SecurityDomainName), "The domain qualifier has to be an upper case  ANSI letter (A-Z).");
				_securityDomainName = value;
			}
		}


		//private double value;
		private StateEnum _state;
		private double _minValue;
		private double _maxValue;
		private char _securityDomainName = 'A';
	}


	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			// Register library
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			
			// Register model object type(s)
			TerminalId maxTerminalId = 2;
			registrar.RegisterModelObjectType(new GenericModelObjectType("ValueDevice", namespaceName, categoryTitle,
				ValueDevice.CreateInstance, ValueDevice.GetPropertyDefinitions, maxTerminalId));

			// Register diagram model object type(s)
			registrar.RegisterDiagramModelObjectType(new GenericDiagramModelObjectType("DiagramModel", namespaceName, categoryTitle,
				GenericDiagramModelObject.CreateInstance, GenericDiagramModelObject.GetPropertyDefinitions));
		}


		#region Fields

		private const string namespaceName = "GeneralModelObjects";
		private const string categoryTitle = "General";
		private const int preferredRepositoryVersion = 3;

		#endregion
	}
}