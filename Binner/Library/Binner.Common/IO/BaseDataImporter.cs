using Binner.Common.Extensions;
using Binner.Global.Common;
using Binner.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    /// <summary>
    /// Base data importing functionality
    /// </summary>
    public class BaseDataImporter : IDataImporter
    {
        private readonly IStorageProvider _storageProvider;
        private readonly TemporaryKeyTracker _temporaryKeyTracker;

        public BaseDataImporter(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            _temporaryKeyTracker = new TemporaryKeyTracker();
        }

        public virtual Task<ImportResult> ImportAsync(string filename, Stream stream, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ImportResult> ImportAsync(IEnumerable<UploadFile> files, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsNullable(PropertyInfo property)
        {
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            var info = nullabilityInfoContext.Create(property);
            if (info.WriteState == NullabilityState.Nullable || info.ReadState == NullabilityState.Nullable)
            {
                return true;
            }

            return false;
        }

        protected virtual async Task AddProjectAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var projectId = values.GetValue("ProjectId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var existingProject = await _storageProvider.GetProjectAsync(name, userContext);
                if (existingProject == null)
                {
                    var project = new Project
                    {
                        Name = name,
                        GlobalId = globalId,
                        Description = GetQuoted(values.GetValue("Description").As<string?>()),
                        Location = GetQuoted(values.GetValue("Location").As<string?>()),
                        Color = values.GetValue("Color").As<int>(),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>()
                    };
                    try
                    {
                        project = await _storageProvider.AddProjectAsync(project, userContext);
                        _temporaryKeyTracker.AddKeyMapping("Projects", "ProjectId", projectId, project.ProjectId, globalId);
                        result.TotalRowsImported++;
                        result.RowsImportedByTable["Projects"]++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"[Row {rowNumber}, Project '{globalId}' with name '{name}' could not be added. Error: {ex.Message}");
                    }
                }
                else
                {
                    // add it to the tracker
                    if (existingProject != null)
                        _temporaryKeyTracker.AddKeyMapping("Projects", "ProjectId", projectId, existingProject.ProjectId, existingProject.GlobalId);
                    result.Warnings.Add($"[Row {rowNumber}] Project '{globalId}' with name '{name}' already exists.");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Skipping project '{globalId}' with no name.");
            }
        }

        protected virtual async Task AddPartTypeAsync(int rowNumber, Dictionary<string, object?> values, ICollection<PartType> partTypes, ImportResult result, IUserContext? userContext)
        {
            var partTypeId = values.GetValue("PartTypeId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                // part types need to have a unique name for the user and can not be part of global part types
                var existingPartType = partTypes.FirstOrDefault(x => x.Name?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true);
                if (existingPartType == null)
                {
                    var parentPartTypeId = values.GetValue("ParentPartTypeId").As<long?>();
                    if (parentPartTypeId == 0) parentPartTypeId = null;
                    var partType = new PartType
                    {
                        ParentPartTypeId = parentPartTypeId != null ? _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", parentPartTypeId.Value, false, false) : null,
                        Name = name,
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        Description = values.GetValue("Description").As<string?>(),
                        ReferenceDesignator = values.GetValue("ReferenceDesignator").As<string?>(),
                        SymbolId = values.GetValue("SymbolId").As<string?>(),
                        Keywords = values.GetValue("Keywords").As<string?>(),
                    };
                    partType = await _storageProvider.GetOrCreatePartTypeAsync(partType, userContext);
                    if (partType != null)
                    {
                        _temporaryKeyTracker.AddKeyMapping("PartTypes", "PartTypeId", partTypeId, partType.PartTypeId);
                        result.TotalRowsImported++;
                        result.RowsImportedByTable["PartTypes"]++;
                    }
                }
                else
                {
                    // add it to the tracker
                    if (existingPartType != null)
                        _temporaryKeyTracker.AddKeyMapping("PartTypes", "PartTypeId", partTypeId, existingPartType.PartTypeId);
                    result.Warnings.Add($"[Row {rowNumber}] PartType with name '{name}' already exists.");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Skipping PartType with no name.");
            }
        }

        protected virtual async Task AddPartAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var partNumber = values.GetValue("PartNumber").As<string?>() ?? string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(partNumber))
                {
                    var partId = values.GetValue("PartId").As<long>();
                    var existingPart = await _storageProvider.GetPartAsync(partNumber, userContext);
                    if (existingPart == null)
                    {
                        var partTypeId = _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", values.GetValue("PartTypeId").As<long>(), true, false);
                        if (partTypeId == null) partTypeId = (long)SystemDefaults.DefaultPartTypes.Other;
                        var projectId = _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", values.GetValue("ProjectId").As<long?>(), true, false);
                        var part = new Part
                        {
                            PartTypeId = partTypeId.Value,
                            ShortId = values.GetValue("ShortId").As<string?>() ?? ShortIdGenerator.Generate(),
                            GlobalId = globalId,
                            Value = values.GetValue("Value").As<string?>(),
                            BinNumber = values.GetValue("BinNumber").As<string?>(),
                            BinNumber2 = values.GetValue("BinNumber2").As<string?>(),
                            Cost = values.GetValue("Cost").As<decimal>(),
                            DatasheetUrl = values.GetValue("DatasheetUrl").As<string?>(),
                            Description = values.GetValue("Description").As<string?>(),
                            DigiKeyPartNumber = values.GetValue("DigiKeyPartNumber").As<string?>(),
                            MouserPartNumber = values.GetValue("MouserPartNumber").As<string?>(),
                            ArrowPartNumber = values.GetValue("ArrowPartNumber").As<string?>(),
                            TmePartNumber = values.GetValue("TmePartNumber").As<string?>(),
                            Element14PartNumber = values.GetValue("Element14PartNumber").As<string?>(),
                            ImageUrl = values.GetValue("ImageUrl").As<string?>(),
                            //Keywords = !string.IsNullOrEmpty(values.GetValue("Keywords").As<string?>()) ? values.GetValue("Keywords").As<string>().Split([","," "], StringSplitOptions.RemoveEmptyEntries) : null,
                            Keywords = values.GetValue("Keywords").As<ICollection<string>>(),
                            Location = values.GetValue("Location").As<string?>(),
                            LowestCostSupplier = values.GetValue("LowestCostSupplier").As<string?>(),
                            LowestCostSupplierUrl = values.GetValue("LowestCostSupplierUrl").As<string?>(),
                            LowStockThreshold = values.GetValue("LowStockThreshold").As<int>(),
                            Manufacturer = values.GetValue("Manufacturer").As<string?>(),
                            ManufacturerPartNumber = values.GetValue("ManufacturerPartNumber").As<string?>(),
                            MountingTypeId = values.GetValue("MountingTypeId").As<int>(),
                            PackageType = values.GetValue("PackageType").As<string?>(),
                            PartNumber = partNumber,
                            ProductUrl = values.GetValue("ProductUrl").As<string?>(),
                            ProjectId = projectId,
                            Quantity = values.GetValue("Quantity").As<long>(),
                            DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                            DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                            Currency = values.GetValue("Currency").As<string?>(),
                            ExtensionValue1 = values.GetValue("ExtensionValue1").As<string?>(),
                            ExtensionValue2 = values.GetValue("ExtensionValue2").As<string?>(),
                            FootprintName = values.GetValue("FootprintName").As<string?>(),
                            SymbolName = values.GetValue("Symbol").As<string?>(),
                            BaseProductNumber = values.GetValue("BaseProductNumber").As<string?>(),
                            ExportControlClassNumber = values.GetValue("ExportControlClassNumber").As<string?>(),
                            HtsusCode = values.GetValue("HtsusCode").As<string?>(),
                            LeadTime = values.GetValue("LeadTime").As<string?>(),
                            MoistureSensitivityLevel = values.GetValue("MoistureSensitivityLevel").As<string?>(),
                            OtherNames = values.GetValue("OtherNames").As<string?>(),
                            ProductStatus = values.GetValue("ProductStatus").As<string?>(),
                            RohsStatus = values.GetValue("RohsStatus").As<string?>(),
                            ReachStatus = values.GetValue("ReachStatus").As<string?>(),
                            Series = values.GetValue("Series").As<string?>(),
                            DataSource = PartDataSources.DataImport,
                        };
                        // some data validation required
                        if (part.ProjectId == 0) part.ProjectId = null;
                        if (part.UserId == 0) part.UserId = userContext?.UserId;
                        if (part.PartTypeId == 0) part.PartTypeId = (long)SystemDefaults.DefaultPartTypes.Other;

                        part = await _storageProvider.AddPartAsync(part, userContext);
                        _temporaryKeyTracker.AddKeyMapping("Parts", "PartId", partId, part.PartId, globalId);
                        result.TotalRowsImported++;
                        result.RowsImportedByTable["Parts"]++;

                    }
                    else
                    {
                        // add it to the tracker
                        if (existingPart != null)
                            _temporaryKeyTracker.AddKeyMapping("Parts", "PartId", partId, existingPart.PartId, existingPart.GlobalId);
                        result.Warnings.Add($"[Row {rowNumber}] Part '{globalId}' with PartNumber '{partNumber}' already exists.");
                    }
                }
                else
                {
                    result.Warnings.Add($"[Row {rowNumber}] Skipping Part with no name.");
                }
            }
            catch (Exception ex)
            {
                // failed to add part
                result.Errors.Add($"[Row {rowNumber}, Part '{globalId}' with PartNumber '{partNumber}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual async Task AddPartModelsAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var name = GetQuoted(values.GetValue("Name").As<string>());
            try
            {
                var partModelId = values.GetValue("PartModelId").As<long>();
                var partId = _temporaryKeyTracker.GetMappedId("Parts", "PartId", values.GetValue("PartId").As<long>(), true, false);
                if (partId != null)
                {
                    var partModel = new PartModel
                    {
                        PartId = partId.Value,
                        GlobalId = globalId,
                        Name = name ?? string.Empty,
                        Filename = GetQuoted(values.GetValue("Filename").As<string?>()),
                        ModelType = (PartModelTypes)values.GetValue("ModelType").As<int>(),
                        Source = (PartModelSources)values.GetValue("Source").As<int>(),
                        Url = GetQuoted(values.GetValue("Url").As<string?>()),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                    };
                    partModel = await _storageProvider.AddPartModelAsync(partModel, userContext);
                    _temporaryKeyTracker.AddKeyMapping("PartModels", "PartModelId", partModelId, partModel.PartModelId, globalId);
                    result.TotalRowsImported++;
                    result.RowsImportedByTable["PartModels"]++;
                }
                else
                {
                    result.Errors.Add($"[Row {rowNumber}] PartModel '{globalId}' with name '{name}' could not be added as the parent Part '{partId}' could not be found.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[Row {rowNumber}, PartModel '{globalId}' with name '{name}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual async Task AddPartParametricsAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var name = GetQuoted(values.GetValue("Name").As<string>());
            try
            {
                var partParametricId = values.GetValue("PartParametricId").As<long>();
                var partId = _temporaryKeyTracker.GetMappedId("Parts", "PartId", values.GetValue("PartId").As<long>(), true, false);
                if (partId != null)
                {
                    var partParametric = new PartParametric
                    {
                        PartId = partId.Value,
                        GlobalId = globalId,
                        Name = name ?? string.Empty,
                        Value = GetQuoted(values.GetValue("Value").As<string?>()) ?? string.Empty,
                        ValueNumber = values.GetValue("ValueNumber").As<decimal>(),
                        Units = (ParametricUnits)values.GetValue("Units").As<int>(),
                        DigiKeyParameterId = values.GetValue("DigiKeyParameterId").As<int>(),
                        DigiKeyParameterText = GetQuoted(values.GetValue("DigiKeyParameterText").As<string?>()),
                        DigiKeyParameterType = GetQuoted(values.GetValue("DigiKeyParameterType").As<string?>()),
                        DigiKeyValueId = GetQuoted(values.GetValue("DigiKeyValueId").As<string?>()),
                        DigiKeyValueText = GetQuoted(values.GetValue("DigiKeyValueText").As<string?>()),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                    };
                    partParametric = await _storageProvider.AddPartParametricAsync(partParametric, userContext);
                    _temporaryKeyTracker.AddKeyMapping("PartParametrics", "PartParametricId", partParametricId, partParametric.PartParametricId, globalId);
                    result.TotalRowsImported++;
                    result.RowsImportedByTable["PartParametrics"]++;
                }
                else
                {
                    result.Errors.Add($"[Row {rowNumber}] PartParametric '{globalId}' with name '{name}' could not be added as the parent Part '{partId}' could not be found.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[Row {rowNumber}, PartParametric '{globalId}' with name '{name}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual async Task AddCustomFieldsAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var customFieldId = values.GetValue("CustomFieldId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var existingCustomField = await _storageProvider.GetCustomFieldAsync(name, userContext);
                if (existingCustomField == null)
                {
                    var customField = new CustomField
                    {
                        GlobalId = globalId,
                        Name = name,
                        Description = GetQuoted(values.GetValue("Description").As<string?>()),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                    };
                    try
                    {
                        customField = await _storageProvider.AddCustomFieldAsync(customField, userContext);
                        _temporaryKeyTracker.AddKeyMapping("CustomFields", "CustomFieldId", customFieldId, customField.CustomFieldId, globalId);
                        result.TotalRowsImported++;
                        result.RowsImportedByTable["CustomFields"]++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"[Row {rowNumber}, CustomField '{globalId}' with name '{name}' could not be added. Error: {ex.Message}");
                    }
                }
                else
                {
                    // add it to the tracker
                    if (existingCustomField != null)
                        _temporaryKeyTracker.AddKeyMapping("CustomFields", "CustomFieldId", customFieldId, existingCustomField.CustomFieldId, existingCustomField.GlobalId);
                    result.Warnings.Add($"[Row {rowNumber}] CustomField '{globalId}' with name '{name}' already exists.");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Skipping CustomField with no name.");
            }
        }

        protected virtual async Task AddCustomFieldValuesAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            try
            {
                var customFieldValueId = values.GetValue("CustomFieldValueId").As<long>();
                var customFieldType = (CustomFieldTypes)values.GetValue("CustomFieldTypeId").As<int>();
                var customFieldId = _temporaryKeyTracker.GetMappedId("CustomFields", "CustomFieldId", values.GetValue("CustomFieldId").As<long>(), true, false);
                if (customFieldId != null)
                {
                    var customFieldValue = new CustomFieldValue
                    {
                        GlobalId = globalId,
                        CustomFieldId = customFieldId.Value,
                        CustomFieldTypeId = customFieldType,
                        //RecordId = _temporaryKeyTracker.GetMappedId("CustomFields", "CustomFieldId", values.GetValue("CustomFieldId").As<long>()),
                        Value = GetQuoted(values.GetValue("Value").As<string?>()),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                    };

                    // we need to map to the part in the database baed on the type of custom field
                    // note: this won't work on parts already in inventory, only ones added here as it requires the temp key tracker
                    switch (customFieldType)
                    {
                        case CustomFieldTypes.Inventory:
                            customFieldValue.RecordId = _temporaryKeyTracker.GetMappedId("Parts", "PartId", values.GetValue("RecordId").As<long>(), false, false) ?? 0;
                            break;
                        case CustomFieldTypes.Project:
                            customFieldValue.RecordId = _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", values.GetValue("RecordId").As<long>(), false, false) ?? 0;
                            break;
                        case CustomFieldTypes.User:
                            customFieldValue.RecordId = _temporaryKeyTracker.GetMappedId("Users", "UserId", values.GetValue("RecordId").As<long>(), false, false) ?? 0;
                            break;
                        case CustomFieldTypes.PartType:
                            customFieldValue.RecordId = _temporaryKeyTracker.GetMappedId("PartTypes", "PartTypeId", values.GetValue("RecordId").As<long>(), false, false) ?? 0;
                            break;
                    }
                    customFieldValue = await _storageProvider.AddCustomFieldValueAsync(customFieldValue, userContext);
                    _temporaryKeyTracker.AddKeyMapping("CustomFieldValues", "CustomFieldValueId", customFieldValueId, customFieldValue.CustomFieldValueId, globalId);
                    result.TotalRowsImported++;
                    result.RowsImportedByTable["CustomFieldValues"]++;
                }
                else
                {
                    result.Errors.Add($"[Row {rowNumber}, CustomFieldValue '{globalId}' could not be added as the parent CustomField was not found.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[Row {rowNumber}, CustomFieldValue '{globalId}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual async Task AddPcbAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            var pcbId = values.GetValue("PcbId").As<long>();
            var name = GetQuoted(values.GetValue("Name").As<string>())?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                var existingPcb = await _storageProvider.GetPcbAsync(name, userContext);
                if (existingPcb == null)
                {
                    var pcb = new Pcb
                    {
                        GlobalId = globalId,
                        Name = name,
                        Description = GetQuoted(values.GetValue("Description").As<string?>()),
                        SerialNumberFormat = GetQuoted(values.GetValue("SerialNumberFormat").As<string?>()),
                        LastSerialNumber = GetQuoted(values.GetValue("LastSerialNumber").As<string?>()),
                        Quantity = values.GetValue("LastSerialNumber").As<int>(),
                        Cost = values.GetValue("Cost").As<float>(),
                        DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                        DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                    };
                    try
                    {
                        pcb = await _storageProvider.AddPcbAsync(pcb, userContext);
                        _temporaryKeyTracker.AddKeyMapping("Pcbs", "PcbId", pcbId, pcb.PcbId, globalId);
                        result.TotalRowsImported++;
                        result.RowsImportedByTable["Pcbs"]++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"[Row {rowNumber}, Pcb '{globalId}' with name '{name}' could not be added. Error: {ex.Message}");
                    }
                }
                else
                {
                    // add it to the tracker
                    if (existingPcb != null)
                        _temporaryKeyTracker.AddKeyMapping("Pcbs", "PcbId", pcbId, existingPcb.PcbId, existingPcb.GlobalId);
                    result.Warnings.Add($"[Row {rowNumber}] Pcb '{globalId}' with name '{name}' already exists.");
                }
            }
            else
            {
                result.Warnings.Add($"[Row {rowNumber}] Skipping Pcb '{globalId}' with no name.");
            }
        }

        protected virtual async Task AddProjectPcbAssignmentAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            try
            {
                var projectPcbAssignmentId = values.GetValue("ProjectPcbAssignmentId").As<long>();
                var projectId = _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", values.GetValue("ProjectId").As<long>(), true, true);
                var pcbId = _temporaryKeyTracker.GetMappedId("Pcbs", "PcbId", values.GetValue("PcbId").As<long>(), true, true);
                var projectPcbAssignment = new ProjectPcbAssignment
                {
                    GlobalId = globalId,
                    ProjectId = projectId.Value,
                    PcbId = pcbId.Value,
                    DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                    DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                };
                projectPcbAssignment = await _storageProvider.AddProjectPcbAssignmentAsync(projectPcbAssignment, userContext);
                _temporaryKeyTracker.AddKeyMapping("ProjectPcbAssignments", "ProjectPcbAssignmentId", projectPcbAssignmentId, projectPcbAssignment.ProjectPcbAssignmentId, globalId);
                result.TotalRowsImported++;
                result.RowsImportedByTable["ProjectPcbAssignments"]++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[Row {rowNumber}, ProjectPcbAssignment '{globalId}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual async Task AddProjectPartAssignmentAsync(int rowNumber, Dictionary<string, object?> values, ImportResult result, IUserContext? userContext)
        {
            var globalId = values.GetValue("GlobalId").As<Guid>();
            try
            {
                var projectPartAssignmentId = values.GetValue("ProjectPcbAssignmentId").As<long>();
                var projectId = _temporaryKeyTracker.GetMappedId("Projects", "ProjectId", values.GetValue("ProjectId").As<long>(), true, true);
                var pcbId = values.GetValue("PcbId").As<long?> != null ? _temporaryKeyTracker.GetMappedId("Pcbs", "PcbId", values.GetValue("PcbId").As<long>()) : null;
                var partId = values.GetValue("PartId").As<long?>() != null ? _temporaryKeyTracker.GetMappedId("Parts", "PartId", values.GetValue("PartId").As<long>()) : null;
                var projectPartAssignment = new ProjectPartAssignment
                {
                    GlobalId = globalId,
                    ProjectId = projectId.Value,
                    PcbId = pcbId,
                    PartId = partId,
                    PartName = GetQuoted(values.GetValue("PartName").As<string?>()),
                    Quantity = values.GetValue("Quantity").As<int>(),
                    QuantityAvailable = values.GetValue("QuantityAvailable").As<int>(),
                    Notes = GetQuoted(values.GetValue("Notes").As<string?>()),
                    ReferenceId = GetQuoted(values.GetValue("ReferenceId").As<string?>()),
                    SchematicReferenceId = GetQuoted(values.GetValue("SchematicReferenceId").As<string?>()),
                    CustomDescription = GetQuoted(values.GetValue("CustomDescription").As<string?>()),
                    Cost = values.GetValue("Cost").As<float>(),
                    Currency = GetQuoted(values.GetValue("Currency").As<string?>()),
                    FootprintName = GetQuoted(values.GetValue("FootprintName").As<string?>()),
                    SymbolName = GetQuoted(values.GetValue("SymbolName").As<string?>()),
                    DateCreatedUtc = values.GetValue("DateCreatedUtc").As<DateTime>(),
                    DateModifiedUtc = values.GetValue("DateModifiedUtc").As<DateTime>(),
                };
                projectPartAssignment = await _storageProvider.AddProjectPartAssignmentAsync(projectPartAssignment, userContext);
                _temporaryKeyTracker.AddKeyMapping("ProjectPartAssignments", "ProjectPartAssignmentId", projectPartAssignmentId, projectPartAssignment.ProjectPartAssignmentId, globalId);
                result.TotalRowsImported++;
                result.RowsImportedByTable["ProjectPartAssignments"]++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[Row {rowNumber}, ProjectPartAssignment '{globalId}' could not be added. Error: {ex.Message}");
            }
        }

        protected virtual void MapValue<T>(bool isSuccess, T? value, PropertyInfo property, ref Dictionary<string, object?> values, ref List<string> errors, object? defaultValue = null)
        {
            // if model has a Key attribute, give it a default value if it wasn't provided
            if (Attribute.IsDefined(property, typeof(KeyAttribute)))
            {
                defaultValue = default(T);
            }
            if (isSuccess)
            {
                values.Add(property.Name, value);
            }
            else
            {
                if (defaultValue != null)
                    values.Add(property.Name, defaultValue);
                else if (!IsNullable(property))
                    errors.Add($"Invalid value for column '{property.Name}'");
            }
        }

        protected virtual bool TryGetValue<T>(string?[] rowData, int columnIndex, string name, out T? value)
        {
            value = default;
            var type = typeof(T);
            if (columnIndex < 0 || columnIndex >= rowData.Length)
            {
                value = default;
                return true;
            }
            if (Nullable.GetUnderlyingType(type) != null && (rowData[columnIndex] == null || rowData[columnIndex]?.Equals("null", StringComparison.InvariantCultureIgnoreCase) == true))
                return true;
            var unquotedValue = GetQuoted(rowData[columnIndex]);

            return TryGetQuotedValue(unquotedValue, out value);
        }

        protected virtual bool TryGetQuotedValue<T>(string? unquotedValue, out T? value)
        {
            value = default;
            var type = typeof(T);
            if (type == typeof(string))
            {
                if (!string.IsNullOrEmpty(unquotedValue))
                    value = (T)(object)unquotedValue;
                return true;
            }
            if (type == typeof(long) || type == typeof(long?))
            {
                var isLongValid = long.TryParse(unquotedValue, out var longValue);
                value = (T)(object)longValue;
                return isLongValid;
            }
            if (type == typeof(int) || type == typeof(int?))
            {
                var isIntValid = int.TryParse(unquotedValue, out var intValue);
                value = (T)(object)intValue;
                return isIntValid;
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                var isIntValid = int.TryParse(unquotedValue, out var boolValue);
                value = (T)(object)boolValue;
                return isIntValid;
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                var isDateValid = false;
                if (!string.IsNullOrEmpty(unquotedValue))
                {
                    isDateValid = DateTime.TryParse(unquotedValue, out var dateValue);
                    value = (T)(object)dateValue;
                }
                return isDateValid;
            }
            if (type == typeof(double) || type == typeof(double?))
            {
                var isDoubleValid = double.TryParse(unquotedValue, out var doubleValue);
                value = (T)(object)doubleValue;
                return isDoubleValid;
            }
            if (type == typeof(float) || type == typeof(float?))
            {
                var isFloatValid = float.TryParse(unquotedValue, out var floatValue);
                value = (T)(object)floatValue;
                return isFloatValid;
            }
            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                var isDecimalValid = decimal.TryParse(unquotedValue, out var decimalValue);
                value = (T)(object)decimalValue;
                return isDecimalValid;
            }
            if (type == typeof(byte) || type == typeof(byte?))
            {
                var isByteValid = byte.TryParse(unquotedValue, out var byteValue);
                value = (T)(object)byteValue;
                return isByteValid;
            }
            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                var isGuidValid = Guid.TryParse(unquotedValue, out var guidValue);
                value = (T)(object)guidValue;
                return isGuidValid;
            }
            if (type == typeof(ICollection<string>) || type == typeof(ICollection<string?>))
            {
                if (string.IsNullOrEmpty(unquotedValue)) return true;
                value = (T)(object)unquotedValue.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a quoted string value from the input string.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        protected virtual string? GetQuoted(string? val)
        {
            if (val == null)
                return null;
            var match = Regex.Match(val, @"'[^']*'|\""[^\""]*\""");
            if (match.Success)
            {
                var value = match.Value.Substring(1, match.Value.Length - 2);
                // decode line breaks
                value = value.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                return value;
            }
            // decode line breaks
            val = val.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
            return val;
        }

        /// <summary>
        /// Splits the data into rows based on the specified delimiters, handling quoted strings correctly.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rowDelimiters"></param>
        /// <param name="removeBoundary"></param>
        /// <returns></returns>
        protected virtual string[] SplitBoundaries(string data, char[] rowDelimiters, bool removeBoundary = false)
        {
            var rows = new List<string>();
            var quotes = new List<char> { '"', '\'' };
            var startPos = 0;
            var insideQuotes = false;
            var insideQuotesChar = '\0';
            for (var i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (quotes.Contains(c))
                {
                    if (!insideQuotes)
                    {
                        insideQuotes = true;
                        insideQuotesChar = c;
                    }
                    else if (c == insideQuotesChar)
                    {
                        insideQuotes = false;
                    }
                }
                if ((rowDelimiters.Any(x => x.Equals(c)) && !insideQuotes) || i == data.Length - 1)
                {
                    var row = data.Substring(startPos, i - startPos + 1 - (removeBoundary && !(i == data.Length - 1) ? 1 : 0));
                    if (!string.IsNullOrEmpty(row) && row.Length > (removeBoundary ? 0 : 1))
                        rows.Add(row);
                    startPos = i + 1;
                }
            }

            return rows.ToArray();
        }
    }
}
