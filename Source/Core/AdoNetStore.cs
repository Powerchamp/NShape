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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	/// <summary>
	/// Defines the operation on an entity.
	/// </summary>
	public enum RepositoryCommandType {
		/// <summary>Inserts one entity of the given type.</summary>
		Insert,
		/// <summary>Inserts a shape of a diagram.</summary>
		InsertDiagramShape,
		/// <summary>Inserts the shape of a template.</summary>
		InsertTemplateShape,
		/// <summary>Inserts a child shape of a parent shape.</summary>
		InsertChildShape,
		/// <summary>Inserts the model object of a template.</summary>
		InsertTemplateModelObject,
		/// <summary>Inserts a model object or a child model object of a parent model object.</summary>
		InsertModelModelObject,
		/// <summary>Inserts a model object or a child model object of a parent model object.</summary>
		InsertChildModelObject,
		/// <summary>Inserts a single diagram model object.</summary>
		InsertDiagramModelObject,
		/// <summary>Updates an entity of the given type.</summary>
		Update,
		/// <summary>Sets the owner of a shape to the given diagram.</summary>
		UpdateOwnerDiagram,
		/// <summary>Sets the owner of a shape to the given parent shape.</summary>
		UpdateOwnerShape,
		/// <summary>Sets the owner of a model object to the given model.</summary>
		UpdateOwnerModel,
		/// <summary>Sets the owner of a model object to the given model object.</summary>
		UpdateOwnerModelObject,
		/// <summary>Deletes an entitiy of the given type identified by its id.</summary>
		Delete,
		/// <summary>Selects all entities of a given type.</summary>
		SelectAll,
		/// <summary>Selects the entity of a given type with the indicated id.</summary>
		SelectById,
		/// <summary>?</summary>
		SelectByName,
		/// <summary>Selects all entities of a given type that have the indicated owner.</summary>
		SelectByOwnerId,
		/// <summary>Selects shapes of a given diagram.</summary>
		SelectDiagramShapes,
		/// <summary>Selects the shape for a given template.</summary>
		SelectTemplateShapes,
		/// <summary>Selects shapes with a given parent shape.</summary>
		SelectChildShapes,
		/// <summary>Selects the model objects for a given template.</summary>
		SelectTemplateModelObjects,
		/// <summary>Selects model objects that have no parent model object.</summary>
		SelectModelModelObjects,
		/// <summary>Selects model objects with a given parent model object.</summary>
		SelectChildModelObjects,
		/// <summary>Selects diagram model objects.</summary>
		SelectDiagramModelObjects,
		/// <summary>Checks whether the template with the given id is used in shapes that are currently not loaded.</summary>
		CheckTemplateInUse,
		/// <summary>Checks whether the style with the given id is used in shapes that are currently not loaded.</summary>
		CheckStyleInUse,
		/// <summary>Checks whether the model object with the given id is used in shapes that are currently not loaded.</summary>
		CheckModelObjectInUse,
		/// <summary>Checks whether the shape type with the given type name is used in shapes that are currently not loaded.</summary>
		CheckShapeTypeInUse,
		/// <summary>Checks whether the model object type with the given type name is used in model objects that are currently not loaded.</summary>
		CheckModelObjectTypeInUse
	}


	/// <summary>
	/// An exception that is thrown when an AdoNetStore encounters an error.
	/// </summary>
	[Serializable]
	public class AdoNetStoreException : NShapeException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.AdoNetStoreException"/>.
		/// </summary>
		internal protected AdoNetStoreException(string message)
			: base(message) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.AdoNetStoreException" />.
		/// </summary>
		internal protected AdoNetStoreException(string format, params object[] args)
			: base(format, args) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.AdoNetStoreException" />.
		/// </summary>
		internal protected AdoNetStoreException(string format, Exception innerException, params object[] args)
			: base(format, innerException, args) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected AdoNetStoreException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}
	}


	/// <summary>
	/// An exception that is thrown when a IDbCommand required for a AdoNetStore method was not set.
	/// </summary>
	[Serializable]
	public class MissingCommandException : AdoNetStoreException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.MissingCommandException" />.
		/// </summary>
		internal protected MissingCommandException(string entityTypeName)
			: base(MessageFmt_NotAllRequiredCommandsExist, entityTypeName) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.MissingCommandException" />.
		/// </summary>
		internal protected MissingCommandException(RepositoryCommandType commandType, string entityTypeName)
			: base(MessageFmt_CommandFor0EntitiesOfType1DoesNotExist,
				commandType == RepositoryCommandType.Delete ? Text_Deleting :
				commandType == RepositoryCommandType.Insert ? Text_Inserting :
				commandType == RepositoryCommandType.SelectById ? Text_LoadingSingle :
				//commandType == RepositoryCommandType.SelectByName ? "loading named single " :
				commandType == RepositoryCommandType.SelectAll ? Text_LoadingMultiple :
				commandType == RepositoryCommandType.Update ? Text_Updating : Text_LoadingAndOrSaving, entityTypeName) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.MissingCommandException" />.
		/// </summary>
		internal protected MissingCommandException(string entityTypeName, string filterEntityTypeName)
			: base(MessageFmt_CommandForLoadingEntitiesDoesNotExist, entityTypeName, filterEntityTypeName) {
		}


		private const string MessageFmt_NotAllRequiredCommandsExist = "Not all required commands exist for loading and/or saving entities of type '{0}'.";
		private const string MessageFmt_CommandFor0EntitiesOfType1DoesNotExist = "Command for {0} entities of type '{1}' does not exist.";
		private const string MessageFmt_CommandForLoadingEntitiesDoesNotExist = "Command for loading entities of type '{1}' filtered by Id of '{1}' does not exist.";
		private const string Text_Deleting = "deleting";
		private const string Text_Inserting = "inserting";
		private const string Text_LoadingAndOrSaving = "loading and/or saving";
		private const string Text_LoadingMultiple = "loading multiple";
		private const string Text_LoadingSingle = "loading single";
		private const string Text_Updating = "updating";

	}


	/// <summary>
	/// Stores NShape projects in any ADO.NET enabled database management system.
	/// </summary>
	[ToolboxBitmap(typeof(AdoNetStore))]
	public abstract class AdoNetStore : Store {

		internal const char CompositionFieldSeparatorChar = ',';
		internal const char CompositionSeparatorChar = ';';


		#region [Public] Store Implementation

		/// <override></override>
		public override string ProjectName {
			get { return _projectName; }
			set {
				if (value == null) _projectName = string.Empty;
				else _projectName = value;
			}
		}


		/// <override></override>
		public override void ReadVersion(IStoreCache cache) {
			bool closeConnection = EnsureDataSourceOpen();
			bool commandsLoaded = false;
			try {
				if (_commands.Count == 0) {
					LoadSysCommands();
					commandsLoaded = true;
				}
				_version = DoReadVersion(cache.ProjectName);
				cache.SetRepositoryBaseVersion(_version);
			} finally {
				// As the AdoNetStore assumes the project is open when commands exist, we have to clear them here
				if (commandsLoaded) ClearCommands();
				if (closeConnection) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override bool Exists() {
			bool result;
			bool closeConnection = EnsureDataSourceOpen();
			bool commandsLoaded = false;
			try {
				if (_commands.Count == 0) {
					LoadSysCommands();
					commandsLoaded = true;
				}
				IDbCommand cmd = GetCommand(ProjectSettings.EntityTypeName, RepositoryCommandType.SelectByName);
				((IDataParameter)cmd.Parameters[0]).Value = _projectName;
				using (IDataReader reader = cmd.ExecuteReader())
					result = reader.Read();
			} finally {
				// As the AdoNetStore assumes the project is open when commands exist, we have to clear them here
				if (commandsLoaded) ClearCommands();
				if (closeConnection) EnsureDataSourceClosed();
			}
			return result;
		}


		/// <override></override>
		public override bool CanModifyVersion {
			get { return false; }
		}


		/// <override></override>
		public override void Create(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			// TODO 2: Perhaps check, whether the project name already exists?
			// TODO 2: Perhaps check, that base version is compatible with database schema?
			OpenCore(cache, true);
		}


		/// <override></override>
		public override void Open(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			// We just check whether the cache is reachable.
			OpenCore(cache, false);
		}


		/// <override></override>
		public override void Close(IStoreCache storeCache) {
			if (storeCache == null) throw new ArgumentNullException(nameof(storeCache));
			if (_connection != null) _connection.Dispose();
			_connection = null;
			foreach (KeyValuePair<CommandKey, IDbCommand> item in _commands) {
				if (item.Value != null) item.Value.Dispose();
			}
			_commands.Clear();
			base.Close(storeCache);
		}


		/// <override></override>
		public override void Erase() {
			AssertClosed();
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				LoadSysCommands();
				// If all constraints are in place, deleting the project info is sufficient.
				IDbCommand cmd = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Delete);
				cmd.Transaction = _transaction;
				((DbParameter)cmd.Parameters[0]).Value = _projectName;
				cmd.ExecuteNonQuery();
			} finally {
				_commands.Clear();  // Unload all commands
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void SaveChanges(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			AssertOpen();
			AssertValid();
			const bool transactional = true;
			Connection.Open();
			Debug.Assert(_transaction == null);
			if (transactional) _transaction = Connection.BeginTransaction();
			try {
				// -- Zeroth Step: Insert or update the project --
				IDbCommand projectCommand;
				if (cache.ProjectId == null) {
					// project is a new one
					projectCommand = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Insert);
					((DbParameter)projectCommand.Parameters[0]).Value = cache.ProjectName;
					((DbParameter)projectCommand.Parameters[1]).Value = _version;
					projectCommand.Transaction = _transaction;
					// Cast to int makes sure the command returns an id.
					cache.SetProjectOwnerId((int)projectCommand.ExecuteScalar());
				} else {
					// We update the project name in case it has been modified.
					projectCommand = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Update);
					((DbParameter)projectCommand.Parameters[0]).Value = cache.ProjectId;
					((DbParameter)projectCommand.Parameters[1]).Value = cache.ProjectName;
					((DbParameter)projectCommand.Parameters[2]).Value = _version;
					projectCommand.Transaction = _transaction;
					projectCommand.ExecuteNonQuery();
				}
				// -- First Step: Delete --
				// Children first, owners afterwards
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.ModelObject)
						DeleteEntities<IModelObject>(cache, et, cache.LoadedModelObjects, delegate (IModelObject s, IEntity o) { return s.Type.FullName == et.FullName; });
				DeleteEntities<Model>(cache, cache.FindEntityTypeByName(Model.EntityTypeName), cache.LoadedModels, null);
				DeleteShapeConnections(cache);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						DeleteEntities<Shape>(cache, et, cache.LoadedShapes, delegate (Shape s, IEntity o) { return s.Type.FullName == et.FullName; });
				DeleteEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.LoadedDiagrams, null);
				DeleteEntities<IModelMapping>(cache, cache.FindEntityTypeByName(NumericModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is NumericModelMapping);
				DeleteEntities<IModelMapping>(cache, cache.FindEntityTypeByName(FormatModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is FormatModelMapping);
				DeleteEntities<IModelMapping>(cache, cache.FindEntityTypeByName(StyleModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is StyleModelMapping);
				DeleteEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName), cache.LoadedTemplates, null);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ColorStyle);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CapStyle);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CharacterStyle);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is FillStyle);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is LineStyle);
				DeleteEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ParagraphStyle);
				DeleteEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName), cache.LoadedDesigns, null);
				DeleteEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.LoadedProjects, null);
				//
				// -- Second Step: Insert --
				// Owners first, children afterwards
				// Insert new entities before the update phase - otherwise updating entities referencing these entities will fail.
				InsertEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.NewProjects, null);
				InsertEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName), cache.NewDesigns, null);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), cache.NewStyles, (s, o) => s is ColorStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), cache.NewStyles, (s, o) => s is CapStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), cache.NewStyles, (s, o) => s is LineStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), cache.NewStyles, (s, o) => s is FillStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), cache.NewStyles, (s, o) => s is CharacterStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), cache.NewStyles, (s, o) => s is ParagraphStyle);
				// Flush templates and their modelObjects, shapes and model mappings
				InsertEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName), cache.NewTemplates, null);
				// Flush model objects owned by templates
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.ModelObject) {
						// Flush parents first, then children
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetCommand(et.FullName, RepositoryCommandType.InsertTemplateModelObject),
							(m, o) => (m.Parent == null && m.Type.FullName == et.FullName && o is Template));
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetCommand(et.FullName, RepositoryCommandType.InsertTemplateModelObject),
							(m, o) => (m.Parent != null && m.Type.FullName == et.FullName && o is Template));
					}
				// Flush shapes owned by templates
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						InsertEntities<Shape>(cache, et, cache.NewShapes, GetCommand(et.FullName, RepositoryCommandType.InsertTemplateShape),
							(s, o) => o is Template && s.Type.FullName == et.FullName);
				// Flush model mappings
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(NumericModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is NumericModelMapping);
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(FormatModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is FormatModelMapping);
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(StyleModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is StyleModelMapping);
				// Flush model
				InsertEntities<Model>(cache, cache.FindEntityTypeByName(Model.EntityTypeName), cache.NewModels, null);
				// Flush model objects
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						// Flush parents first, then children
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetCommand(et.FullName, RepositoryCommandType.InsertModelModelObject),
							(m, o) => (m.Parent == null && m.Type.FullName == et.FullName && !(o is Template)));
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetCommand(et.FullName, RepositoryCommandType.InsertModelModelObject),
							(m, o) => (m.Parent != null && m.Type.FullName == et.FullName && !(o is Template)));
					}
				}
				// Flush diagram model objects
				if (Version >= 7) {
					foreach (EntityType et in cache.EntityTypes) {
						if (et.Category == EntityCategory.DiagramModelObject)
							InsertEntities<IDiagramModelObject>(cache, et, cache.NewDiagramModelObjects, GetCommand(et.FullName, RepositoryCommandType.InsertDiagramModelObject),
								(m, o) => m.Type.FullName == et.FullName);
					}
				}
				// Flush diagrams and their shapes
				InsertEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.NewDiagrams, null);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						InsertEntities<Shape>(cache, et, cache.NewShapes, GetCommand(et.FullName, RepositoryCommandType.InsertDiagramShape),
							(s, o) => o is Diagram && s.Type.FullName == et.FullName);
				// RUNTIME CHECK: At this point we only must have new shapes left that are template shapes or child shapes
				foreach (KeyValuePair<Shape, IEntity> p in cache.NewShapes)
					Debug.Assert(((IEntity)p.Key).Id != null || p.Value is Template || p.Value is Shape);
				// Flush child shapes level by level
				// In each cycle we insert all those shapes, whose parent has already been inserted.
				bool allInserted;
				do {
					allInserted = true;
					foreach (EntityType et in cache.EntityTypes)
						if (et.Category == EntityCategory.Shape)
							InsertEntities<Shape>(cache, et, cache.NewShapes, GetCommand(et.FullName, RepositoryCommandType.InsertChildShape),
								delegate (Shape s, IEntity o) { if (((IEntity)s).Id != null) return false; else { allInserted = false; return s.Type.FullName == et.FullName && ((IEntity)s.Parent).Id != null; } });
				} while (!allInserted);
				InsertShapeConnections(cache);
				UpdateShapeOwners(cache);
				//
				// -- Third Step: Update --
				// Owners first, children afterwards
				UpdateEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.LoadedProjects, null);
				UpdateEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName), cache.LoadedDesigns, null);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ColorStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CapStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is LineStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is FillStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CharacterStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ParagraphStyle);
				UpdateEntities<Model>(cache, cache.FindEntityTypeByName(Model.EntityTypeName), cache.LoadedModels, null);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.ModelObject)
						UpdateEntities<IModelObject>(cache, et, cache.LoadedModelObjects, delegate (IModelObject s, IEntity o) { return s.Type.FullName == et.FullName; });
				UpdateEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName), cache.LoadedTemplates, null);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(NumericModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is NumericModelMapping);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(FormatModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is FormatModelMapping);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(StyleModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is StyleModelMapping);
				UpdateEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.LoadedDiagrams, null);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						UpdateEntities<Shape>(cache, et, cache.LoadedShapes, delegate (Shape s, IEntity o) { return s.Type.FullName == et.FullName; });
				if (transactional) _transaction.Commit();
			} catch (Exception exc) {
				Debug.Print(exc.Message);
				if (transactional) _transaction.Rollback();
				throw;
			} finally {
				if (_transaction != null) {
					_transaction.Dispose();
					_transaction = null;
				}
				Connection.Close();
			}
		}


		/// <override></override>
		public override void LoadTemplates(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				// Load all templates
				foreach (EntityBucket<Template> tb in LoadEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName),
					id => !cache.LoadedTemplates.Contains(id), id => cache.LoadedProjects.GetEntity(id),
					RepositoryCommandType.SelectByOwnerId, projectId)) {
					cache.LoadedTemplates.Add(tb);
				}
				// Load all template model objects. The template shapes will assign them in their load method
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> sb in LoadEntities<IModelObject>(cache, et, id => true, id => cache.LoadedTemplates.GetEntity(id),
							RepositoryCommandType.SelectTemplateModelObjects, cache.ProjectId)) {
							cache.LoadedModelObjects.Add(sb);
						}
					}
				}
				// Load all template shapes and assign them to their templates
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb in LoadEntities<Shape>(cache, et, id => true, id => cache.LoadedTemplates.GetEntity(id),
							RepositoryCommandType.SelectTemplateShapes, cache.ProjectId)) {
							Template t = (Template)sb.Owner;
							if (t.Shape != null) throw new AdoNetStoreException(ResourceStrings.MessageFmt_Template0HasMoreThanOneShape, t.Id);
							((Template)sb.Owner).Shape = sb.ObjectRef;
							LoadChildShapes(cache, sb.ObjectRef);
							cache.LoadedShapes.Add(sb);
						}
					}
				}
				// Load and assign all ModelMappings of the template.
				// Model mappings have to be assigned after shape and model object
				foreach (EntityBucket<Template> tb in cache.LoadedTemplates) {
					if (tb.ObjectRef.Shape == null) throw new AdoNetStoreException(ResourceStrings.MessageFmt_Template0HasNoShape, tb.ObjectRef.Id);
					foreach (EntityType et in cache.EntityTypes) {
						if (et.Category == EntityCategory.ModelMapping) {
							foreach (EntityBucket<IModelMapping> eb in LoadEntities<IModelMapping>(cache, et,
								id => true, id => tb.ObjectRef, RepositoryCommandType.SelectByOwnerId, tb.ObjectRef.Id)) {
								cache.LoadedModelMappings.Add(eb);
								((Template)tb.ObjectRef).MapProperties(eb.ObjectRef);
							}
						}
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadProjects(IStoreCache cache, IEntityType entityType, params object[] parameters) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (entityType == null) throw new ArgumentNullException(nameof(entityType));
			foreach (EntityBucket<ProjectSettings> pb in LoadEntities<ProjectSettings>(cache, entityType, id => true, id => null,
				RepositoryCommandType.SelectByName, cache.ProjectName))
				cache.LoadedProjects.Add(pb);
		}


		/// <override></override>
		public override void LoadModel(IStoreCache cache, object modelId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				// Load model
				foreach (EntityBucket<Model> mb in LoadEntities<Model>(cache,
					cache.FindEntityTypeByName(Model.EntityTypeName), id => true,
					id => cache.Project, RepositoryCommandType.SelectByOwnerId, cache.ProjectId))
					cache.LoadedModels.Add(mb);
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadDesigns(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			Debug.Assert(projectId == null && ((IEntity)cache.Project).Id == null || ((IEntity)cache.Project).Id.Equals(projectId));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				foreach (EntityBucket<Design> pb in LoadEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName),
					id => true, id => cache.Project, RepositoryCommandType.SelectByOwnerId, projectId)) {
					cache.LoadedDesigns.Add(pb);
					// Load the styles of the design (They reference each other so put them into the collections immediately.)
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadDiagramShapes(IStoreCache cache, Diagram diagram) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				// Load all shapes of diagram
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb
							in LoadEntities<Shape>(cache, et, id => !cache.LoadedShapes.Contains(id), pid => diagram,
							RepositoryCommandType.SelectDiagramShapes, ((IEntity)diagram).Id)) {
							Debug.Assert(!diagram.Shapes.Contains(sb.ObjectRef));
							diagram.Shapes.Add(sb.ObjectRef, sb.ObjectRef.ZOrder);
							diagram.AddShapeToLayers(sb.ObjectRef, sb.ObjectRef.HomeLayer, sb.ObjectRef.SupplementalLayers);
							LoadChildShapes(cache, sb.ObjectRef);
							cache.LoadedShapes.Add(sb);
						}
					}
				}
				// Load all shape connections of diagram
				LoadShapeConnections(cache, diagram);
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadTemplateShapes(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void LoadDiagrams(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			foreach (EntityBucket<Diagram> db in LoadEntities<Diagram>(cache,
				cache.FindEntityTypeByName(Diagram.EntityTypeName), id => true,
				id => cache.Project, RepositoryCommandType.SelectByOwnerId, cache.ProjectId)) {
				cache.LoadedDiagrams.Add(db);
			}
		}


		/// <override></override>
		public override void LoadChildShapes(IStoreCache cache, object parentShapeId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (parentShapeId == null) throw new ArgumentNullException(nameof(parentShapeId));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				Shape parentShape = cache.GetShape(parentShapeId);
				// Load all shapes of diagram
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb
							in LoadEntities<Shape>(cache, et, id => !cache.LoadedShapes.Contains(id), pid => parentShape,
							RepositoryCommandType.SelectDiagramShapes, parentShapeId)) {
							Debug.Assert(!parentShape.Children.Contains(sb.ObjectRef));
							parentShape.Children.Add(sb.ObjectRef);
							cache.LoadedShapes.Add(sb);
						}
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadTemplateModelObjects(IStoreCache cache, object templateId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (templateId == null) throw new ArgumentNullException(nameof(templateId));
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void LoadModelModelObjects(IStoreCache cache, object modelId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (modelId == null) throw new ArgumentNullException(nameof(modelId));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				// Load all root model objects of the model
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> mb
							in LoadEntities<IModelObject>(cache, et, id => !cache.LoadedModelObjects.Contains(id),
							id => cache.GetModel(), RepositoryCommandType.SelectModelModelObjects, modelId)) {
							cache.LoadedModelObjects.Add(mb);
							LoadChildModelObjects(cache, mb.ObjectRef.Id);
						}
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadChildModelObjects(IStoreCache cache, object parentModelObjectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (parentModelObjectId == null) throw new ArgumentNullException(nameof(parentModelObjectId));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				// Load all root model objects of the model
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> mb
							in LoadEntities<IModelObject>(cache, et, id => !cache.LoadedModelObjects.Contains(id),
							id => cache.GetModelObject(parentModelObjectId), RepositoryCommandType.SelectChildModelObjects, parentModelObjectId)) {
							cache.LoadedModelObjects.Add(mb);
						}
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadDiagramModelObjects(IStoreCache cache, object modelId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (modelId == null) throw new ArgumentNullException(nameof(modelId));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				if (Version >= 7) {
					// Load all root model objects of the model
					foreach (EntityType et in cache.EntityTypes) {
						if (et.Category == EntityCategory.DiagramModelObject) {
							foreach (EntityBucket<IDiagramModelObject> mb in LoadEntities<IDiagramModelObject>(cache, et, id => !cache.LoadedDiagramModelObjects.Contains(id),
								id => cache.Project, RepositoryCommandType.SelectDiagramModelObjects, modelId)) {
								cache.LoadedDiagramModelObjects.Add(mb);
							}
						}
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override bool CheckTemplateInUse(IStoreCache cache, object templateId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (templateId == null) throw new ArgumentNullException(nameof(templateId));
			bool result = false;
			bool closeDataSource = EnsureDataSourceOpen();
			try {
				DbCommand command = (DbCommand)GetCommand(ProjectSettings.EntityTypeName, RepositoryCommandType.CheckTemplateInUse);
				command.Prepare();
				// We have to check all shapes in all diagrams of the project that are *not* loaded
				// because the already loaded diagrams may have been modified, deleted, etc.

				foreach (Diagram diagram in ((IRepository)cache).GetDiagrams()) {
					EntityBucket<Diagram> diagramBucket = cache.LoadedDiagrams[((IEntity)diagram).Id];
					if (!(diagramBucket.State == ItemState.Original && diagramBucket.ObjectRef.Shapes.Count == 0))
						continue;   // As long as partial diagram loading is not supported, this check is sufficient
					command.Parameters["Project"].Value = cache.ProjectId;
					command.Parameters["Diagram"].Value = ((IEntity)diagram).Id;
					command.Parameters["Template"].Value = templateId;
					result = ((int)command.ExecuteScalar() != 0);
					if (result == true) break;
				}
			} finally {
				if (closeDataSource) EnsureDataSourceClosed();
			}
			return result;
		}


		/// <override></override>
		public override bool CheckModelObjectInUse(IStoreCache cache, object modelObjectId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (modelObjectId == null) throw new ArgumentNullException(nameof(modelObjectId));
			bool result = false;
			bool closeDataSource = EnsureDataSourceOpen();
			try {
				DbCommand command = (DbCommand)GetCommand(ProjectSettings.EntityTypeName, RepositoryCommandType.CheckModelObjectInUse);
				command.Prepare();
				// We have to check all shapes in all diagrams of the project that are *not* loaded
				// because the already loaded diagrams may have been modified, deleted, etc.

				foreach (Diagram diagram in ((IRepository)cache).GetDiagrams()) {
					EntityBucket<Diagram> diagramBucket = cache.LoadedDiagrams[((IEntity)diagram).Id];
					if (!(diagramBucket.State == ItemState.Original && diagramBucket.ObjectRef.Shapes.Count == 0))
						continue;   // As long as partial diagram loading is not supported, this check is sufficient
					command.Parameters["Project"].Value = cache.ProjectId;
					command.Parameters["Diagram"].Value = ((IEntity)diagram).Id;
					command.Parameters["ModelObject"].Value = modelObjectId;
					result = ((int)command.ExecuteScalar() != 0);
					if (result == true) break;
				}
			} finally {
				if (closeDataSource) EnsureDataSourceClosed();
			}
			return result;
		}


		/// <override></override>
		public override bool CheckStyleInUse(IStoreCache cache, object styleId) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (styleId == null) throw new ArgumentNullException(nameof(styleId));
			bool result = false;
			bool closeDataSource = EnsureDataSourceOpen();
			try {
				DbCommand command = (DbCommand)GetCommand(ProjectSettings.EntityTypeName, RepositoryCommandType.CheckStyleInUse);
				command.Prepare();
				// We have to check all shapes in all diagrams of the project that are *not* loaded
				// because the already loaded diagrams may have been modified, deleted, etc.
				foreach (Diagram diagram in ((IRepository)cache).GetDiagrams()) {
					EntityBucket<Diagram> diagramBucket = cache.LoadedDiagrams[((IEntity)diagram).Id];
					if (!(diagramBucket.State == ItemState.Original && diagramBucket.ObjectRef.Shapes.Count == 0))
						continue;   // As long as partial diagram loading is not supported, this check is sufficient
					command.Parameters["Project"].Value = cache.ProjectId;
					command.Parameters["Diagram"].Value = ((IEntity)diagram).Id;
					command.Parameters["Style"].Value = styleId;
					result = ((int)command.ExecuteScalar() != 0);
					if (result == true) break;
				}
			} finally {
				if (closeDataSource) EnsureDataSourceClosed();
			}
			return result;
		}


		/// <override></override>
		public override bool CheckShapeTypeInUse(IStoreCache cache, string typeName) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException(nameof(typeName));
			bool result = false;
			bool closeDatasource = EnsureDataSourceOpen();
			try {
				// We have to check all shapes in all diagrams of the project that are *not* loaded
				foreach (Diagram diagram in ((IRepository)cache).GetDiagrams()) {
					EntityBucket<Diagram> diagramBucket = cache.LoadedDiagrams[((IEntity)diagram).Id];
					if (diagramBucket.State == ItemState.Original && diagramBucket.ObjectRef.Shapes.Count == 0) {
						DbCommand command = (DbCommand)GetCommand(typeName, RepositoryCommandType.CheckShapeTypeInUse);
						command.Parameters["Project"].Value = cache.ProjectId;
						command.Parameters["Diagram"].Value = ((IEntity)diagramBucket.ObjectRef).Id;
						int cnt = (int)command.ExecuteScalar();
						result = (cnt > 0);
					}
					if (result == true) break;
				}
			} finally {
				if (closeDatasource) EnsureDataSourceClosed();
			}
			return result;
		}

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Specifies the name of the ADO.NET provider used as listed in the system configuration.
		/// </summary>
		public string ProviderName {
			get { return _providerName; }
			set {
				_providerName = value;
				_factory = string.IsNullOrEmpty(_providerName) ? null : DbProviderFactories.GetFactory(_providerName);
			}
		}


		/// <summary>
		/// Specifies the connection string for the database store.
		/// </summary>
		public string ConnectionString {
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		#endregion


		#region [Public] Methods: Obtaining Commands

		/// <summary>
		/// Retrieves a command text for checking whether the 'SysCommand' table exists.
		/// </summary>
		public virtual IDbCommand GetSysCommandTableExistsCommand() {
			IDbCommand result = CreateCommand(
				"IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SysCommand') " +
				"	SELECT 1 AS res ELSE SELECT 0 AS res");
			result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Retrieves a command text for creating sys commands entries.
		/// Table SysCommand contains all SQL commands for loading/saving the repository from/to the database.
		/// </summary>
		public virtual IDbCommand GetInsertSysCommandCommand() {
			IDbCommand result = CreateCommand(
				"INSERT INTO SysCommand (Kind, EntityType, Text) VALUES (@Kind, @EntityType, @Text); "
				+ "SELECT CAST(IDENT_CURRENT('SysCommand') AS INT)",
				CreateParameter("Kind", DbType.String),
				CreateParameter("EntityType", DbType.String),
				CreateParameter("Text", DbType.String));
			result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Retrieves a command for inserting command parameters into the database.
		/// </summary>
		public virtual IDbCommand GetInsertSysParameterCommand() {
			IDbCommand result = CreateCommand("INSERT INTO SysParameter (Command, No, Name, Type) VALUES (@Command, @No, @Name, @Type)",
				CreateParameter("Command", DbType.Int32),
				CreateParameter("No", DbType.Int32),
				CreateParameter("Name", DbType.String),
				CreateParameter("Type", DbType.String));
			result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Creates and returns a command for reading the specified command.
		/// </summary>
		public IDbCommand GetSelectSysCommandsCommand() {
			IDbCommand result = CreateCommand("SELECT * FROM SysCommand");
			result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Creates and returns a command for reading the specified command parameter.
		/// </summary>
		public IDbCommand GetSelectSysParameterCommand() {
			// The column 'No', which defines the parameter sequence, was introduced with version 3.
			IDbCommand result = CreateCommand(
				string.Format("SELECT Name, Type FROM SysParameter WHERE Command = @Command {0}", _version <= 2 ? string.Empty : "ORDER BY No"),
				CreateParameter("Command", DbType.Int32));
			result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Retrieves a command that creates all tables of the database schema.
		/// </summary>
		public IDbCommand GetCreateTablesCommand() {
			if (_createTablesCommand == null) throw new AdoNetStoreException(ResourceStrings.MessageTxt_CommandForCreatingTheSchemaIsNotDefined);
			if (_createTablesCommand.Connection != Connection) _createTablesCommand.Connection = Connection;
			return _createTablesCommand;
		}


		/// <summary>
		/// Sets a command that creates all tables of the database schema.
		/// </summary>
		public void SetCreateTablesCommand(IDbCommand command) {
			if (command == null) throw new ArgumentNullException(nameof(command));
			_createTablesCommand = command;
		}


		/// <summary>
		/// Retrieves a command of the specified type for the specified entity type.
		/// </summary>
		public IDbCommand GetCommand(string entityTypeName, RepositoryCommandType cmdType) {
			if (string.IsNullOrEmpty(entityTypeName)) throw new ArgumentNullException(nameof(entityTypeName));
			IDbCommand result;
			CommandKey commandKey;
			commandKey.Kind = cmdType;
			commandKey.EntityTypeName = entityTypeName;
			if (!_commands.TryGetValue(commandKey, out result))
				throw new InvalidOperationException(string.Format(ResourceStrings.MessageFmt_ADONETCommand0ForEntityType1HasNotBeenDefined, cmdType, entityTypeName));
			if (result.Connection != Connection) result.Connection = Connection;
			return result;
		}


		/// <summary>
		/// Sets a command of the specified type for the specified entity type.
		/// </summary>
		public void SetCommand(string entityTypeName, RepositoryCommandType cmdType, IDbCommand command) {
			if (string.IsNullOrEmpty(entityTypeName)) throw new ArgumentNullException(nameof(entityTypeName));
			if (command == null) throw new ArgumentNullException(nameof(command));
			CommandKey commandKey;
			commandKey.Kind = cmdType;
			commandKey.EntityTypeName = entityTypeName;
			if (_commands.ContainsKey(commandKey))
				_commands[commandKey] = command;
			else _commands.Add(commandKey, command);
		}

		#endregion


		/// <override></override>
		protected internal override int Version {
			get { return _version; }
			set { _version = value; }
		}


		internal IDbTransaction CurrentTransaction {
			get { return _transaction; }
		}


		/// <summary>
		/// Tests whether a inner object entity should be stored as serialized string.
		/// </summary>
		static protected bool IsComposition(EntityPropertyDefinition propertyInfo) {
			return (string.Compare(propertyInfo.Name, "ConnectionPointMappings", StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(propertyInfo.Name, "ValueRanges", StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(propertyInfo.Name, "Vertices", StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(propertyInfo.Name, "ConnectionPoints", StringComparison.InvariantCultureIgnoreCase) == 0
				//|| string.Compare(propertyInfo.Name, "ColumnNames", StringComparison.InvariantCultureIgnoreCase) == 0)
				|| string.Compare(propertyInfo.Name, "TableColumns", StringComparison.InvariantCultureIgnoreCase) == 0);
		}


		#region [Protected] Methods: Creating Commands and Parameters

		// Command setting functions must specify the entity by the entity name and 
		// not by the entity type, because it must be possible to set commands, before 
		// libraries are loaded. Therefore the entities are usually registered later
		// than the command.
		// Another reason is that external clients have no access to the entity type.
		// They should not even know the interface IEntityType or the type EntityType.

		/// <summary>
		/// Creates an ADO.NET command.
		/// </summary>
		protected IDbCommand CreateCommand() {
			if (_factory == null)
				throw new AdoNetStoreException(ResourceStrings.MessageTxt_NoValidADONETProviderSpecified);
			return _factory.CreateCommand();
		}


		/// <summary>
		/// Creates an ADO.NET command.
		/// </summary>
		protected IDbCommand CreateCommand(string cmdText, params IDataParameter[] parameters) {
			IDbCommand result = CreateCommand();
			result.CommandText = cmdText;
			foreach (IDataParameter p in parameters)
				if (p != null) result.Parameters.Add(p);
			return result;
		}


		/// <summary>
		/// Creates an ADO.NET command parameter.
		/// </summary>
		protected IDataParameter CreateParameter(string name, DbType dbType) {
			IDataParameter result = _factory.CreateParameter();
			result.DbType = dbType;
			result.ParameterName = name;
			return result;
		}

		#endregion


		#region [Protected] Methods

		/// <override></override>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// Dispose and delete commands
				if (_commands != null) {
					foreach (KeyValuePair<CommandKey, IDbCommand> item in _commands)
						if (item.Value != null) item.Value.Dispose();
					_commands.Clear();
				}
				if (_createTablesCommand != null) _createTablesCommand.Dispose();
				if (_transaction != null) _transaction.Dispose();
				// Close and dispose connection
				if (_connection != null) {
					if (_connection.State != ConnectionState.Closed) _connection.Close();
					_connection.Dispose();
				}
			}
			base.Dispose(disposing);
		}


		/// <summary>
		/// Override this method to create the actual SQL commands for your database.
		/// </summary>
		public virtual void CreateDbCommands(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			foreach (KeyValuePair<CommandKey, IDbCommand> item in _commands)
				item.Value.Dispose();
			_commands.Clear();
			Version = cache.RepositoryBaseVersion;
		}


		/// <summary>
		/// Creates a schema for the database based on the current DB commands.
		/// </summary>
		/// <remarks>
		/// This function has to be regarded more as a testing feature. Real life application will usually provide 
		/// their specialized database schemas and generation scripts.
		/// </remarks>
		public virtual void CreateDbSchema(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			AssertClosed();
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				Version = cache.RepositoryBaseVersion;
				// Create the actual schema
				GetCreateTablesCommand().ExecuteNonQuery();
				// Insert the SQL statements into the SysCommand table.
				IDbCommand cmdCmd = GetInsertSysCommandCommand();
				IDbCommand paramCmd = GetInsertSysParameterCommand();
				try {
					cmdCmd.Prepare();
					paramCmd.Prepare();
					foreach (KeyValuePair<CommandKey, IDbCommand> item in _commands) {
						((IDbDataParameter)cmdCmd.Parameters[0]).Value = item.Key.Kind;
						((IDbDataParameter)cmdCmd.Parameters[1]).Value = item.Key.EntityTypeName;
						((IDbDataParameter)cmdCmd.Parameters[2]).Value = item.Value.CommandText;
						int cmdId = (int)cmdCmd.ExecuteScalar();
						int paramCnt = item.Value.Parameters.Count;
						for (int i = 0; i < paramCnt; ++i) {
							IDataParameter p = (IDataParameter)item.Value.Parameters[i];
							((IDbDataParameter)paramCmd.Parameters[0]).Value = cmdId;
							((IDbDataParameter)paramCmd.Parameters[1]).Value = i + 1;
							((IDbDataParameter)paramCmd.Parameters[2]).Value = p.ParameterName;
							((IDbDataParameter)paramCmd.Parameters[3]).Value = p.DbType.ToString();
							paramCmd.ExecuteNonQuery();
						}
					}
				} finally {
					paramCmd.Dispose();
					cmdCmd.Dispose();
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <summary>
		/// Deletes the schema for the database based on the current DB commands.
		/// </summary>
		/// <remarks>
		/// This function has to be regarded more as a testing feature. Real life application will usually provide 
		/// their specialized database schemas and generation scripts.
		/// </remarks>
		public virtual void DropDbSchema() {
			AssertClosed();
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				try {
					bool sysCommandsTableExists;
					using (IDbCommand command = GetSysCommandTableExistsCommand())
						sysCommandsTableExists = Convert.ToInt64(command.ExecuteScalar()) == 1;

					if (sysCommandsTableExists) {
						LoadSysCommands();

						IDbCommand dropCommand = GetCommand("All", RepositoryCommandType.Delete);
						dropCommand.Connection = _connection;
						dropCommand.ExecuteNonQuery();
					}
				} catch (DbException exc) {
					if (exc.ErrorCode == -2146232060) {
						// Assumption: No SysCommand table available, i.e. no NShape tables present
						Trace.TraceWarning(exc.Message);
						return;
					} else throw exc;
				}
			} finally {
				ClearCommands();
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <summary>
		/// Loads the commands for loading and saving entities from the data store.
		/// </summary>
		protected virtual void LoadSysCommands() {
			ClearCommands();
			IDbCommand commandCmd = null;
			IDbCommand paramCmd = null;
			try {
				commandCmd = GetSelectSysCommandsCommand();
				paramCmd = GetSelectSysParameterCommand();
				CommandKey ck;
				using (IDataReader commandReader = commandCmd.ExecuteReader()) {
					while (commandReader.Read()) {
						ck.Kind = (RepositoryCommandType)Enum.Parse(typeof(RepositoryCommandType), commandReader.GetString(1));
						ck.EntityTypeName = commandReader.GetString(2);
						IDbCommand command = CreateCommand(commandReader.GetString(3));
						// Read parameters
						((IDataParameter)paramCmd.Parameters[0]).Value = commandReader.GetInt32(0);
						using (IDataReader paramReader = paramCmd.ExecuteReader()) {
							while (paramReader.Read()) {
								command.Parameters.Add(CreateParameter(paramReader.GetString(0),
									(DbType)Enum.Parse(typeof(DbType), paramReader.GetString(1))));
							}
						}
						if (command.CommandText.Contains("@") && command.Parameters.Count == 0) {
							// Skip command if command text contains parameters but no parameters were found in the parameters table
							Debug.Print(string.Format("No command parameters found for entity type '{0}'", ck.EntityTypeName));
						} else {
							// Add command
							_commands.Add(ck, command);
						}
					} // while loop
				} // using commandReader
			} finally {
				if (commandCmd != null) commandCmd.Dispose();
				if (paramCmd != null) paramCmd.Dispose();
			}
		}


		/// <summary>
		/// Loads all shape connections from the data store.
		/// </summary>
		protected void LoadShapeConnections(IStoreCache cache, Diagram diagram) {
			// If shape is new, there cannot be any connections in the database.
			if (((IEntity)diagram).Id == null) return;
			//
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				IDbCommand cmd = GetCommand(connectionEntityTypeName, RepositoryCommandType.SelectByOwnerId);
				((IDbDataParameter)cmd.Parameters[0]).Value = ((IEntity)diagram).Id;
				using (IDataReader dataReader = cmd.ExecuteReader()) {
					ShapeConnection sc;
					while (dataReader.Read()) {
						sc.ConnectorShape = cache.GetShape(dataReader.GetInt32(0));
						sc.GluePointId = dataReader.GetInt32(1);
						sc.TargetShape = cache.GetShape(dataReader.GetInt32(2)); // TODO 3: Error handling?
						sc.TargetPointId = dataReader.GetInt32(3);
						sc.ConnectorShape.Connect(sc.GluePointId, sc.TargetShape, sc.TargetPointId);
					}
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}


		/// <summary>
		/// Closes the connection to the database.
		/// </summary>
		protected bool EnsureDataSourceOpen() {
			bool result;
			if (Connection.State != ConnectionState.Open) {
				Connection.Open();
				result = true;
			} else result = false;
			return result;
		}


		/// <summary>
		/// Opens the connection to the database if not yet opened.
		/// </summary>
		protected void EnsureDataSourceClosed() {
			Connection.Close();
		}


		/// <summary>
		/// Opens the store for read/write access.
		/// </summary>
		protected void OpenCore(IStoreCache cache, bool create) {
			// Check if store is already open
			if (_commands.Count > 0) throw new InvalidOperationException(string.Format(ResourceStrings.MessageFmt_0IsAlreadyOpen, GetType().Name));
			bool dataSourceOpened = EnsureDataSourceOpen();
			try {
				LoadSysCommands();
				if (!create) {
					int ver = DoReadVersion(cache.ProjectName);
					Debug.Assert(ver == _version);
				}
			} finally {
				if (dataSourceOpened) EnsureDataSourceClosed();
			}
		}

		#endregion


		#region [Protected] Methods: Load objects from database

		// This is the actual reading function and will go into a separate data access layer in later versions.
		/// <summary>
		/// Loads objects of the specified entity type into the given entity buffer using the given command and filter.
		/// </summary>
		protected IEnumerable<EntityBucket<TEntity>> LoadEntities<TEntity>(IStoreCache cache,
			IEntityType entityType, IdFilter idFilter, Resolver parentResolver, RepositoryCommandType cmdType,
			params object[] parameters) where TEntity : IEntity {
			//
			// We must store all loaded entities for loading of inner objects
			// TODO 2: Now with MARS, we could reunite the two phases.
			List<EntityBucket<TEntity>> newEntities = new List<EntityBucket<TEntity>>(1000);
			//
			bool connectionOpened = EnsureDataSourceOpen();
			DbParameterReader repositoryReader = null;
			int version = entityType.RepositoryVersion;
			try {
				IDbCommand cmd = GetCommand(entityType.FullName, cmdType);
				for (int i = 0; i < parameters.Length; ++i)
					((IDbDataParameter)cmd.Parameters[i]).Value = parameters[i];
				//
				IDataReader dataReader = cmd.ExecuteReader();
				try {
					repositoryReader = new DbParameterReader(this, cache);
					repositoryReader.ResetFieldReading(entityType.PropertyDefinitions, dataReader);
					while (repositoryReader.DoBeginObject()) {
						object id = repositoryReader.ReadId();
						if (idFilter(id)) {
							// Read the fields
							object parentId = repositoryReader.ReadId();
							TEntity entity = entityType.CreateInstanceForLoading<TEntity>();
							entity.AssignId(id);
							entity.LoadFields(repositoryReader, version);
							int pix = 0;
							// Read the composite inner objects
							foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
								if (pi is EntityInnerObjectsDefinition && IsComposition(pi)) {
									// property index -1 is id. LoadInnerObjects will increment the PropertyIndex.
									repositoryReader.PropertyIndex = pix - 1;
									entity.LoadInnerObjects(pi.Name, repositoryReader, version);
								}
								++pix;
							}
							newEntities.Add(new EntityBucket<TEntity>(entity, parentResolver(parentId), ItemState.Original));
						}
					}
				} finally {
					dataReader.Close();
				}
				// Read the associated inner objects
				if (entityType.HasInnerObjects) {
					repositoryReader.ResetInnerObjectsReading();
					foreach (EntityBucket<TEntity> eb in newEntities) {
						repositoryReader.PrepareInnerObjectsReading(eb.ObjectRef);
						int pix = 0;
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
							if (pi is EntityInnerObjectsDefinition && !IsComposition(pi)) {
								repositoryReader.PropertyIndex = pix - 1;
								eb.ObjectRef.LoadInnerObjects(pi.Name, repositoryReader, version);
							}
							++pix;
						}
					}
				}
			} finally {
				if (repositoryReader != null) repositoryReader.Dispose();
				if (connectionOpened) EnsureDataSourceClosed();
			}
			foreach (EntityBucket<TEntity> eb in newEntities)
				yield return eb;
		}

		#endregion


		#region [Protected] Methods: Save objects to database

		/// <summary>
		/// Inserts new objects into the data store.
		/// </summary>
		protected void InsertEntities<TEntity>(IStoreCache cache, IEntityType entityType,
			IEnumerable<KeyValuePair<TEntity, IEntity>> newEntities,
			FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			InsertEntities<TEntity>(cache, entityType, newEntities,
				GetCommand(entityType.FullName, RepositoryCommandType.Insert), filterDelegate);
		}


		/// <summary>
		/// Inserts new objects into the data store.
		/// </summary>
		protected virtual void InsertEntities<TEntity>(IStoreCache cache,
			IEntityType entityType, IEnumerable<KeyValuePair<TEntity, IEntity>> newEntities,
			IDbCommand dbCommand, FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			// We must remove the new entities from the new dictionary and enter them to 
			// the loaded dictionary afterwards.
			DbParameterWriter repositoryWriter = null;
			foreach (KeyValuePair<TEntity, IEntity> e in newEntities) {
				// Filter out the entities we do not need
				if (filterDelegate != null && !filterDelegate(e.Key, e.Value)) continue;
				//
				// Create repositoryWriter if this is the first new object
				if (repositoryWriter == null) {
					repositoryWriter = new DbParameterWriter(this, cache);
					repositoryWriter.Reset(entityType.PropertyDefinitions);
					repositoryWriter.Command = dbCommand;
					repositoryWriter.Command.Transaction = _transaction;
				}
				repositoryWriter.Prepare(e.Key);
				// Write parent id (only ProjectInfo and Design have none)
				if (e.Value != null) repositoryWriter.WriteId(e.Value.Id);
				e.Key.SaveFields(repositoryWriter, entityType.RepositoryVersion);
				int pix = 0;
				// Save all the composite inner objects.
				foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
					if (pi is EntityInnerObjectsDefinition && IsComposition(pi)) {
						repositoryWriter.PropertyIndex = pix - 1;
						e.Key.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
					}
					++pix;
				}
				repositoryWriter.Flush();
				// Now save all the non-composite inner objects.
				pix = 0;
				foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
					if (pi is EntityInnerObjectsDefinition && !IsComposition(pi)) {
						repositoryWriter.PropertyIndex = pix - 1;
						e.Key.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
					}
					++pix;
				}
			}
			repositoryWriter = null;
		}


		/// <summary>
		/// Erases the deleted connections from the database.
		/// </summary>
		protected virtual void DeleteShapeConnections(IStoreCache cache) {
			// Delete command need not be defined if no deleted shape connections exist.
			IDbCommand command = GetCommand(connectionEntityTypeName, RepositoryCommandType.Delete);
			command.Transaction = _transaction;
			foreach (ShapeConnection sc in cache.DeletedShapeConnections) {
				((IDataParameter)command.Parameters[0]).Value = ((IEntity)sc.ConnectorShape).Id;
				((IDataParameter)command.Parameters[1]).Value = sc.GluePointId;
				command.ExecuteNonQuery();
			}
		}


		/// <summary>
		/// Inserts new shape connections into the data store.
		/// </summary>
		protected virtual void InsertShapeConnections(IStoreCache storeCache) {
			IDbCommand command = GetCommand(connectionEntityTypeName, RepositoryCommandType.Insert);
			command.Transaction = _transaction;
			foreach (ShapeConnection sc in storeCache.NewShapeConnections) {
				((IDataParameter)command.Parameters[0]).Value = ((IEntity)sc.ConnectorShape).Id;
				((IDataParameter)command.Parameters[1]).Value = sc.GluePointId;
				((IDataParameter)command.Parameters[2]).Value = ((IEntity)sc.TargetShape).Id;
				((IDataParameter)command.Parameters[3]).Value = sc.TargetPointId;
				command.ExecuteNonQuery();
			}
		}


		/// <summary>
		/// Updates the modified entities against the ADO.NET data provider.
		/// </summary>
		protected virtual void UpdateEntities<TEntity>(IStoreCache cache,
			IEntityType entityType, IEnumerable<EntityBucket<TEntity>> loadedEntities,
			FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			List<object> modifiedIds = new List<object>(100);
			DbParameterWriter repositoryWriter = null;
			foreach (EntityBucket<TEntity> ei in loadedEntities) {
				// Filter out the entities we do not need
				if (filterDelegate != null && !filterDelegate(ei.ObjectRef, null)) continue;
				switch (ei.State) {
					// For OwnerChanged, we assume it was modified as well.
					case ItemState.OwnerChanged:
					case ItemState.Modified:
						// Create repositoryWriter if this is the first modified entity
						if (repositoryWriter == null) {
							repositoryWriter = new DbParameterWriter(this, cache);
							repositoryWriter.Command = GetCommand(entityType.FullName, RepositoryCommandType.Update);
							repositoryWriter.Command.Transaction = _transaction;
							repositoryWriter.Reset(entityType.PropertyDefinitions);
						}
						repositoryWriter.Prepare(ei.ObjectRef);
						repositoryWriter.WriteId(ei.ObjectRef.Id);
						// Update of owner is handled in the "UpdateOwnerXYZ" methods
						// repositoryWriter.WriteId(ei.Owner == null? null: ei.Owner.Id);
						ei.ObjectRef.SaveFields(repositoryWriter, entityType.RepositoryVersion);
						// Save all the composite inner objects.
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition && IsComposition(pi))
								ei.ObjectRef.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
						repositoryWriter.Flush();
						// Now save all the non-composite inner objects.
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition && !IsComposition(pi))
								ei.ObjectRef.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
						modifiedIds.Add(ei.ObjectRef.Id);
						break;
					case ItemState.Deleted:
					case ItemState.Original:
						continue;   // nothing to do
					default:
						throw new NShapeUnsupportedValueException(ei.State.GetType(), ei.State);
				}
			}
		}


		/// <summary>
		/// Erases deleted entities of type TEntity from the data store.
		/// </summary>
		protected virtual void DeleteEntities<TEntity>(IStoreCache cache,
			IEntityType entityType, IEnumerable<EntityBucket<TEntity>> loadedEntities,
			FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			// store id's of deleted shapes in this list and remove them after iterating with the IEnumerable enumerator
			DbParameterWriter repositoryWriter = null;
			foreach (EntityBucket<TEntity> ei in loadedEntities) {
				if (filterDelegate != null && !filterDelegate(ei.ObjectRef, null)) continue;
				if (ei.ObjectRef is TEntity) {
					switch (ei.State) {
						case ItemState.Deleted:
							if (repositoryWriter == null) {
								repositoryWriter = new DbParameterWriter(this, cache);
								repositoryWriter.Reset(entityType.PropertyDefinitions);
								repositoryWriter.Command = GetCommand(entityType.FullName, RepositoryCommandType.Delete);
								repositoryWriter.Command.Transaction = _transaction;
							}
							repositoryWriter.Prepare(ei.ObjectRef);
							repositoryWriter.WriteId(ei.ObjectRef.Id);
							// Delete all non-composition inner objects
							int pix = 0;
							foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
								if (pi is EntityInnerObjectsDefinition && !IsComposition(pi)) {
									repositoryWriter.PropertyIndex = pix - 1;
									repositoryWriter.DeleteInnerObjects();
								}
								++pix;
							}
							repositoryWriter.Flush();
							break;
						case ItemState.OwnerChanged:
						case ItemState.Modified:
						case ItemState.Original:
							continue;
						default:
							throw new NShapeUnsupportedValueException(ei.State.GetType(), ei.State);
					}
				}
			}
		}


		/// <summary>
		/// Updates modified shape owners.
		/// </summary>
		protected void UpdateShapeOwners(IStoreCache cache) {
			foreach (EntityBucket<Shape> eb in cache.LoadedShapes) {
				if (eb.State == ItemState.OwnerChanged) {
					IDbCommand updateOwnerCmd;
					// For a new owner we assume parent shape. 
					if (eb.Owner == null || eb.Owner is Shape)
						updateOwnerCmd = GetCommand(shapeEntityTypeName, RepositoryCommandType.UpdateOwnerShape);
					else if (eb.Owner is Diagram)
						updateOwnerCmd = GetCommand(shapeEntityTypeName, RepositoryCommandType.UpdateOwnerDiagram);
					else {
						Debug.Fail("Unexpected owner in AdoNetRepository.Update!");
						updateOwnerCmd = null;
					}
					updateOwnerCmd.Transaction = _transaction;
					((IDataParameter)updateOwnerCmd.Parameters[0]).Value = ((IEntity)eb.ObjectRef).Id;
					((IDataParameter)updateOwnerCmd.Parameters[1]).Value = ((IEntity)eb.Owner).Id;
					updateOwnerCmd.ExecuteNonQuery();
				}
			}
		}

		#endregion


		#region [Protected] Types

		/// <summary>
		/// Defines the type of repository command and the associated entity name.
		/// </summary>
		protected struct CommandKey : IEquatable<CommandKey> {

			/// <summary>Specifies the kind of repository action, the command was designed for.</summary>
			public RepositoryCommandType Kind;

			/// <summary>Specifies the entity name the associated command is responsible for.</summary>
			public string EntityTypeName;

			/// <override></override>
			public override string ToString() {
				return EntityTypeName + "." + Kind.ToString();
			}

			/// <override></override>
			public bool Equals(CommandKey other) {
				return (other.EntityTypeName == this.EntityTypeName
					&& other.Kind == this.Kind);
			}
		}


		/// <summary>
		/// A set of commands for loading and saving entities from and to the database.
		/// </summary>
		protected struct CommandSet : IEquatable<CommandSet> {

			/// <summary>
			/// Defines an ampty CommandSet.
			/// </summary>
			static public CommandSet Empty {
				get {
					CommandSet result;
					result.DeleteCommand = null;
					result.InsertCommand = null;
					result.SelectByIdCommand = null;
					result.SelectByNameCommand = null;
					result.SelectIdsCommand = null;
					result.SelectByOwnerIdCommand = null;
					result.UpdateCommand = null;
					return result;
				}
			}


			/// <summary>
			/// Associates the given command with the given repository action
			/// </summary>
			public void SetCommand(RepositoryCommandType type, IDbCommand command) {
				if (command == null) throw new ArgumentNullException(nameof(command));
				switch (type) {
					case RepositoryCommandType.SelectAll:
						SelectIdsCommand = command; break;
					case RepositoryCommandType.SelectById:
						SelectByIdCommand = command; break;
					case RepositoryCommandType.SelectByName:
						SelectByNameCommand = command; break;
					case RepositoryCommandType.SelectByOwnerId:
						SelectByOwnerIdCommand = command; break;
					case RepositoryCommandType.Update:
						UpdateCommand = command; break;
					case RepositoryCommandType.Delete:
						DeleteCommand = command; break;
					case RepositoryCommandType.Insert:
						InsertCommand = command; break;
					default:
						Debug.Fail("Unsupported command type");
						break;
				}
			}


			/// <summary>
			/// Retrieves the command associated with the given repository action
			/// </summary>
			public IDbCommand GetCommand(RepositoryCommandType commandType) {
				switch (commandType) {
					case RepositoryCommandType.Delete:
						return DeleteCommand;
					case RepositoryCommandType.Insert:
						return InsertCommand;
					case RepositoryCommandType.SelectAll:
						return SelectIdsCommand;
					case RepositoryCommandType.SelectById:
						return SelectByIdCommand;
					case RepositoryCommandType.SelectByOwnerId:
						return SelectByOwnerIdCommand;
					case RepositoryCommandType.SelectByName:
						return SelectByNameCommand;
					case RepositoryCommandType.Update:
						return UpdateCommand;
					default:
						throw new NShapeUnsupportedValueException(typeof(RepositoryCommandType), commandType);
				}
			}


			/// <override></override>
			public bool Equals(CommandSet other) {
				return (other.DeleteCommand == this.DeleteCommand
					&& other.InsertCommand == this.InsertCommand
					&& other.SelectByIdCommand == this.SelectByIdCommand
					&& other.SelectByNameCommand == this.SelectByNameCommand
					&& other.SelectByOwnerIdCommand == this.SelectByOwnerIdCommand
					&& other.SelectIdsCommand == this.SelectIdsCommand
					&& other.UpdateCommand == this.UpdateCommand);
			}


			private IDbCommand SelectIdsCommand;
			private IDbCommand SelectByIdCommand;
			private IDbCommand SelectByNameCommand;
			private IDbCommand SelectByOwnerIdCommand;
			private IDbCommand InsertCommand;
			private IDbCommand UpdateCommand;
			private IDbCommand DeleteCommand;
		}

		#endregion


		#region [Protected] Types: DBCommandReader / DBCommandWriter

		/// <summary>
		/// A repository reader implementation reading data from of ADO.NET command parameters.
		/// </summary>
		protected class DbParameterReader : RepositoryReader, IDisposable {

			/// <summary>
			/// Initialized a new instance of <see cref="T:Dataweb.NShape.AdoNetStore.DBParameterReader" />.
			/// </summary>
			public DbParameterReader(AdoNetStore store, IStoreCache cache)
				: base(cache) {
				if (store == null) throw new ArgumentNullException(nameof(store));
				this._store = store;
			}


			/// <summary>
			/// Finalizer of <see cref="T:Dataweb.NShape.AdoNetStore.DBParameterReader" />.
			/// </summary>
			~DbParameterReader() {
				Dispose();
			}


			/// <summary>
			/// PreparesReading of the inner objects from the given entity.
			/// </summary>
			public void PrepareInnerObjectsReading(IEntity entity) {
				if (entity == null) throw new ArgumentNullException(nameof(entity));
				this.Object = entity;
				PropertyIndex = 0;
				foreach (EntityPropertyDefinition pi in PropertyInfos) {
					if (pi is EntityInnerObjectsDefinition) break;
					++PropertyIndex;
				}
			}


			#region [Protected] Methods

			/// <summary>
			/// Prepares the reader for reading the given properties of an enitity.
			/// The data reader is managed outside the DBParameterReader.
			/// </summary>
			protected internal void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos, IDataReader dataReader) {
				base.ResetFieldReading(propertyInfos);
				this._dataReader = dataReader;
			}


			/// <summary>
			/// Prepares the reader for reading inner objects of an enitity.
			/// </summary>
			protected internal void ResetInnerObjectsReading() {
				// base.Reset(propertyInfos);
			}


			/// <summary>
			/// The IDataReader providing the data.
			/// </summary>
			protected internal IDataReader DataReader {
				get { return _dataReader; }
				set { _dataReader = value; }
			}

			#endregion


			#region [Protected][Internal] Methods: RepositoryReader Implementation

			/// <override></override>
			protected internal override object ReadId() {
				++PropertyIndex;
				ValidateFieldIndex();
				object result = _dataReader.GetValue(PropertyIndex + internalPropertyCount);
				return Convert.IsDBNull(result) ? null : result;
			}


			/// <summary>
			/// Prepares the reader for reading fields of an entity. The first fields to read internally are the id and the parent id. Therefore we start at index -2.
			/// </summary>
			protected internal override bool DoBeginObject() {
				Debug.Assert(_dataReader != null);
				if (_dataReader.Read()) PropertyIndex = -internalPropertyCount - 1;
				else PropertyIndex = int.MinValue;
				return PropertyIndex > int.MinValue;
			}


			/// <summary>
			/// Finishes reading fields of an entity. The first fields to read internally are the id and the parent id. Therefore we start at index -2.
			/// </summary>
			protected internal override void DoEndObject() {
				// Nothing to do
			}

			#endregion


			#region [Protected] Methods: RepositoryReader Implementation


			/// <override></override>
			protected override bool DoReadBool() {
				return _dataReader.GetBoolean(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override byte DoReadByte() {
				return _dataReader.GetByte(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override short DoReadInt16() {
				return _dataReader.GetInt16(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override int DoReadInt32() {
				return _dataReader.GetInt32(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override long DoReadInt64() {
				return _dataReader.GetInt64(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override float DoReadFloat() {
				return _dataReader.GetFloat(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override double DoReadDouble() {
				return _dataReader.GetDouble(PropertyIndex + internalPropertyCount);
			}


			/// <override></override>
			protected override char DoReadChar() {
				// SqlDataReader.GetChar is not implemented and _always_ throws a NotSupportedException()
				//return dataReader.GetChar(PropertyIndex + internalPropertyCount);
				return Char.Parse(_dataReader.GetString(PropertyIndex + internalPropertyCount));
			}


			/// <override></override>
			protected override string DoReadString() {
				string result = string.Empty;
				if (!_dataReader.IsDBNull(PropertyIndex + internalPropertyCount))
					result = _dataReader.GetString(PropertyIndex + internalPropertyCount);
				return result;
			}


			/// <override></override>
			protected override DateTime DoReadDate() {
				DateTime result = _dataReader.GetDateTime(PropertyIndex + internalPropertyCount).ToLocalTime();
				return result;
			}


			/// <override></override>
			protected override Image DoReadImage() {
				Image result = null;
				if (!_dataReader.IsDBNull(PropertyIndex + internalPropertyCount)) {
					byte[] buffer = new Byte[_dataReader.GetBytes(PropertyIndex + internalPropertyCount, 0, null, 0, 0)];
					_dataReader.GetBytes(PropertyIndex + internalPropertyCount, 0, buffer, 0, buffer.Length);
					if (buffer.Length > 0) {
						MemoryStream stream = new MemoryStream(buffer, false);
						result = Image.FromStream(stream);
					}
				}
				return result;
			}


			/// <override></override>
			public override void BeginReadInnerObjects() {
				Debug.Assert(innerObjectsReader == null);
				++PropertyIndex;
				EntityInnerObjectsDefinition innerInfo = (EntityInnerObjectsDefinition)propertyInfos[PropertyIndex];
				// TODO 3: Replace by generic mechanism once the string reader is established
				if (AdoNetStore.IsComposition(innerInfo)) {
					innerObjectsReader = new StringReader(Cache);
					((StringReader)innerObjectsReader).ResetFieldReading(innerInfo.PropertyDefinitions, _dataReader.GetString(PropertyIndex + internalPropertyCount));
				} else {
					IDbCommand cmd = _store.GetCommand(innerInfo.EntityTypeName, RepositoryCommandType.SelectById);
					cmd.Transaction = _store.CurrentTransaction;
					((IDataParameter)cmd.Parameters[0]).Value = Object.Id;
					_innerObjectsDataReader = cmd.ExecuteReader();
					cmd.Dispose(); // Geht das?
					innerObjectsReader = new DbParameterReader(_store, Cache);
					InnerObjectsReader.ResetFieldReading(innerInfo.PropertyDefinitions, _innerObjectsDataReader);
				}
			}


			/// <override></override>
			public override void EndReadInnerObjects() {
				Debug.Assert(innerObjectsReader != null);
				if (_innerObjectsDataReader != null) {
					_innerObjectsDataReader.Dispose();
					_innerObjectsDataReader = null;
				}
				innerObjectsReader.Dispose();
				innerObjectsReader = null;
			}


			/// <override></override>
			public override void EndReadInnerObject() {
				innerObjectsReader.DoEndObject();
			}


			/// <override></override>
			protected override void Dispose(bool disposing) {
				if (_innerObjectsDataReader != null) {
					_innerObjectsDataReader.Dispose();
					_innerObjectsDataReader = null;
				}
			}

			#endregion


			#region [Private] Methods

			private void ValidateFieldIndex() {
				if (PropertyIndex >= _dataReader.FieldCount)
					throw new InvalidOperationException(ResourceStrings.MessageTxt_AnObjectTriesToLoadMorePropertiesThanTheEntityIsDefined);
			}


			private DbParameterReader InnerObjectsReader {
				get { return (DbParameterReader)innerObjectsReader; }
			}

			#endregion


			#region Fields

			// Id and parent id are always inside the cache.
			private const int internalPropertyCount = 2;

			private AdoNetStore _store;

			// Reference to an outside-created data repositoryReader for fields reading
			private IDataReader _dataReader;

			// Owned data repositoryReader for the fields of inner objects read by additional cache repositoryReader.
			private IDataReader _innerObjectsDataReader;

			#endregion

		}


		/// <summary>
		/// A repository writer implementation writing data to ADO.NET command parameters.
		/// </summary>
		protected class DbParameterWriter : RepositoryWriter {

			/// <summary>
			/// Initializes a new instance of <see cref="T:Dataweb.NShape.AdoNetStore.DBParameterWriter" />.
			/// </summary>
			/// <param name="store"></param>
			/// <param name="cache"></param>
			public DbParameterWriter(AdoNetStore store, IStoreCache cache)
				: base(cache) {
				this._store = store;
			}


			/// <summary>
			/// Specifies the IDbCommand providing the parameters for the DbParameterWriter.
			/// </summary>
			public IDbCommand Command {
				get { return _command; }
				set { _command = value; }
			}


			#region [Protected Internal] Methods

			/// <summary>
			/// Commits the changes to the database.
			/// </summary>
			internal protected void Flush() {
				if (Entity == null) {
					// Writing an inner object
					_command.ExecuteNonQuery();
				} else if (Entity.Id == null) {
					// Inserting an entity
					Entity.AssignId(_command.ExecuteScalar());
				} else {
					// Updating an entity
					object result = _command.ExecuteNonQuery();
					if (result == null)
						throw new Exception(string.Format(ResourceStrings.MessageFmt_NoRecordsAffectedByStatement01, Environment.NewLine, _command.CommandText));
				}
			}


			/// <summary>
			/// Prepares the cache repositoryWriter for writing the fields of another entity.
			/// </summary>
			internal protected void PrepareFields() {
				// The first parameter is the parent id, which is automatically inserted and does
				// therefore not correspond to to a real property. We handle it as property index -1.
				PropertyIndex = -2;
			}


			/// <override></override>
			protected internal override void Reset(IEnumerable<EntityPropertyDefinition> propertyInfos) {
				base.Reset(propertyInfos);
			}


			/// <summary>
			/// This method has to be called before passing the repositoryWriter to the IPeristable object for saving.
			/// </summary>
			protected internal override void Prepare(IEntity entity) {
				if (entity == null) throw new ArgumentNullException(nameof(entity));
				base.Prepare(entity);
				//if (!commandIsPrepared && command != null) {
				//    command.Prepare();
				//    commandIsPrepared = true;
				//}
				PropertyIndex = -2;
			}


			/// <override></override>
			protected internal override void Finish() {
				Flush();
				base.Finish();
			}

			#endregion


			#region [Protected] RepositoryWriter Implementation

			/// <override></override>
			protected override void DoWriteId(object value) {
				if (value == null) value = DBNull.Value;
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteBool(bool value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteByte(byte value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt16(short value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt32(int value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt64(long value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteFloat(float value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteDouble(double value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteChar(char value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteString(string value) {
				if (value == null) DoWriteValue(DBNull.Value);
				else DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteDate(DateTime value) {
				DoWriteValue(value);
			}


			/// <override></override>
			protected override void DoWriteImage(Image value) {
				if (value == null) DoWriteValue(DBNull.Value);
				else {
					MemoryStream stream = new MemoryStream();
					ImageFormat format = value.RawFormat;
					// ArgumentNullException when trying to save a MemoryBitmap in a MemoryStream
					if (format.Guid == ImageFormat.MemoryBmp.Guid)
						format = ImageFormat.Png;
					try {
						if (value != null) value.Save(stream, format);
						DoWriteValue(stream.GetBuffer());
					} finally {
						stream.Close();
						stream.Dispose();
						stream = null;
					}
				}
			}


			// Advance to the next set of inner objects, erase the previous content, 
			// get the command and prepare for writing
			/// <override></override>
			protected override void DoBeginWriteInnerObjects() {
				++PropertyIndex;
				ValidateInnerObjectsIndex();
				if (!(PropertyInfos[PropertyIndex] is EntityInnerObjectsDefinition))
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_PropertyIsNotAnInnerObjectsProperty);
				//
				EntityInnerObjectsDefinition innerInfo = (EntityInnerObjectsDefinition)PropertyInfos[PropertyIndex];
				if (AdoNetStore.IsComposition(innerInfo)) {
					base.InnerObjectsWriter = new StringWriter(Cache);
					((StringWriter)base.InnerObjectsWriter).Reset(innerInfo.PropertyDefinitions);
				} else {
					DoDeleteInnerObjects();
					base.InnerObjectsWriter = new DbParameterWriter(_store, Cache);
					InnerObjectsWriter.Reset(((EntityInnerObjectsDefinition)PropertyInfos[PropertyIndex]).PropertyDefinitions);
					InnerObjectsWriter.Command = _store.GetCommand(((EntityInnerObjectsDefinition)PropertyInfos[PropertyIndex]).EntityTypeName, RepositoryCommandType.Insert);
					InnerObjectsWriter.Command.Transaction = _store._transaction;
				}
			}


			/// <override></override>
			protected override void DoEndWriteInnerObjects() {
				if (base.InnerObjectsWriter is StringWriter) {
					((IDataParameter)_command.Parameters[PropertyIndex + 1]).Value = ((StringWriter)base.InnerObjectsWriter).StringData;
				} else {
					// Nothing to do
				}
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObject() {
				// Advance to next object
				base.InnerObjectsWriter.Prepare(Entity);
				// An inner object has no id of its own but stores its parent id in the first property.
				if (base.InnerObjectsWriter is DbParameterWriter)
					base.InnerObjectsWriter.WriteId(Entity.Id);
			}


			/// <override></override>
			protected override void DoEndWriteInnerObject() {
				// Commit inner object to data store
				base.InnerObjectsWriter.Finish();
			}


			/// <override></override>
			protected override void DoDeleteInnerObjects() {
				ValidateInnerObjectsIndex();
				if (!(PropertyInfos[PropertyIndex] is EntityInnerObjectsDefinition)) throw new AdoNetStoreException(ResourceStrings.MessageTxt_PropertyIsNotAnInnerObjectsProperty);
				//
				// Delete all existing inner objects of the current persistable object			
				IDbCommand command = _store.GetCommand(((EntityInnerObjectsDefinition)PropertyInfos[PropertyIndex]).EntityTypeName, RepositoryCommandType.Delete);
				command.Transaction = _store.CurrentTransaction;
				((IDataParameter)command.Parameters[0]).Value = Entity.Id;
				int count = command.ExecuteNonQuery();
			}

			#endregion


			#region [Private] Properties and Methods

			private new DbParameterWriter InnerObjectsWriter {
				get { return (DbParameterWriter)base.InnerObjectsWriter; }
			}


			// Cannot check against the property count, because tables can contain additional 
			// columns, e.g for the parent id and the id.
			private void ValidateFieldIndex() {
				if (PropertyIndex >= _command.Parameters.Count)
					throw new AdoNetStoreException(MessageFmt_Field0OfEntityCannotBeWrittenNotEnoughParameters, PropertyIndex);
			}


			// Can check against the property count here, becuause there are no hidden inner objects.
			private void ValidateInnerObjectsIndex() {
				if (PropertyIndex >= PropertyInfos.Count)
					throw new AdoNetStoreException(ResourceStrings.MessageFmt_InnerObjects0OfEntityCannotBeWrittenNotEnoughPropertyDefs, PropertyIndex);
			}


			private void DoWriteValue(object value) {
				++PropertyIndex;
				ValidateFieldIndex();
				((IDataParameter)_command.Parameters[PropertyIndex + 1]).Value = value;
			}

			#endregion


			#region Fields

			private const string MessageFmt_Field0OfEntityCannotBeWrittenNotEnoughParameters = "Field '{0}' of entity cannot be written to the repository because the mapping contains not enough items. Check whether the SQL commands for this entity are correct.";

			private AdoNetStore _store;
			private IDbCommand _command;

			#endregion
		}

		#endregion


		#region [Protected] Types: StringReader / StringWriter

		/// <summary>
		/// A repository reader implementation reading data from a string.
		/// </summary>
		protected class StringReader : RepositoryReader {

			/// <summary>
			/// Initialzes a new instance of <see cref="T:Dataweb.NShape.AdoNetStore.StringReader" />.
			/// </summary>
			/// <param name="store"></param>
			public StringReader(IStoreCache store)
				: base(store) {
			}


			// Starts reading fields for one type of object
			/// <override></override>
			public void ResetFieldReading(IEnumerable<EntityPropertyDefinition> fieldInfos, string data) {
				base.ResetFieldReading(fieldInfos);
				if (data == null) throw new ArgumentNullException(nameof(data));
				_str = data;
				_pos = 0;
			}


			#region RepositoryReader Implementation

			/// <override></override>
			public override void BeginReadInnerObjects() {
				throw new NotSupportedException();
			}


			/// <override></override>
			public override void EndReadInnerObjects() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected internal override bool DoBeginObject() {
				// Current position is either a valid field start or end of string
				if (_pos < 0 || _pos > _str.Length) throw new InvalidOperationException("Unsupported string position");
				return _pos < _str.Length;
			}


			/// <override></override>
			protected internal override void DoEndObject() {
				if (_str[_pos] != AdoNetStore.CompositionSeparatorChar) throw new InvalidOperationException(ResourceStrings.MessageTxt_UnsupportedStringSemicolonExpected);
				++_pos;
			}


			/// <override></override>
			public override void EndReadInnerObject() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected internal override object ReadId() {
				object result = null;
				// read id type
				if (_pos < _str.Length && _str[_pos] == '(') {
					string typeDesc = string.Empty;
					++_pos; // skip '('
					while (_pos < _str.Length && _str[_pos] != ')') {
						typeDesc += _str[_pos];
						++_pos;
					}
					if (_pos < _str.Length && _str[_pos] == ')') ++_pos;

					if (typeDesc != string.Empty) {
						if (typeDesc == typeof(int).Name)
							result = DoReadInt32();
						else if (typeDesc == typeof(long).Name)
							result = DoReadInt64();
						else throw new NotSupportedException();
					}
					if (_pos < _str.Length && _str[_pos] == AdoNetStore.CompositionFieldSeparatorChar) ++_pos;
				}
				return result;
			}


			/// <override></override>
			protected override bool DoReadBool() {
				return (DoReadIntValue() != 0);
			}


			/// <override></override>
			protected override byte DoReadByte() {
				long value = DoReadIntValue();
				if (value < byte.MinValue || byte.MaxValue < value)
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				return (byte)value;
			}


			/// <override></override>
			protected override short DoReadInt16() {
				long value = DoReadIntValue();
				if (value < short.MinValue || short.MaxValue < value)
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				return (short)value;
			}


			/// <override></override>
			protected override int DoReadInt32() {
				long value = DoReadIntValue();
				if (value < int.MinValue || int.MaxValue < value)
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				return (int)value;
			}


			/// <override></override>
			protected override long DoReadInt64() {
				return DoReadIntValue();
			}


			/// <override></override>
			protected override float DoReadFloat() {
				double value = DoReadDblValue();
				if (value < float.MinValue || float.MaxValue < value)
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				return (float)value;
			}


			/// <override></override>
			protected override double DoReadDouble() {
				return DoReadDblValue();
			}


			/// <override></override>
			protected override char DoReadChar() {
				char value;
				if (!char.TryParse(DoReadStringValue(), out value))
					throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				return value;
			}


			/// <override></override>
			protected override string DoReadString() {
				return DoReadStringValue();
			}


			/// <override></override>
			protected override DateTime DoReadDate() {
				return DateTime.Parse(DoReadStringValue());
			}


			/// <override></override>
			protected override Image DoReadImage() {
				throw new NotSupportedException();
			}

			#endregion


			private long DoReadIntValue() {
				long result = 0;
				int startPos = _pos;
				if (_pos < _str.Length && (_str[_pos] == '-' || _str[_pos] == '+'))
					++_pos;
				while (_pos < _str.Length && _str[_pos] >= '0' && _str[_pos] <= '9')
					++_pos;
				result = long.Parse(_str.Substring(startPos, _pos - startPos), CultureInfo.InvariantCulture);
				if (_pos < _str.Length && _str[_pos] == AdoNetStore.CompositionFieldSeparatorChar)
					++_pos;
				return result;
			}


			private double DoReadDblValue() {
				double result = DoReadIntValue();
				if (_pos < _str.Length && _str[_pos] == '.') {
					++_pos;
					int startPos = _pos;
					while (_pos < _str.Length && _str[_pos] >= '0' && _str[_pos] <= '9')
						++_pos;
					string fracValueStr = _str.Substring(startPos, _pos - startPos);
					result += (long.Parse(fracValueStr, CultureInfo.InvariantCulture) / Math.Pow(10, fracValueStr.Length));
					if (_pos < _str.Length && _str[_pos] == AdoNetStore.CompositionFieldSeparatorChar)
						++_pos;
				}
				return result;
			}


			private string DoReadStringValue() {
				int separatorPos = _str.IndexOfAny(_separators, _pos);
				if (separatorPos < 0) throw new AdoNetStoreException(ResourceStrings.MessageTxt_InvalidRepositoryFormat);
				string result = Uri.UnescapeDataString(_str.Substring(_pos, separatorPos - _pos));
				_pos = separatorPos;
				return result;
			}


			// String data
			private string _str;
			// Current position within string data
			private int _pos;
			private char[] _separators = new char[2] { AdoNetStore.CompositionFieldSeparatorChar, AdoNetStore.CompositionSeparatorChar };
		}


		/// <summary>
		/// A repository writer implementation writing data to a string.
		/// </summary>
		protected class StringWriter : RepositoryWriter {

			/// <summary>
			/// Initializes a new instance of <see cref="T:Dataweb.NShape.AdoNetStore.StringWriter" />
			/// </summary>
			/// <param name="store"></param>
			public StringWriter(IStoreCache store)
				: base(store) {
			}


			/// <override></override>
			protected internal override void Reset(IEnumerable<EntityPropertyDefinition> fieldInfos) {
				base.Reset(fieldInfos);
			}


			/// <override></override>
			protected internal override void Prepare(IEntity entity) {
				base.Prepare(entity);
			}


			/// <override></override>
			protected internal override void Finish() {
				_str.Append(AdoNetStore.CompositionSeparatorChar);
				base.Finish();
			}


			/// <summary>
			/// Gets the data written by this <see cref="T:Dataweb.NShape.AdoNetStore.StringWriter" />.
			/// </summary>
			public string StringData {
				get { return _str.ToString(); }
			}


			/// <override></override>
			protected override void DoWriteId(object id) {
				++PropertyIndex;
				if (PropertyIndex >= 0) _str.Append(AdoNetStore.CompositionFieldSeparatorChar);
				_str.Append(string.Format("({0}){1}", id.GetType().Name, id));
			}


			/// <override></override>
			protected override void DoWriteBool(bool value) {
				DoWriteIntValue(value ? 1 : 0);
			}


			/// <override></override>
			protected override void DoWriteByte(byte value) {
				DoWriteIntValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt16(short value) {
				DoWriteIntValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt32(int value) {
				DoWriteIntValue(value);
			}


			/// <override></override>
			protected override void DoWriteInt64(long value) {
				DoWriteIntValue(value);
			}


			/// <override></override>
			protected override void DoWriteFloat(float value) {
				DoWriteDblValue(value);
			}


			/// <override></override>
			protected override void DoWriteDouble(double value) {
				DoWriteDblValue(value);
			}


			/// <override></override>
			protected override void DoWriteChar(char value) {
				DoWriteStrValue(value.ToString());
			}


			/// <override></override>
			protected override void DoWriteString(string value) {
				DoWriteStrValue(value);
			}


			/// <override></override>
			protected override void DoWriteDate(DateTime date) {
				DoWriteStrValue(date.ToString(CultureInfo.InvariantCulture));
			}


			/// <override></override>
			protected override void DoWriteImage(Image image) {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObjects() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected override void DoEndWriteInnerObjects() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObject() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected override void DoEndWriteInnerObject() {
				throw new NotSupportedException();
			}


			/// <override></override>
			protected override void DoDeleteInnerObjects() {
				throw new NotSupportedException();
			}


			private void DoWriteDblValue(double value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) _str.Append(AdoNetStore.CompositionFieldSeparatorChar);
				_str.Append(value.ToString(CultureInfo.InvariantCulture));
			}


			private void DoWriteIntValue(long value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) _str.Append(AdoNetStore.CompositionFieldSeparatorChar);
				_str.Append(value.ToString(CultureInfo.InvariantCulture));
			}


			private void DoWriteStrValue(string value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) _str.Append(AdoNetStore.CompositionFieldSeparatorChar);
				if (!string.IsNullOrEmpty(value)) _str.Append(Uri.EscapeDataString(value));
			}


			private StringBuilder _str = new StringBuilder();
		}

		#endregion


		private IDbConnection Connection {
			get {
				if (_connection == null) {
					if (string.IsNullOrEmpty(ProviderName))
						throw new AdoNetStoreException(ResourceStrings.MessageTxt_ProviderNameNotSet);
					if (string.IsNullOrEmpty(ConnectionString))
						throw new AdoNetStoreException(ResourceStrings.MessageTxt_ConnectionStringNotSet);
					_connection = _factory.CreateConnection();
					_connection.ConnectionString = ConnectionString;
				}
				return _connection;
			}
		}


		#region [Private] Methods

		private void AssertClosed() {
		}


		private void AssertOpen() {
			// TODO 3: Implement
		}


		private void AssertValid() {
			// TODO 3: Implement
		}


		private void ClearCommands() {
			foreach (KeyValuePair<CommandKey, IDbCommand> item in _commands)
				item.Value.Dispose();
			_commands.Clear();
		}


		private int DoReadVersion(string projectName) {
			// Read the project's repository version
			IDbCommand cmd = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.SelectByName);
			((IDataParameter)cmd.Parameters[0]).Value = projectName;
			using (IDataReader reader = cmd.ExecuteReader()) {
				if (!reader.Read()) throw new InvalidOperationException(string.Format("Project '{0}' not found in ADO.NET repository.", projectName));
				return reader.GetInt32(1);
			}
		}


		private void LoadChildShapes(IStoreCache cache, Shape s) {
			foreach (EntityType et in cache.EntityTypes) {
				if (et.Category == EntityCategory.Shape) {
					foreach (EntityBucket<Shape> sb in LoadEntities<Shape>(cache, et, id => true, pid => s, RepositoryCommandType.SelectChildShapes, ((IEntity)s).Id)) {
						cache.LoadedShapes.Add(sb);
						s.Children.Add(sb.ObjectRef);
					}
				}
			}
		}

		#endregion


		private class ResourceStrings {
			internal const string MessageFmt_0AlreadyDeletedFromTheRepository = "{0} already deleted from the repository.";
			internal const string MessageFmt_0AlreadyExistsInRepository = "{0} already exists in the repository.";
			internal const string MessageFmt_0IsAlreadyOpen = "{0} is already open.";
			internal const string MessageFmt_ADONETCommand0ForEntityType1HasNotBeenDefined = "ADO.NET command '{0}' for entity type '{1}' has not been defined.";
			internal const string MessageFmt_InnerObjects0OfEntityCannotBeWrittenNotEnoughPropertyDefs = "Inner objects '{0}' of entity cannot be written to the data store because the entity defines not enough properties.";
			internal const string MessageFmt_NoRecordsAffectedByStatement01 = "No Records affected by statement{0}{1}";
			internal const string MessageFmt_Template0HasMoreThanOneShape = "Template {0} has more than one shape.";
			internal const string MessageFmt_Template0HasNoShape = "Template {0} has no shape.";

			internal const string MessageTxt_AnObjectTriesToLoadMorePropertiesThanTheEntityIsDefined = "An object tries to load more properties than the entity is defined to have.";
			internal const string MessageTxt_CommandForCreatingTheSchemaIsNotDefined = "Command for creating the schema is not defined.";
			internal const string MessageTxt_ConnectionStringNotSet = "ConnectionString is not set.";
			internal const string MessageTxt_InvalidRepositoryFormat = "Invalid repository format.";
			internal const string MessageTxt_NoValidADONETProviderSpecified = "No valid ADO.NET provider specified.";
			internal const string MessageTxt_PropertyIsNotAnInnerObjectsProperty = "Property is not an inner objects property.";
			internal const string MessageTxt_ProviderNameNotSet = "ProviderName is not set.";
			internal const string MessageTxt_UnsupportedStringSemicolonExpected = "Unsupported string. ';' expected.";
		}


		#region Fields

		/// <summary>Entity type name of the project info.</summary>
		protected const string projectInfoEntityTypeName = "AdoNetRepository.ProjectInfo";
		/// <summary>Entity type name of the shape base class.</summary>
		protected const string shapeEntityTypeName = "Core.Shape";
		/// <summary>Entity type name of shape connections.</summary>
		protected const string connectionEntityTypeName = "Core.ShapeConnection";
		/// <summary>Entity type name of shape vertexes.</summary>
		protected const string vertexEntityTypeName = "Core.Vertex";
		/// <summary>Entity type name of shape connection points.</summary>
		protected const string connectionPointEntityTypeName = "Core.ConnectionPoint";


		private string _projectName;
		private string _providerName;
		private string _connectionString;
		private DbProviderFactory _factory;
		private IDbConnection _connection;
		private int _version;
		// Currently active transaction
		private IDbTransaction _transaction;
		private IDbCommand _createTablesCommand;
		// Commands used to load and save against the data store
		private Dictionary<CommandKey, IDbCommand> _commands = new Dictionary<CommandKey, IDbCommand>(1000);

		#endregion
	}

}
