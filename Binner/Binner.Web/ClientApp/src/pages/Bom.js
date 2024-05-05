import React, { useState, useEffect } from "react";
import { HashLink } from 'react-router-hash-link';
import { useParams, useNavigate, Link } from "react-router-dom";
import { Trans, useTranslation } from "react-i18next";
import { Icon, Input, Tab, Button, TextArea, Form, Table, Segment, Popup, Grid, Pagination, Dropdown, Confirm, Breadcrumb, Statistic, Menu, Label } from "semantic-ui-react";
import { FormHeader } from "../components/FormHeader";
import axios from "axios";
import _ from "underscore";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import { toast } from "react-toastify";
import { getCurrencySymbol } from "../common/Utils";
import { format, parseJSON } from "date-fns";
import { AddBomPartModal } from "../components/AddBomPartModal";
import { AddPcbModal } from "../components/AddPcbModal";
import { ProducePcbModal } from "../components/ProducePcbModal";
import { Clipboard } from "../components/Clipboard";
import { FormatFullDateTime } from "../common/datetime";
import { getAuthToken } from "../common/authentication";
import { getProduciblePcbCount, getTotalOutOfStockParts, getTotalInStockParts, getProjectColor } from "../common/bomTools";
import "./Bom.css";

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
  const [pageSize, setPageSize] = useState(parseInt(localStorage.getItem("bomRecordsPerPage")) || 5);
  const [loading, setLoading] = useState(true);
  const [project, setProject] = useState(defaultProject);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
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
  

  const loadProject = async (projectName) => {
    setLoading(true);
    const response = await fetchApi(`api/bom?name=${encodeURIComponent(projectName)}`).catch((c) => {
      if (c.status === 404) {
        toast.error(t("error.projectNotFound", "Could not find project named {{projectName}}"), { projectName });
        setPageDisabled(true);
        setLoading(false);
        return;
      }
    });
    if (response && response.data) {
      const { data } = response;
      setProject(data);
      setInventoryMessage(getInventoryMessage(data));
      setTotalPages(Math.ceil(data.parts.length / pageSize));
      setCurrentPcbPages(_.map(data.pcbs, (x) => ({ pcbId: x.pcbId, page: 1 })));
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProject(props.params.project);
  }, [props.params.project]);

  const handlePageSizeChange = async (e, control) => {
    const newPageSize = parseInt(control.value);
    const newTotalPages = Math.ceil(project.parts.length / newPageSize);
    setPageSize(newPageSize);
    setTotalPages(newTotalPages);
    localStorage.setItem("bomRecordsPerPage", newPageSize);
    // redirect to the last page if less pages
    if (page > newTotalPages) {
      setPage(newTotalPages);
    }
  };

  const handlePageChange = (e, control) => {
    setPage(control.activePage);
  };

  const handlePcbPageChange = (e, control, pcbId) => {
    let currentPcbPage = _.find(currentPcbPages, (x) => x.pcbId === pcbId);
    currentPcbPage.page = control.activePage;
    setCurrentPcbPages([...currentPcbPages]);
  };

  const handleChange = (e, control) => {
    project[control.name] = control.value;
    setProject({ ...project });
  };

  const handleFilterInStockChange = (e, control) => {
    setFilterInStock(control.checked);
  };

  const handleClick = (e, control) => {};

  const onSubmit = () => {};

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

  const handleDelete = async (e, control) => {
    const checkboxes = document.getElementsByName("chkSelect");
    let checkedValues = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkedValues.push(parseInt(checkboxes[i].value));
    }

    setLoading(true);
    const request = {
      projectId: project.projectId,
      ids: checkedValues
    };
    const response = await fetchApi("api/bom/part", {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const parts = _.filter(project.parts, (i) => !checkedValues.includes(i.projectPartAssignmentId));

      // also reset the checkboxes
      for (let i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = false;
      }
      const newProject = { ...project, parts: parts };
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

  const handlePartSelected = (e, part) => {
    const checkboxesChecked = getPartsSelected();
    if (checkboxesChecked.length > 0) {
      if (checkboxesChecked.length > 1) setBtnDeleteText(t("button.removeXParts", "Remove ({{checkboxesChecked}}) Parts", { checkboxesChecked: checkboxesChecked.length }));
      else setBtnDeleteText(t("button.removePart", "Remove Part"));
      setBtnSelectToolsDisabled(false);
    } else {
      setBtnDeleteText(t("button.removePart", "Remove Part"));
      setBtnSelectToolsDisabled(true);
    }
  };

  const getPartsSelected = () => {
    const checkboxes = document.getElementsByName("chkSelect");
    let checkboxesChecked = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkboxesChecked.push(checkboxes[i]);
    }
    return checkboxesChecked;
  };

  const saveColumn = async (e, bomPart) => {
    await savePartInlineChange(bomPart);
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
    const response = await fetchApi("api/bom/part", {
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

  const getPage = (pcb) => {
    let currentPage = page;
    if (pcb.pcbId > 0) {
      currentPage = getPcbCurrentPage(pcb.pcbId);
    }
    const start = (currentPage - 1) * pageSize;
    let dataSource = [];
    // show all parts, or the active PCB tab
    if (pcb.pcbId === -1) dataSource = project.parts;
    else dataSource = _.filter(project.parts, (x) => x.pcbId === pcb.pcbId);

    let parts = [];
    if (filterInStock) parts = _.filter(dataSource, (x) => x.quantity > x.part.quantity);
    else parts = dataSource;
    const partsPage = [];
    for (let i = start; i < start + pageSize; i++) {
      if (i < parts.length) partsPage.push(parts[i]);
    }
    return partsPage;
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
    const response = await fetchApi("api/bom/part", {
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
    fetchApi("api/authentication/identity").then((_) => {
      axios
        .request({
          method: "post",
          url: "api/bom/pcbWithImage",
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
    const response = await fetchApi("api/bom/produce", {
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
    const checkboxes = document.getElementsByName("chkSelect");
    let checkedValues = [];
    for (let i = 0; i < checkboxes.length; i++) {
      if (checkboxes[i].checked) checkedValues.push(parseInt(checkboxes[i].value));
    }
    const request = {
      projectId: project.projectId,
      ids: checkedValues,
      pcbId: control.value
    };
    setLoading(true);
    const response = await fetchApi("api/bom/move", {
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
        if (checkedValues.includes(part.projectPartAssignmentId)) {
          part.pcbId = control.value;
        }
        return part;
      });
      // also reset the checkboxes
      for (let i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = false;
      }
      const newProject = { ...project, parts: parts };
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
    fetchApi("api/authentication/identity").then((_) => {
      axios
        .post(`api/bom/download`, 
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
    const checkboxesChecked = getPartsSelected();
    const selectedPartAssignmentIds = checkboxesChecked.map((c) => parseInt(c.value));
    setConfirmDeleteIsOpen(true);
    setConfirmPartDeleteContent(
      <p>
        <Trans i18nKey="confirm.removeBomParts" quantity={checkboxesChecked.length}>
          Are you sure you want to remove these <b>{{ quantity: checkboxesChecked.length }}</b> part(s) from your BOM?
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
      const pcbParts = _.filter(project.parts, (p) => p.pcbId === pcb.pcbId);
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

    const pcbCount = getProduciblePcbCount(project.parts, pcb);
    if (pcbCount.count === 0) {
      return <div className="inventorymessage">{t("page.bom.notEnoughPartsToProduceBom", "You do not have enough parts to produce your entire BOM.")}</div>;
    }
    const limitingPcb = _.find(project.pcbs, (x) => x.pcbId === pcbCount.limitingPcb);
    return (
      <div className="inventorymessage">
        <div>
          <Trans i18nKey="page.bom.bomProduceCount" count={pcbCount.count}>
            You can produce your entire BOM <b>{{ count: pcbCount.count }}</b> times with your current inventory.
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
      </div>
    );
  };

  const setActivePartName = (e, partName) => {
    const activePartName = document.getElementById("activePartName");
    if (activePartName) {
      activePartName.innerHTML = partName;
      if (partName.length > 0) {
        activePartName.style.opacity = 1;
      } else {
        activePartName.style.opacity = 0;
      }
    }
  };

  const renderTabContent = (pcb = { pcbId: -1 }) => {
    const pageParts = getPage(pcb);
    return (
      <div className="scroll-container">
        {pcb.pcbId > 0 && <div className="produce-summary">{getInventoryMessage(project, pcb)}</div>}
        <Table className="bom" style={{ width: pageParts.length === 0 ? "auto" : "" }}>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell></Table.HeaderCell>
              {pcb.pcbId === -1 && (
                <Table.HeaderCell style={{ width: "120px" }} sorted={column === "PCB" ? direction : null} onClick={handleSort("PCB")}>
                  <Popup
                    wide="very"
                    mouseEnterDelay={PopupDelayMs}
                    content={
                      <p>
                        <Trans i18nKey="popup.bom.pcb">
                          Indicates which PCB your part is assigned to. A PCB assignment is optional, all unassigned parts will appear in the <b>All</b> tab.
                        </Trans>
                      </p>
                    }
                    trigger={<span>{t("label.pcb", "PCB")}</span>}
                  />
                </Table.HeaderCell>
              )}
              <Table.HeaderCell width={2} sorted={column === "partNumber" ? direction : null} onClick={handleSort("partNumber")}>
                {t("label.partNumber", "Part Number")}
              </Table.HeaderCell>
              <Table.HeaderCell width={2} sorted={column === "manufacturerPartNumber" ? direction : null} onClick={handleSort("manufacturerPartNumber")}>
                {t("label.mfrPart", "Mfr Part")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "100px" }} sorted={column === "partType" ? direction : null} onClick={handleSort("partType")}>
                {t("label.partType", "Part Type")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "102px" }} sorted={column === "cost" ? direction : null} onClick={handleSort("cost")}>
                {t("label.cost", "Cost")}
              </Table.HeaderCell>
              <Table.HeaderCell sorted={column === "quantity" ? direction : null} onClick={handleSort("quantity")}>
                <Popup
                  mouseEnterDelay={PopupDelayMs}
                  content={<p>{t("popup.bom.quantity", "The quantity of parts required for a single PCB")}</p>}
                  trigger={<span>{t("label.quantity", "Quantity")}</span>}
                />
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "90px" }} sorted={column === "stock" ? direction : null} onClick={handleSort("stock")}>
                <Popup
                  mouseEnterDelay={PopupDelayMs}
                  content={<p>{t("popup.bom.inventoryQuantity", "The quantity of parts currently in inventory")}</p>}
                  trigger={<span>{t("label.inStock", "In Stock")}</span>}
                />
              </Table.HeaderCell>
              <Table.HeaderCell sorted={column === "leadTime" ? direction : null} onClick={handleSort("leadTime")}>
                {t("label.leadTime", "Lead Time")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "110px" }} sorted={column === "referenceId" ? direction : null} onClick={handleSort("referenceId")}>
                <Popup
                  wide="very"
                  mouseEnterDelay={PopupDelayMs}
                  content={
                    <p>
                      <Trans i18nKey="popup.bom.referenceId">
                        Your custom <Icon name="terminal" />
                        reference Id(s) you can use for identifying this part.
                      </Trans>
                    </p>
                  }
                  trigger={<span>{t("label.referenceIds", "Reference Id(s)")}</span>}
                />
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "110px" }} sorted={column === "schematicReferenceId" ? direction : null} onClick={handleSort("schematicReferenceId")}>
                <Popup
                  wide="very"
                  mouseEnterDelay={PopupDelayMs}
                  content={
                    <p>
                      <Trans i18nKey="popup.bom.schematicReferenceId">
                        Your custom <Icon name="hashtag" /> schematic reference Id(s) that identify the part on the PCB silkscreen.
                        <br />
                        Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                      </Trans>
                    </p>
                  }
                  trigger={<span>{t("label.schematicReferenceIds", "Schematic Reference Id(s)")}</span>}
                />
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "200px" }} sorted={column === "description" ? direction : null} onClick={handleSort("description")}>
                {t("label.description", "Description")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "200px" }} sorted={column === "customDescription" ? direction : null} onClick={handleSort("customDescription")}>
                {t("label.customDescription", "Custom Description")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ width: "200px" }} sorted={column === "note" ? direction : null} onClick={handleSort("note")}>
                {t("label.note", "Note")}
              </Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {pageParts.length > 0 ? (
              pageParts.map((bomPart, key) => (
                <Table.Row key={key} onMouseEnter={(e) => setActivePartName(e, bomPart.part?.partNumber || bomPart.partName)} onMouseLeave={(e) => setActivePartName(e, "")}>
                  <Table.Cell>
                    <input type="checkbox" name="chkSelect" value={bomPart.projectPartAssignmentId} onChange={(e) => handlePartSelected(e, bomPart)} />
                  </Table.Cell>
                  {pcb.pcbId === -1 && (
                    <Table.Cell className="overflow">
                      <div style={{ maxWidth: "120px" }}>{_.find(project.pcbs, (x) => x.pcbId === bomPart.pcbId)?.name}</div>
                    </Table.Cell>
                  )}
                  <Table.Cell>
                    <Clipboard text={bomPart.part?.partNumber || bomPart.partName} />
                    {bomPart.part ? (
                      <Link to={`/inventory/${encodeURIComponent(bomPart.part.partNumber)}`}>{bomPart.part.partNumber}</Link>
                    ) : (
                      <Popup
                        wide
                        mouseEnterDelay={PopupDelayMs}
                        content={
                          <p>
                            <Trans i18nKey="popup.bom.unassociatedPartName">
                              Edit the name of your unassociated <Icon name="microchip" /> part
                            </Trans>
                          </p>
                        }
                        trigger={
                          <Input
                            type="text"
                            transparent
                            name="partName"
                            onBlur={(e) => saveColumn(e, bomPart)}
                            onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                            value={bomPart.partName || 0}
                            className="inline-editable"
                          />
                        }
                      />
                    )}
                  </Table.Cell>
                  <Table.Cell>
                    {bomPart.part?.manufacturerPartNumber && (
                      <>
                        <Clipboard text={bomPart.part?.manufacturerPartNumber} /> {bomPart.part?.manufacturerPartNumber}
                      </>
                    )}
                  </Table.Cell>
                  <Table.Cell>{bomPart.part?.partType}</Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.bomCost">Edit the part cost</Trans>
                        </p>
                      }
                      trigger={
                        <Input
                          label={getCurrencySymbol(bomPart.part?.currency || bomPart.currency || "USD")}
                          type="text"
                          transparent
                          name="cost"
                          onBlur={(e) => saveColumn(e, bomPart)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                          value={bomPart.part?.cost || bomPart.cost || 0}
                          fluid
                          className="inline-editable"
                        />
                      }
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.bomQuantity">
                            Edit the <Icon name="clipboard list" /> BOM quantity required
                          </Trans>
                        </p>
                      }
                      trigger={
                        <Input
                          type="text"
                          transparent
                          name="quantity"
                          onBlur={(e) => saveColumn(e, bomPart)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                          value={bomPart.quantity || 0}
                          fluid
                          className={`inline-editable ${bomPart.quantity > (bomPart.part?.quantity || bomPart.quantityAvailable || 0) ? "outofstock" : ""}`}
                        />
                      }
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.quantityAvailable">
                            Edit the quantity available in your <Icon name="box" /> inventory
                          </Trans>
                        </p>
                      }
                      trigger={
                        <Input
                          type="text"
                          transparent
                          name="quantityAvailable"
                          onBlur={(e) => saveColumn(e, bomPart)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                          value={bomPart.part?.quantity || bomPart.quantityAvailable || 0}
                          fluid
                          className="inline-editable"
                        />
                      }
                    />
                  </Table.Cell>
                  <Table.Cell>{bomPart.part?.leadTime || ""}</Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.referenceId">
                            Edit your custom <Icon name="terminal" /> reference Id(s) you can use for identifying this part.
                          </Trans>
                        </p>
                      }
                      trigger={
                        <Input
                          type="text"
                          transparent
                          name="referenceId"
                          onBlur={(e) => saveColumn(e, bomPart)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                          value={bomPart.referenceId || ""}
                          fluid
                          className="inline-editable"
                        />
                      }
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide="very"
                      mouseEnterDelay={PopupDelayMs}
                      content={
                        <p>
                          <Trans i18nKey="popup.bom.editSchematicReferenceId">
                            Edit your custom <Icon name="hashtag" /> schematic reference Id(s) that identify the part on the PCB silkscreen.
                            <br />
                            Examples: <i>D1</i>, <i>C2</i>, <i>Q1</i>
                          </Trans>
                        </p>
                      }
                      trigger={
                        <Input
                          type="text"
                          transparent
                          name="schematicReferenceId"
                          onBlur={(e) => saveColumn(e, bomPart)}
                          onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                          value={bomPart.schematicReferenceId || ""}
                          fluid
                          className="inline-editable"
                        />
                      }
                    />
                  </Table.Cell>
                  <Table.Cell className="overflow">
                    <div style={{ width: "250px" }}>
                      {bomPart.part?.description && (
                        <>
                          <Clipboard text={bomPart.part?.description} />
                          <Popup hoverable content={<p>{bomPart.part?.description}</p>} trigger={<span>{bomPart.part?.description}</span>} />
                        </>
                      )}
                    </div>
                  </Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide
                      mouseEnterDelay={PopupDelayMs}
                      content={<p>{t("popup.bom.customDescription", "Provide a custom description")}</p>}
                      trigger={
                        <div style={{ maxWidth: "250px" }}>
                          <Form.Field
                            type="text"
                            control={TextArea}
                            style={{ height: "50px", minHeight: "50px", padding: "5px" }}
                            name="customDescription"
                            onBlur={(e) => saveColumn(e, bomPart)}
                            onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                            value={bomPart.customDescription || ""}
                            className="transparent inline-editable"
                          />
                        </div>
                      }
                    />
                  </Table.Cell>
                  <Table.Cell>
                    <Popup
                      wide
                      mouseEnterDelay={PopupDelayMs}
                      content={<p>{t("popup.bom.customNote", "Provide a custom note")}</p>}
                      trigger={
                        <div style={{ maxWidth: "250px" }}>
                          <Form.Field
                            type="text"
                            control={TextArea}
                            style={{ height: "50px", minHeight: "50px", padding: "5px" }}
                            name="notes"
                            onBlur={(e) => saveColumn(e, bomPart)}
                            onChange={(e, control) => handlePartsInlineChange(e, control, bomPart)}
                            value={bomPart.notes || ""}
                            className="transparent inline-editable"
                          />
                        </div>
                      }
                    />
                  </Table.Cell>
                </Table.Row>
              ))
            ) : (
              <Table.Row>
                <Table.Cell colSpan={12} textAlign="center" style={{ padding: "30px" }}>
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
                </Table.Cell>
              </Table.Row>
            )}
          </Table.Body>
        </Table>
      </div>
    );
  };

  const getPcbTotalPages = (pcbId) => {
    return Math.ceil(_.filter(project.parts, (x) => x.pcbId === pcbId).length / pageSize);
  };

  const getPcbCurrentPage = (pcbId) => {
    return _.find(currentPcbPages, (x) => x.pcbId === pcbId).page;
  };

  // stats at top of page
  const outOfStock = getTotalOutOfStockParts(project?.parts).length;
  const inStock = getTotalInStockParts(project?.parts).length;
  const producibleCount = getProduciblePcbCount(project?.parts);

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
          <div style={{ float: "right", verticalAlign: "middle", fontSize: "0.9em", marginTop: "7px", marginRight: "5px", height: "0" }}>
            <Dropdown selection options={itemsPerPageOptions} value={pageSize} className="small labeled" onChange={handlePageSizeChange} />
            <span>{t("label.recordsPerPage", "records per page")}</span>
          </div>
          <Pagination activePage={page} totalPages={totalPages} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" style={{ margin: "5px" }} />
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
                {pcb.quantity > 1 && <b>&nbsp;x{pcb.quantity}</b>} {project.pcbs.length <= 5 && <Label>{_.filter(project.parts, (x) => x.pcbId === pcb.pcbId).length}</Label>}
              </div>
            }
          />
        </Menu.Item>
      ),
      render: () => (
        <Tab.Pane>
          <div style={{ float: "right", verticalAlign: "middle", fontSize: "0.9em", marginTop: "7px", marginRight: "5px", height: "0" }}>
            <Dropdown selection options={itemsPerPageOptions} value={pageSize} className="small labeled" onChange={handlePageSizeChange} />
            <span>{t("label.recordsPerPage", "records per page")}</span>
          </div>
          <Pagination
            activePage={getPcbCurrentPage(pcb.pcbId)}
            totalPages={getPcbTotalPages(pcb.pcbId)}
            firstItem={null}
            lastItem={null}
            onPageChange={(e, control) => handlePcbPageChange(e, control, pcb.pcbId)}
            size="mini"
            style={{ margin: "5px" }}
          />
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
      <AddBomPartModal isOpen={addPartModalOpen} onAdd={handleAddPart} onClose={() => setAddPartModalOpen(false)} pcbs={project.pcbs || []} />
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
            <Grid.Column className="projectinfo">
              <span className="large">{project.name}</span>
              <div style={{ float: "right" }}>
                <Link to={`/project/${project.name}`}>{t("label.editProject", "Edit Project")}</Link>
              </div>
              <div>
                <label>
                  {t("label.lastModified", "Last modified")}: {project.dateModifiedUtc && format(parseJSON(project.dateModifiedUtc), FormatFullDateTime)}
                </label>
              </div>
            </Grid.Column>
            <Grid.Column>
              <Segment inverted textAlign="center" className="statisticsContainer">
                <Statistic.Group widths="four" size="tiny">
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
                  <Statistic inverted color={`${project?.parts.length > 0 && producibleCount.count === 0 ? "red" : "blue"}`}>
                    <Statistic.Value>{producibleCount.count}</Statistic.Value>
                    <Statistic.Label>{t("label.producible", "Producible")}</Statistic.Label>
                  </Statistic>
                </Statistic.Group>
                {inventoryMessage}
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
                    <Icon name="mail forward" /> {t("button.move", "Move")}
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
