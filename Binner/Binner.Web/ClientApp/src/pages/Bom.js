import React, { useState, useEffect, useMemo, useCallback } from "react";
import { HashLink } from 'react-router-hash-link';
import { useParams, useNavigate, Link } from "react-router-dom";
import { Trans, useTranslation } from "react-i18next";
import { Icon, Input, Tab, Button, TextArea, Form, Table, Segment, Popup, Grid, Pagination, Dropdown, Confirm, Breadcrumb, Statistic, Menu, Label } from "semantic-ui-react";
import { FormHeader } from "../components/FormHeader";
import axios from "axios";
import _, { sortBy } from "underscore";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import { toast } from "react-toastify";
import { getCurrencySymbol } from "../common/Utils";
import { format, parseJSON } from "date-fns";
import { AddBomPartModal } from "../components/modals/AddBomPartModal";
import { AddPcbModal } from "../components/modals/AddPcbModal";
import { ProducePcbModal } from "../components/modals/ProducePcbModal";
import { Clipboard } from "../components/Clipboard";
import { FormatFullDateTime } from "../common/datetime";
import { getAuthToken } from "../common/authentication";
import { getSystemSettings } from "../common/applicationSettings";
import { getProduciblePcbCount, getProducibleBomCount, getTotalOutOfStockParts, getTotalInStockParts, getProjectColor } from "../common/bomTools";
import BomPartsGrid from "../components/BomPartsGrid";
import "./Bom.css";

/** BOM Management
 * Description: Manage parts and PCBs that are part of a BOM project
 */
