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
using System.Runtime.Serialization;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape {

	// TODO 3: Redesign exceptions
	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeException : Exception {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeException" />.
		/// </summary>
		public NShapeException() : base() { }


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeException" />.
		/// </summary>
		public NShapeException(string message) : base(message) { }


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeException" />.
		/// </summary>
		public NShapeException(string message, Exception innerException)
			: base(message, innerException) { 
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeException" />.
		/// </summary>
		public NShapeException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeException" />.
		/// </summary>
		public NShapeException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeSecurityException : NShapeException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeSecurityException" />.
		/// </summary>
		public NShapeSecurityException(Permission permission)
			: base(Properties.Resources.MessageFmt_RequiredPermission0IsNotGranted, permission) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeSecurityException" />.
		/// </summary>
		public NShapeSecurityException(ICommand command)
			: base(GetMessageText(command)) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeSecurityException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}


		/// <summary>
		/// Extracts a message text for the exception from the given ICommand.
		/// </summary>
		protected static string GetMessageText(ICommand command) {
			if (command != null) {
				string commandText = command.Description ?? command.GetType().Name;
				if (command is Command)
					return string.Format(Properties.Resources.MessageFmt_0DeniedRequiredPermission1IsNotGranted, commandText, ((Command)command).RequiredPermission);
				else 
					return string.Format(Properties.Resources.MessageFmt_0DeniedRequiredPermissionIsNotGranted, commandText);
			} else return Properties.Resources.MessageFmt_RequiredPermissionIsNotGranted;
		}

	}


	
	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class LoadLibraryException : NShapeException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.LoadLibraryException" />.
		/// </summary>
		public LoadLibraryException(string assemblyNameOrPath)
			: base(NoAutoLoadDescriptionFmt, assemblyNameOrPath, Environment.NewLine) {
			this._assemblyNameOrPath = assemblyNameOrPath;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.LoadLibraryException" />.
		/// </summary>
		public LoadLibraryException(string assemblyNameOrPath, Exception exc)
			: base(LoadExcDescriptionFmt, exc, assemblyNameOrPath, Environment.NewLine) {
			this._assemblyNameOrPath = assemblyNameOrPath;
		}


		/// <summary>
		/// Specifies the assembly name or path of the assembly that could not be loaded.
		/// </summary>
		public string AssemblyName {
			get { return _assemblyNameOrPath; }
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected LoadLibraryException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
				_assemblyNameOrPath = info.GetString(MemberNameAssemblyName);
		}


		/// <override></override>
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			info.AddValue(MemberNameAssemblyName, _assemblyNameOrPath);
		}


		static LoadLibraryException(){
			NoAutoLoadDescriptionFmt = Properties.Resources.MessageFmt_RequiredLibrary0WasNotFoundInLoadedLibraries1;
			LoadExcDescriptionFmt = Properties.Resources.MessageFmt_RequiredLibrary0CouldNotBeLoaded1;
		}


		private const string MemberNameAssemblyName = "AssemblyName";
		private static readonly string NoAutoLoadDescriptionFmt;
		private static readonly string LoadExcDescriptionFmt;

		private string _assemblyNameOrPath;
	}
	
	
	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeInternalException : Exception {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public NShapeInternalException(string message)
			: base(message) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public NShapeInternalException(string message, Exception innerException)
			: base(message, innerException) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public NShapeInternalException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public NShapeInternalException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeInternalException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeUnsupportedValueException : NShapeInternalException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeUnsupportedValueException" />.
		/// </summary>
		public NShapeUnsupportedValueException(Type type, object value)
			: base("Unsupported {0} value '{1}'.", type.Name, value) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeUnsupportedValueException" />.
		/// </summary>
		public NShapeUnsupportedValueException(object value)
			: base((value != null) ? string.Format("Unsupported {0} value '{1}'.", value.GetType().Name, value) : "Unsupported value.") {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeUnsupportedValueException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeInterfaceNotSupportedException : NShapeInternalException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInterfaceNotSupportedException" />.
		/// </summary>
		public NShapeInterfaceNotSupportedException(Type instanceType, Type neededInterface)
			: base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInterfaceNotSupportedException" />.
		/// </summary>
		public NShapeInterfaceNotSupportedException(string instanceTypeName, Type neededInterface)
			: base("Type '{0}' does not implement interface '{1}'.", instanceTypeName, neededInterface.FullName) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInterfaceNotSupportedException" />.
		/// </summary>
		public NShapeInterfaceNotSupportedException(ShapeType instanceType, Type neededInterface)
			: base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInterfaceNotSupportedException" />.
		/// </summary>
		public NShapeInterfaceNotSupportedException(ModelObjectType instanceType, Type neededInterface)
			: base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) {
		}
	
	
		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeInterfaceNotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapeMappingNotSupportedException : NShapeInternalException {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeMappingNotSupportedException" />.
		/// </summary>
		public NShapeMappingNotSupportedException(Type shapeType, Type modelType)
			: base("Mapping between proeprty types '{0}' and '{1}' are not supported.", modelType.Name, shapeType.Name) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapeMappingNotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class NShapePropertyNotSetException : NShapeInternalException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapePropertyNotSetException" />.
		/// </summary>
		public NShapePropertyNotSetException(string propertyName)
			: base("Property '{0}' is not set.") {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapePropertyNotSetException" />.
		/// </summary>
		public NShapePropertyNotSetException(object propertyOwner, string propertyName)
			: base("Property '{0}' of {1} is not set.", propertyName, propertyOwner.GetType().Name) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected NShapePropertyNotSetException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	[Serializable]
	public class ItemNotFoundException<T> : NShapeInternalException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapePropertyNotSetException" />.
		/// </summary>
		public ItemNotFoundException(T obj)
			: this(typeof(T), obj) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapePropertyNotSetException" />.
		/// </summary>
		public ItemNotFoundException(Type type, T value)
			: base(string.Format("{0} '{1}' not found in the collection.", type.Name, value)) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapePropertyNotSetException" />.
		/// </summary>
		public ItemNotFoundException(Type type, string value)
			: base(string.Format("{0} '{1}' not found in the collection.", type.Name, value)) {
		}

		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected ItemNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}

}