export function Bom(props) {
  const { t } = useTranslation();
  const PopupDelayMs = 500;
  const defaultProject = {
    projectId: null,
    name: "",
    description: "",
    location: "",
    color: 0,
    loading: false,
    parts: [],
    pcbs: []
  };
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [pageSize, setPageSize] = useState(parseInt(localStorage.getItem("bomRecordsPerPage")) || 25);
  const [loading, setLoading] = useState(true);
  const [project, setProject] = useState(defaultProject);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [systemSettings, setSystemSettings] = useState({ currency: "USD", customFields: [] });
  const [btnSelectToolsDisabled, setBtnSelectToolsDisabled] = useState(true);
  const [addPartModalOpen, setAddPartModalOpen] = useState(false);
  const [addPcbModalOpen, setAddPcbModalOpen] = useState(false);
  const [producePcbModalOpen, setProducePcbModalOpen] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [confirmPartDeleteContent, setConfirmPartDeleteContent] = useState(null);
  const [isDirty, setIsDirty] = useState(false);
  const [filterInStock, setFilterInStock] = useState(false);
  const [inventoryMessage, setInventoryMessage] = useState(null);
  const [pageDisabled, setPageDisabled] = useState(false);
  const [btnDeleteText, setBtnDeleteText] = useState(t("button.removePart", "Remove Part"));
  const [currentPcbPages, setCurrentPcbPages] = useState([]);
  const [activeTab, setActiveTab] = useState(-1);
  const [sortBy, setSortBy] = useState("PcbId");
  const [sortDirection, setSortDirection] = useState("Ascending");

  const [colors] = useState(
    _.map(ProjectColors, function (c) {
      return {
        key: c.value,
        value: c.value,
        text: c.name,
        label: { ...(c.name !== "" && { color: c.name }), circular: true, empty: true, size: "tiny" }
      };
    })
  );

  const itemsPerPageOptions = [
    { key: 1, text: "5", value: 5 },
    { key: 2, text: "10", value: 10 },
    { key: 3, text: "25", value: 25 },
    { key: 4, text: "50", value: 50 },
    { key: 5, text: "100", value: 100 }
  ];

  const downloadOptions = [
    { key: "csv", icon: "file text", text: "Csv", value: "csv" },
    { key: "excel", icon: "file excel", text: "Excel", value: "excel" }
  ];

  const moveOptions = [
    { key: 0, icon: "folder open", text: "Unassociated", value: 0 },
    ...project.pcbs.map((pcb, key) => (
    { key: key + 1, icon: <i className="pcb-icon tiny" />, text: pcb.name, value: pcb.pcbId, disabled: project.pcbs[activeTab - 1]?.pcbId === pcb.pcbId }
  ))];
  

  const loadProject = async (projectName, systemSettings) => {
    setLoading(true);
    const response = await fetchApi(`/api/bom?name=${encodeURIComponent(projectName)}`).catch((c) => {
      if (c.status === 404) {
        toast.error(t("error.projectNotFound", "Could not find project named {{projectName}}"), { projectName });
        setPageDisabled(true);
        setLoading(false);
        return;
      }
    });
    if (response && response.data) {
      const { data } = response;
      setProject({ ...data, parts: data.parts.map((part) => ({...part, selected: false })) });
      setInventoryMessage(getInventoryMessage(data));
      setTotalRecords(data.parts.length);
      setTotalPages(Math.ceil(data.parts.length / pageSize));
      setCurrentPcbPages(_.map(data.pcbs, (x) => ({ pcbId: x.pcbId, page: 1 })));
      setLoading(false);
    }
  };

  useEffect(() => {
    getSystemSettings()
      .then((systemSettings) => {
        setSystemSettings(systemSettings);
        loadProject(props.params.project, systemSettings);
      });
  }, [props.params.project]);

  const handlePageSizeChange = async (e, pageSize) => {
    setPageSize(pageSize);
    setPage(1);
    setTotalPages(Math.ceil(totalRecords / pageSize));
    localStorage.setItem("bomRecordsPerPage", pageSize);
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
  };

  const handlePcbPageChange = (e, control, pcbId) => {
    let currentPcbPage = _.find(currentPcbPages, (x) => x.pcbId === pcbId);
    currentPcbPage.page = control.activePage;
    setCurrentPcbPages([...currentPcbPages]);
  };

  const handleFilterInStockChange = (e, control) => {
    setFilterInStock(control.checked);
  };

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setProject({ ...project, parts: _.sortBy(project.parts, [clickedColumn]) });
      setDirection("ascending");
    } else {
      setColumn(clickedColumn);
      setProject({ ...project, parts: project.parts.reverse() });
      setDirection(direction === "ascending" ? "descending" : "ascending");
    }
  };

  /** Get all parts for a pcb */
  const getPartsForPcb = (pcbId) => {
    return _.filter(project.parts, part => part.pcbId === pcbId);
  };

  /** Get the projectPartAssignmentId for all selected parts */
  const handleGetSelectedPartAssignmentIds = (parts) => {
    return handleGetSelectedParts(parts).map(i => i.projectPartAssignmentId);
  };

  /** Get all selected parts */
  const handleGetSelectedParts = (parts) => {
    return _.filter(parts, i => i.selected);
  };

  const handleDelete = async (e, control) => {
    const checkedValues = handleGetSelectedPartAssignmentIds(project.parts);

    setLoading(true);
    const request = {
      projectId: project.projectId,
      ids: checkedValues
    };
    const response = await fetchApi("/api/bom/part", {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const parts = _.filter(project.parts, (i) => !checkedValues.includes(i.projectPartAssignmentId));
      // reset all the parts to unselected
      const newProject = { ...project, parts: parts.map(i => ({...i, selected: false })) };
      setProject(newProject);
      setInventoryMessage(getInventoryMessage(newProject));
      setTotalPages(Math.ceil(parts.length / pageSize));
      setCurrentPcbPages(_.map(newProject.pcbs, (x) => ({ pcbId: x.pcbId, page: 1 })));
    } else {
      toast.error(t("error.failedToRemoveBomParts", "Failed to remove parts from BOM!"));
    }
    setLoading(false);
    setBtnSelectToolsDisabled(true);
    setConfirmDeleteIsOpen(false);
  };

  const handleOpenAddPart = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setAddPartModalOpen(true);
  };

  const handleOpenProducePcb = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setProducePcbModalOpen(true);
  };

  const handleOpenAddPcb = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setAddPcbModalOpen(true);
  };

  const savePartInlineChange = async (bomPart) => {
    if (!isDirty) return;
    setLoading(true);
    const request = {
      ...bomPart,
      cost: parseFloat(bomPart.cost) || 0,
      quantity: parseInt(bomPart.quantity) || 0,
      quantityAvailable: bomPart.part ? 0 : parseInt(bomPart.quantityAvailable) || 0,
      // conditionally add the part if it's available
      ...(bomPart.part && {
        part: {
          ...bomPart.part,
          cost: parseFloat(bomPart.part.cost) || 0,
          quantity: parseInt(bomPart.part.quantity) || 0
        }
      })
    };
    const response = await fetchApi("/api/bom/part", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      setInventoryMessage(getInventoryMessage(project));
    } else toast.error(t("error.failedSaveProject", "Failed to save project change!"));
    setLoading(false);
    setIsDirty(false);
  };

  const handlePartsInlineChange = (e, control, part) => {
    e.preventDefault();
    e.stopPropagation();
    if (part[control.name] !== control.value) setIsDirty(true);
    let parsed = 0;
    switch (control.name) {
      case "quantity":
        parsed = parseInt(control.value);
        if (!isNaN(parsed)) {
          part.quantity = parsed;
        }
        break;
      case "quantityAvailable":
        parsed = parseInt(control.value);
        if (!isNaN(parsed)) {
          // special case, if editing a part change its quantity.
          /// if no part is associated, set the custom quantityAvailable
          if (part.part) {
            part.part.quantity = parsed;
          } else {
            part.quantityAvailable = parsed;
          }
        }
        break;
      case "cost":
        if (part.part) {
          part.part.cost = control.value;
        } else {
          part.cost = control.value;
        }
        break;
      default:
        part[control.name] = control.value;
        break;
    }
    setProject({ ...project });
  };

  const handleTabChange = (e, data) => {
    setActiveTab(data.activeIndex);
  };

  const handleAddPart = async (e, addPartSelectedPart) => {
    if (!addPartSelectedPart) {
      toast.error(t("error.noPartSelected", "No part selected!"));
      return;
    }
    // add part to BOM/project
    setAddPartModalOpen(false);
    setLoading(true);
    const request = {
      projectId: project.projectId,
      partNumber: addPartSelectedPart.part?.partNumber ?? addPartSelectedPart.partName,
      pcbId: addPartSelectedPart.pcbId,
      quantity: addPartSelectedPart.quantity,
      quantityAvailable: addPartSelectedPart.part ? 0 : addPartSelectedPart.quantity,
      notes: addPartSelectedPart.notes,
      referenceId: addPartSelectedPart.referenceId,
      schematicReferenceId: addPartSelectedPart.schematicReferenceId,
      customDescription: addPartSelectedPart.customDescription
    };
    const response = await fetchApi("/api/bom/part", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.ok) {
      const newPart = response.data;
      const newProject = { ...project, parts: [...project.parts, newPart] };
      setProject(newProject);
      const newTotalPages = Math.ceil(newProject.parts.length / pageSize);
      setTotalPages(newTotalPages);
      setPage(newTotalPages);
      setInventoryMessage(getInventoryMessage(newProject));
      setCurrentPcbPages(_.map(newProject.pcbs, (x) => ({ pcbId: x.pcbId, page: 1 })));
    } else {
      toast.error(t("error.failedAddPart", "Failed to add part!"));
    }
    setLoading(false);
  };

  const handleAddPcb = async (e, pcb) => {
    // add part to BOM/project
    setAddPcbModalOpen(false);
    setLoading(true);
    const request = { ...pcb, projectId: project.projectId };
    const requestData = new FormData();
    requestData.append("projectId", request.projectId);
    requestData.append("name", request.name);
    requestData.append("description", request.description);
    requestData.append("serialNumberFormat", request.serialNumberFormat);
    requestData.append("serialNumber", request.serialNumberFormat);
    requestData.append("quantity", request.quantity);
    requestData.append("cost", request.cost);
    if (pcb.image !== null) requestData.append("image", pcb.image);

    // first fetch some data using fetchApi, to leverage 401 token refresh
    fetchApi("/api/authentication/identity").then((_) => {
      axios
        .request({
          method: "post",
          url: "/api/bom/pcbWithImage",
          data: requestData,
          headers: { Authorization: `Bearer ${getAuthToken()}` }
        })
        .then((response) => {
          if (response.status === 200) {
            const data = response.data;
            project.pcbs.push(data);
            setProject({ ...project });
            setInventoryMessage(getInventoryMessage(project));
            setCurrentPcbPages(project.pcbs.map((x) => ({ pcbId: x.pcbId, page: 1 })));
            toast.success(`Added ${pcb.name} pcb to project!`);
          } else {
            toast.error(t("error.failedAddPcb", "Failed to add pcb!"));
          }
          setLoading(false);
        })
        .catch((error) => {
          console.error('error', error);
          toast.error(t("error.failedAddPcb", "Failed to add pcb!"));
          setLoading(false);
        });
    });
  };

  const handleProducePcb = async (e, producePcbRequest) => {
    // produce BOM pcb(s) by reducing inventory quantities
    setProducePcbModalOpen(false);
    setLoading(true);
    const request = { ...producePcbRequest, projectId: project.projectId };
    const response = await fetchApi("/api/bom/produce", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request),
      catchErrors: true
    });
    if (response.responseObject.ok) {
      const data = response.data;
      setProject(data);
      setInventoryMessage(getInventoryMessage(data));
      toast.success(t("success.producedPcbs", "{{quantity}} PCB's were produced!", { quantity: producePcbRequest.quantity }));
    } else {
      const message = await response.text();
      toast.error(message, { autoClose: 10000 });
    }
    setLoading(false);
  };

  const handleMove = async (e, control) => {
    const selectedParts = handleGetSelectedPartAssignmentIds(project.parts);

    const request = {
      projectId: project.projectId,
      ids: selectedParts,
      pcbId: control.value
    };
    setLoading(true);
    const response = await fetchApi("/api/bom/move", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request),
      catchErrors: true
    });
    if (response.responseObject.status === 200 && response.data) {
      // success
      // todo: finish move internal data
      const parts = _.map(project.parts, (part) => {
        if (selectedParts.includes(part.projectPartAssignmentId)) {
          part.pcbId = control.value;
        }
        return part;
      });
      // also reset the checkboxes
      const newProject = { ...project, parts: parts.map(i => ({ ...i, selected: false})) };
      setProject(newProject);
      setInventoryMessage(getInventoryMessage(newProject));
      setTotalPages(Math.ceil(parts.length / pageSize));
      setCurrentPcbPages(_.map(newProject.pcbs, (x) => ({ pcbId: x.pcbId, page: 1 })));
    } else {
      toast.error(t("error.failedToMoveBomParts", "Failed to move parts!"));
    }
    setLoading(false);
    setBtnSelectToolsDisabled(true);
  };

  const handleDownload = async (e, control) => {
    // prevent onBlur from triggering a download
    if (e.type !== "click") return;

    // download a BOM parts list
    setLoading(true);
    let format = 0;
    let label = "Csv";
    switch (control.value) {
      case "csv":
      default:
        format = 0;
        label = "Csv";
        break;
      case "excel":
        format = 1;
        label = "Excel";
        break;
    }
    const request = {
      projectId: project.projectId,
      format
    };
    // first fetch some data using fetchApi, to leverage 401 token refresh
    fetchApi("/api/authentication/identity").then((_) => {
      axios
        .post(`/api/bom/download`, 
          request, 
          { 
            responseType: "blob", 
            headers: { Authorization: `Bearer ${getAuthToken()}` } 
          })
        .then((blob) => {
          // specifying blob filename, must create an anchor tag and use it as suggested: https://stackoverflow.com/questions/19327749/javascript-blob-filename-without-link
          var file = window.URL.createObjectURL(blob.data);
          var a = document.createElement("a");
          document.body.appendChild(a);
          a.style = "display: none";
          a.href = file;
          a.download = `${project.name}-${label}-BOM.zip`;
          a.click();
          window.URL.revokeObjectURL(file);
          toast.success(t("success.bomExported", "BOM exported successfully!"));
          setLoading(false);
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(t("error.failedBomExport", "BOM export failed!"));
          setLoading(false);
        });
    });
  };

  const confirmDeleteOpen = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const selectedPartAssignmentIds = handleGetSelectedPartAssignmentIds(project.parts);
    setConfirmDeleteIsOpen(true);
    setConfirmPartDeleteContent(
      <p>
        <Trans i18nKey="confirm.removeBomParts" quantity={selectedPartAssignmentIds.length}>
          Are you sure you want to remove these <b>{{ quantity: selectedPartAssignmentIds.length }}</b> part(s) from your BOM?
        </Trans>
        <br />
        <br />
        <b>
          {project.parts
            .filter((p) => selectedPartAssignmentIds.includes(p.projectPartAssignmentId))
            .map((p) => p.partName)
            .join(", ")}
        </b>
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
          This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteIsOpen(false);
  };

  const getInventoryMessage = (project, pcb) => {
    if (pcb && pcb.pcbId > 0) {
      const pcbParts = getPartsForPcb(pcb.pcbId);
      const pcbCount = getProduciblePcbCount(project.parts, pcb);
      if (pcbParts.length > 0 && pcbCount.count === 0) {
        return <div className="inventorymessage">{t("page.bom.notEnoughPartsToProducePcb", "You do not have enough parts to produce this PCB.")}</div>;
      } else if (pcbParts.length === 0) {
        return <div className="inventorymessage">{t("page.bom.noPartsAssigned", "Assign parts to get a count of how many times you can produce this PCB.")}</div>;
      }
      return (
        <span className="inventorymessage">
          <Trans i18nKey="page.bom.pcbProduceCount" count={pcbCount.count}>
            You can produce this PCB <b>{{ count: pcbCount.count }}</b> times with your current inventory.
          </Trans>
        </span>
      );
    }

    const maxPcbProduceCount = getProduciblePcbCount(project.parts, pcb);
    const maxBomPcbProduceCount = getProducibleBomCount(project.parts, project.pcbs);

    // list the pcb that has the least amount of parts
    const limitingPcb = _.find(project.pcbs, (x) => x.pcbId === maxPcbProduceCount.limitingPcb);
    const result = (<div className="inventorymessage">
      {maxBomPcbProduceCount.count > 0 
      ?
      <div>
        <Trans i18nKey="page.bom.maxBomPcbProduceCount" count={maxBomPcbProduceCount.count}>
          You can produce your entire BOM <b>{{ count: maxBomPcbProduceCount.count }}</b> times with your current inventory.
        </Trans>
      </div>
      : <div>{t("page.bom.notEnoughPartsToProduceBom", "You do not have enough parts to produce your entire BOM.")}</div>}
      <div>
        <Trans i18nKey="page.bom.maxPcbProduceCount" count={maxPcbProduceCount.count}>
          You can produce <b>{{ count: maxPcbProduceCount.count }}</b> PCB(s) with your current inventory.
        </Trans>
      </div>
      {limitingPcb && (
        <div className="textvalignmiddle">
          <i className="pcb-icon tiny" />
          <span>
            <Trans i18nKey="page.bom.lowestPcb" name={limitingPcb?.name}>
              <i>{{ name: limitingPcb?.name }}</i> is the lowest on inventory.
            </Trans>
          </span>
        </div>
      )}
    </div>);
    return result;
  };

  const setActivePartName = (e, partName) => {
    const activePartName = document.getElementById("activePartName");
    if (activePartName) {
      activePartName.innerHTML = partName;
      if (partName?.length > 0) {
        activePartName.style.opacity = 1;
      } else {
        activePartName.style.opacity = 0;
      }
    }
  };

  const handleSaveInlineChange = async (e, control, part) => {
    await savePartInlineChange(part);
  };

  /** Fired when the checkbox/toggle for a part is changed */
  const handleSelectPartChanged = (e, part) => {
    // get all the parts
    const updatedProject = { ...project, parts: project.parts.map(p => {
      if (p.projectPartAssignmentId === part.projectPartAssignmentId) {
        return part;
      }
      return p;
    })};
    //const updatedProject = { ...project, parts: [..._.filter(project.parts, i => i.partId !== part.partId), part] };
    setProject(updatedProject);
    const selectedParts = handleGetSelectedParts(updatedProject.parts);
    if (selectedParts.length > 0) {
      if (selectedParts.length > 1) setBtnDeleteText(t("button.removeXParts", "Remove ({{checkboxesChecked}}) Parts", { checkboxesChecked: selectedParts.length }));
      else setBtnDeleteText(t("button.removePart", "Remove Part"));
      setBtnSelectToolsDisabled(false);
    } else {
      setBtnDeleteText(t("button.removePart", "Remove Part"));
      setBtnSelectToolsDisabled(true);
    }
  };

  const handleSetPage = (e, page) => {
    setPage(page);
  };

  const handlePartClick = (e, part) => {
    //setActivePartName(e, part.partName);
  };

  const handleSortChange = async (sortBy, sortDirection) => {
    const newSortBy = sortBy || "DateCreatedUtc";
    const newSortDirection = sortDirection || "Descending";
    setSortBy(newSortBy);
    setSortDirection(newSortDirection);
  };

  const handleInit = (e) => {

  };

  const handleSelectAll = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const parts = _.map(project.parts, i => ({ ...i, selected: true }));
    const newProject = { ...project, parts };
    setProject(newProject);
  };

  const handleSelectNone = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const parts = _.map(project.parts, i => ({ ...i, selected: false }));
    const newProject = { ...project, parts };
    setProject(newProject);
  };

  const createSortablePart = (bomPart) => {
    return {
      ...bomPart,
      // virtualized fields for sorting
      sorted_cost: bomPart.cost || bomPart.part?.cost || 0,
      sorted_quantity: bomPart.quantity,
      sorted_quantityAvailable: bomPart.quantityAvailable || bomPart.part?.quantity || 0,
      sorted_pcb: bomPart.pcbId
    };
  };

  const renderTabContent = useCallback((pcb = { pcbId: -1 }) => {
    let tabParts = project.parts;
    if (pcb.pcbId > 0) tabParts = getPartsForPcb(pcb.pcbId);
    tabParts = tabParts.map(i => createSortablePart(i));

    // handle sorting
    if (sortBy) {
      // some fields come from the root bom part, some from the part child object. Use virtualized fields when available
      const sortField = 'sorted_' + sortBy; // first look for sorted_*, then try the root property, then the root.part property
      tabParts = _.sortBy(tabParts, bomPart => sortField in bomPart ? bomPart[sortField] : sortBy in bomPart ? bomPart[sortBy] : bomPart.part[sortBy]);
      if (sortDirection === 'Descending')
        tabParts.reverse();
    }

    const totalPagesForTab = Math.ceil(tabParts.length / pageSize);

    return (
      <div className="scroll-container">
        {pcb.pcbId > 0 && <div className="produce-summary">{getInventoryMessage(project, pcb)}</div>}
        <BomPartsGrid
          project={project}
          parts={_.chain(tabParts).drop((page-1) * pageSize).take(pageSize).value()}
          onCheckedChange={handleSelectPartChanged}
          page={page}
          totalPages={totalPagesForTab}
          totalRecords={project.parts.length}
          loading={loading}
          loadPage={handleSetPage}
          onPartClick={handlePartClick}
          onPageSizeChange={handlePageSizeChange}
          onSortChange={handleSortChange}
          onInit={handleInit}
          onSelectAll={handleSelectAll}
          onSelectNone={handleSelectNone}
          onRowEditChange={handlePartsInlineChange}
          onSaveInlineChange={handleSaveInlineChange}
          name="partsGrid">
          {filterInStock ? t("message.noOutOfStockParts", "No out of stock parts.") :
            <>
              {t("message.noPartsAdded", "No parts added.")}
              <br />
              <br />
              <Link to="" onClick={handleOpenAddPart}>
                {t("button.addFirstPart", "Add your first part!")}
              </Link>
            </>
          }
        </BomPartsGrid>
      </div>);
  }, [project, filterInStock, page, pageSize, totalPages, loading, sortBy, sortDirection]);

  const getPcbTotalPages = (pcbId) => {
    return Math.ceil(getPartsForPcb(pcbId).length / pageSize);
  };

  const getPcbCurrentPage = (pcbId) => {
    return _.find(currentPcbPages, (x) => x.pcbId === pcbId).page;
  };

  // stats at top of page
  const outOfStock = getTotalOutOfStockParts(project?.parts).length;
  const inStock = getTotalInStockParts(project?.parts).length;
  const producibleCount = getProduciblePcbCount(project?.parts);
  const bomProducibleCount = getProducibleBomCount(project.parts, project.pcbs);

  const tabs = [
    {
      menuItem: (
        <Menu.Item key="all">
          <Popup
            position="top center"
            offset={[0, 10]}
            wide
            mouseEnterDelay={PopupDelayMs}
            content={<p>{t("popup.bom.displayAll", "Displays all parts including parts not associated with a PCB.")}</p>}
            trigger={
              <div>
                {t("button.all", "All")} <Label>{project.parts.length}</Label>
              </div>
            }
          />
        </Menu.Item>
      ),
      render: () => (
        <Tab.Pane>
          {renderTabContent()}
        </Tab.Pane>
      )
    },
    ..._.map(project.pcbs, (pcb) => ({
      menuItem: (
        <Menu.Item key={pcb.pcbId} className={`${project.pcbs.length > 5 ? "smaller" : ""}`}>
          <Popup
            position="top center"
            offset={[0, 10]}
            wide
            mouseEnterDelay={PopupDelayMs}
            content={<p>{pcb.description}</p>}
            trigger={
              <div>
                {project.pcbs.length <= 4 && <i className="pcb-icon tiny" />}
                {pcb.name}
                {pcb.quantity > 1 && <b>&nbsp;x{pcb.quantity}</b>} {project.pcbs.length <= 5 && <Label>{getPartsForPcb(pcb.pcbId).length}</Label>}
              </div>
            }
          />
        </Menu.Item>
      ),
      render: () => (
        <Tab.Pane>
          {renderTabContent(pcb)}
        </Tab.Pane>
      )
    }))
  ];

  const scrollWithOffset = (el) => {
    window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' }); 
}

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => props.history("/")}>
          {t("bc.home", "Home")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => props.history("/bom")}>
          {t("bc.boms", "BOMs")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.bom", "BOM")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.bom.header.title", "Bill of Materials")} to="/">
        <Trans i18nKey="page.bom.header.description">Manage your BOM by creating PCB(s) and adding your parts.</Trans>
      </FormHeader>
      <AddBomPartModal isOpen={addPartModalOpen} onAdd={handleAddPart} onClose={() => setAddPartModalOpen(false)} parts={project.parts || []} pcbs={project.pcbs || []} defaultPcb={activeTab > -1 ? project.pcbs[activeTab - 1] : null} />
      <AddPcbModal isOpen={addPcbModalOpen} onAdd={handleAddPcb} onClose={() => setAddPcbModalOpen(false)} />
      <ProducePcbModal isOpen={producePcbModalOpen} onSubmit={handleProducePcb} onClose={() => setProducePcbModalOpen(false)} project={project} />
      <Confirm
        className="confirm"
        header={t("confirm.header.removeBomPart", "Remove Part from BOM")}
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={handleDelete}
        content={confirmPartDeleteContent}
      />

      <Form className="bom">
        <Segment raised disabled={pageDisabled} className="thicker" {...getProjectColor(ProjectColors, project)}>
          <Grid columns={2} style={{ marginBottom: "10px" }}>
            <Grid.Column className="projectinfo" width={7}>
              <span className="large">{project.name}</span>
              <div className="link-icon">
                <Link to={`/project/${encodeURIComponent(project.name)}`}><Icon name="edit" /></Link>
              </div>
              <div>
                <label>
                  {t("label.lastModified", "Last modified")}: {project.dateModifiedUtc && format(parseJSON(project.dateModifiedUtc), FormatFullDateTime)}
                </label>
              </div>
            </Grid.Column>
            <Grid.Column width={9}>
              <Segment inverted textAlign="center" className="statisticsContainer">
                <Grid columns={2} style={{backgroundColor: 'transparent'}}>
                  <Grid.Column width={11}>
                    <Statistic.Group widths="three" size="tiny">
                      <Statistic inverted color="blue">
                        <Statistic.Value>{inStock}</Statistic.Value>
                        <Statistic.Label>{t("label.inStock", "In Stock")}</Statistic.Label>
                      </Statistic>
                      <Statistic inverted color={`${project?.parts.length > 0 && outOfStock > 0 ? "red" : "blue"}`}>
                        <Statistic.Value>{outOfStock}</Statistic.Value>
                        <Statistic.Label>{t("label.outOfStock", "Out of Stock")}</Statistic.Label>
                      </Statistic>
                      <Statistic inverted color="blue">
                        <Statistic.Value>{project.parts.length}</Statistic.Value>
                        <Statistic.Label>{t("label.totalParts", "Total Parts")}</Statistic.Label>
                      </Statistic>
                    </Statistic.Group>
                    {inventoryMessage}
                  </Grid.Column>
                  <Grid.Column width={5}>
                    <Statistic.Group widths="one" size="tiny">
                      <Statistic inverted color={`${project?.parts.length > 0 && bomProducibleCount.count === 0 ? "red" : "blue"}`}>
                        <Statistic.Value>{bomProducibleCount.count}</Statistic.Value>
                        <Statistic.Label>{t("label.bomProducible", "BOM Producible")}</Statistic.Label>
                      </Statistic>
                      <Statistic inverted color={`${project?.parts.length > 0 && producibleCount.count === 0 ? "red" : "blue"}`}>
                        <Statistic.Value>{producibleCount.count}</Statistic.Value>
                        <Statistic.Label>{t("label.pcbProducible", "PCB(s) Producible")}</Statistic.Label>
                      </Statistic>
                    </Statistic.Group>
                  </Grid.Column>
                </Grid>
                
              </Segment>
            </Grid.Column>
          </Grid>

          <div className="buttons">
            <Popup
              content={t("popup.bom.addPart", "Add a part to the BOM")}
              trigger={
                <Button primary onClick={handleOpenAddPart} size="mini" disabled={pageDisabled}>
                  <Icon name="plus" /> {t("button.addPart", "Add Part")}
                </Button>
              }
            />
            <Popup
              content={t("popup.bom.downloadBom", "Download a BOM part list")}
              trigger={
                <Button.Group size="mini">
                  <Button onClick={handleDownload} size="mini" disabled={pageDisabled} style={{ marginRight: "0" }}>
                    <Icon name="download" /> {t("button.download", "download")}
                  </Button>
                  <Dropdown className="button icon" floating options={downloadOptions} trigger={<></>} value={-1} onChange={handleDownload} />
                </Button.Group>
              }
            />
            <Popup
              wide
              content={t("popup.bom.addPcb", "Add a PCB to this project")}
              trigger={
                <Button onClick={handleOpenAddPcb} size="mini" disabled={pageDisabled}>
                  <i className="pcb-icon tiny" /> {t("button.addPcb", "Add PCB")}
                </Button>
              }
            />
            <Popup
              wide
              content={t("popup.bom.producePcb", "Reduce inventory quantities when a PCB is assembled")}
              trigger={
                <Button secondary onClick={handleOpenProducePcb} size="mini" disabled={pageDisabled}>
                  <i className="pcb-icon tiny" /> {t("button.producePcb", "Produce PCB")}
                </Button>
              }
            />
            <Popup
              wide
              content={t("popup.bom.produceHistory", "View production history")}
              trigger={
                <HashLink smooth to={`/project/${project.name}#history`} scroll={scrollWithOffset}>
                  <Button size="mini" disabled={pageDisabled}>
                    <i className="pcb-icon tiny" /> {t("button.produceHistory", "History")}
                  </Button>
                </HashLink>
              }
            />
            <div style={{ float: "right" }}>
              <Popup
                wide
                content={t("popup.bom.filterOutOfStock", "Select to only show out of stock parts")}
                trigger={<Form.Checkbox toggle label="Out of Stock" name="filterInStock" onChange={handleFilterInStockChange} />}
              />
            </div>
          </div>
          <div className="buttons small" style={{marginTop: '5px'}}>
            <Popup
              content={t("popup.bom.removePart", "Remove selected parts from the BOM")}
              trigger={
                <Button onClick={confirmDeleteOpen} disabled={btnSelectToolsDisabled} size="mini">
                  <Icon name="trash alternate" /> {btnDeleteText}
                </Button>
              }
            />
            <Popup
              content={t("popup.bom.movePart", "Move selected parts to another tab")}
              trigger={
                <Button.Group size="mini">
                  <Button onClick={handleDownload} size="mini" style={{ marginRight: "0" }} disabled={btnSelectToolsDisabled}>
                    <Icon name="mail forward" /> {t("button.moveTo", "Move to")}
                  </Button>
                  <Dropdown className="button icon" floating options={moveOptions} trigger={<></>} value={-1} onChange={handleMove} disabled={btnSelectToolsDisabled} />
                </Button.Group>
              }
            />
          </div>

          <div id="activePartName" />

          <Segment loading={loading}>
            <Tab panes={tabs} onTabChange={handleTabChange}></Tab>
          </Segment>
        </Segment>
      </Form>
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Bom {...props} params={useParams()} history={useNavigate()} location={window.location} />;
