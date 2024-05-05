import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import {
  Icon,
  Input,
  Button,
  TextArea,
  Form,
  Table,
  Segment,
  Popup,
  Breadcrumb,
  Confirm,
  Checkbox,
  Image
} from "semantic-ui-react";
import { getImagesToken } from "../common/authentication";
import { format, parseJSON } from "date-fns";
import { FormatFullDateTime } from "../common/datetime";
import { FormHeader } from "../components/FormHeader";
import _ from "underscore";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import { toast } from "react-toastify";
import { PcbHistoryModal } from "../components/PcbHistoryModal";
import "./Bom.css";

export function Project(props) {
  const { t } = useTranslation();
  const defaultProject = {
    projectId: null,
    name: "",
    description: "",
    location: "",
    color: 0,
    loading: false,
    parts: [],
    pcbs: [],
    produceHistory: []
  };
	const [loading, setLoading] = useState(true);
  const [project, setProject] = useState(defaultProject);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [confirmDeleteProjectIsOpen, setConfirmDeleteProjectIsOpen] = useState(false);
	const [confirmDeletePcbIsOpen, setConfirmDeletePcbIsOpen] = useState(false);
  const [confirmDeleteProduceHistoryIsOpen, setConfirmDeleteProduceHistoryIsOpen] = useState(false);
	const [btnDeleteProjectDisabled, setBtnDeleteProjectDisabled] = useState(true);
	const [changeTracker, setChangeTracker] = useState([]);
  const [confirmDeleteProjectContent, setConfirmDeleteProjectContent] = useState(null);
	const [confirmDeletePcbContent, setConfirmDeletePcbContent] = useState(null);
  const [confirmDeleteProduceHistoryContent, setConfirmDeleteProduceHistoryContent] = useState(null);
	const [selectedPcb, setSelectedPcb] = useState(null);
  const [selectedProduceHistory, setSelectedProduceHistory] = useState(null);
	const [btnSubmitDisabled, setBtnSubmitDisabled] = useState(false);
	const [btnDeleteDisabled, setBtnDeleteDisabled] = useState(false);
	const [pageDisabled, setPageDisabled] = useState(false);
  const [pcbHistoryModalIsOpen, setPcbHistoryModalIsOpen] = useState(false);
  const [pcbHistoryModalContext, setPcbHistoryModalContext] = useState(null);
	
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

  const loadProject = async (projectName) => {
    setLoading(true);
    const response = await fetchApi(`api/bom?name=${encodeURIComponent(projectName)}`)
			.catch(c => {
				if (c.status === 404){
					toast.error(t('error.projectNotFound', "Could not find a project named {{projectName}}", { projectName }));
					setBtnSubmitDisabled(true);
					setBtnDeleteDisabled(true);
					setPageDisabled(true);
					setLoading(false);
					return;	
				}
			});
		if (response && response.responseObject.status === 200) {
			const { data } = response;
      data.pcbs = _.map(data.pcbs, x => ({...x, cost: Number(x.cost).toFixed(2)}));
			setProject(data);
		}
    setLoading(false);
  };

  useEffect(() => {
    loadProject(props.params.project);
  }, [props.params.project]);

  const handleDeleteProject = async (e, control) => {
    setLoading(true);
    const request = {
      projectId: project.projectId,
    };
    const response = await fetchApi('api/project', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    }).catch(() => {
		});
    if (response && response.responseObject.status === 200) {
			toast.success(t('success.deletedProject', 'Project was deleted!'));
			props.history(-1);
    }
    else
      toast.error(t('error.failedDeleteProject', "Failed to remove project!"));
    setLoading(false);
    setBtnDeleteProjectDisabled(true);
    setConfirmDeleteProjectIsOpen(false);
  };

	const handleDeletePcb = async (e) => {
    setLoading(true);
    const request = {
      projectId: project.projectId,
			pcbId: selectedPcb.pcbId
    };
    const response = await fetchApi('api/bom/pcb', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    }).catch(() => {
		});
    if (response && response.responseObject.status === 200) {
			// remove pcb from list
			setProject({...project, pcbs: _.filter(project.pcbs, x => x.pcbId !== selectedPcb.pcbId)});
      toast.success(t('success.deletedPcb', 'Pcb was deleted!'));
    }
    else
      toast.error(t('error.failedDeletePcb', "Failed to remove pcb!"));
    setLoading(false);
    setConfirmDeletePcbIsOpen(false);
		setSelectedPcb(null);
  };

  const handleDeleteProduceHistory = async (e) => {
    setLoading(true);
    const request = {...selectedProduceHistory};
    const response = await fetchApi('api/bom/produce/history', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    }).catch(() => {
		});
    if (response && response.responseObject.status === 200) {
			// remove pcb from list
			setProject({...project, produceHistory: _.filter(project.produceHistory, x => x.projectProduceHistoryId !== selectedProduceHistory.projectProduceHistoryId)});
      toast.success(t('success.deletedRecord', 'Record was deleted!'));
    }
    else
      toast.error(t('error.failedDeleteRecord', "Failed to delete record!"));
    setLoading(false);
    setConfirmDeleteProduceHistoryIsOpen(false);
		setSelectedProduceHistory(null);
  };

	const inlineSave = async (pcb) => {
    pcb.cost = Number(pcb.cost).toFixed(2);
    const request = {
			...pcb,
      projectId: project.projectId,		
    };
    const response = await fetchApi('api/bom/pcb', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.status === 200) {
      const { data } = response;
      // update pcbs
      const newPcbs = [...project.pcbs];
      let existingPcb = _.find(newPcbs, x => x.pcbId === pcb.pcbId);
      existingPcb.cost = Number(data.cost).toFixed(2);
      existingPcb.quantity = data.quantity;
      existingPcb.name = data.name;
      existingPcb.description = data.description;
      existingPcb.serialNumberFormat = data.serialNumberFormat;
      existingPcb.lastSerialNumber = data.lastSerialNumber;
      existingPcb.partsCount = data.partsCount;
      setProject({...project, pcbs: newPcbs});
    }
    else {
      toast.error(t('error.failedSavePcb', "Failed to save pcb!"));
    }
  };

	const handleChange = (e, control) => {
    project[control.name] = control.value;
    setProject({ ...project });
  };

  const handleSaveProject = async (e) => {
    setLoading(true);
		setBtnDeleteProjectDisabled(true);
    const request = {...project};
    const response = await fetchApi("api/project", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.responseObject.ok) {
      const data = response.data;
			// redirect if name changed to new name
			props.history(`/project/${data.name}`);
			toast.success('Project saved!');
    } else {
      toast.error(t('error.failedSaveProject', "Failed to save project change!"));
    }
    setLoading(false);
		setBtnDeleteProjectDisabled(false);
  };

  const confirmDeleteProjectOpen = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteProjectIsOpen(true);
    setConfirmDeleteProjectContent(
      <p>
        {t('confirm.deleteProject', "Are you sure you want to delete this project and your entire BOM?")}
        <br />
        <br />
        <Trans i18nKey="confirm.permanent">
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmDeleteProjectClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteProjectIsOpen(false);
  };

	const confirmDeletePcbOpen = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
		setSelectedPcb(p);
    setConfirmDeletePcbIsOpen(true);
    setConfirmDeletePcbContent(
      <p>
        {t('confirm.deletePcb', "Are you sure you want to delete this PCB and it's parts from your BOM?")}
        <br />
        <br />
        <Trans i18nKey='confirm.permanent'>
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

  const confirmDeleteProduceHistoryOpen = (e, p) => {
    e.preventDefault();
    e.stopPropagation();
		setSelectedProduceHistory(p);
    setConfirmDeleteProduceHistoryIsOpen(true);
    setConfirmDeleteProduceHistoryContent(
      <p>
        {t('confirm.deleteHistory', "Are you sure you want to delete this history record?")}
        <br />
        <br />
        <Trans i18nKey='confirm.partsWillBeBackInStock'>
        All consumed parts will be placed <b>back in stock</b>.
        </Trans>
        <br />
        <br />
        <Trans i18nKey='confirm.permanent'>
        This action is <i>permanent and cannot be recovered</i>.
        </Trans>
      </p>
    );
  };

	const confirmDeletePcbClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeletePcbIsOpen(false);
		setSelectedPcb(null);
  };

  const confirmDeleteProduceHistoryClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeleteProduceHistoryIsOpen(false);
		setSelectedProduceHistory(null);
  };

	const focusColumn = (e) => {
    e.preventDefault();
    e.stopPropagation();
  };

	const saveColumn = async (e) => {
    changeTracker.forEach(async (val) => {
      const pcb = _.where(project.pcbs, { pcbId: val.pcbId }) || [];
      if (pcb.length > 0) await inlineSave(pcb[0]);
    });
    setChangeTracker([]);
  };

	const handleInlineChange = (e, control, pcb) => {
    pcb[control.name] = control.value;
    let changes = [...changeTracker];
    
    if (_.where(changes, { pcbId: pcb.pcbId }).length === 0)
      changes.push({ pcbId: pcb.pcbId });
    
    setProject({...project });
    setChangeTracker(changes);
  };

  const openPcbHistoryModal = (e, pcbHistory) => {
    e.preventDefault();
    e.stopPropagation();
    setPcbHistoryModalContext(pcbHistory);
    setPcbHistoryModalIsOpen(true);
  };

  return (
    <div className="contentwrapper">
			<Breadcrumb>
        <Breadcrumb.Section link onClick={() => props.history("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => props.history("/bom")}>{t('bc.boms', "BOMs")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => props.history(`/bom/${project.name}`)}>{t('bc.bom', "BOM")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.editProject', "Edit Project")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.project.title', "Edit Project")} to={`/bom/${project.name}`}>
      {t('page.project.description', "Projects are used as part of your BOM, allowing you to associate parts to multiple PCBs.")}
			</FormHeader>
      <Confirm
        className="confirm"
        header={t('page.project.confirm.deleteProjectHeader', "Delete Project")}
        open={confirmDeleteProjectIsOpen}
        onCancel={confirmDeleteProjectClose}
        onConfirm={handleDeleteProject}
        content={confirmDeleteProjectContent}
      />
			<Confirm
        className="confirm"
        header={t('page.project.confirm.deletePcbHeader', "Delete Pcb")}
        open={confirmDeletePcbIsOpen}
        onCancel={confirmDeletePcbClose}
        onConfirm={handleDeletePcb}
        content={confirmDeletePcbContent}
      />
      <Confirm
        className="confirm"
        header={t('page.project.confirm.deleteProduceHistoryHeader', "Delete History Record")}
        open={confirmDeleteProduceHistoryIsOpen}
        onCancel={confirmDeleteProduceHistoryClose}
        onConfirm={handleDeleteProduceHistory}
        content={confirmDeleteProduceHistoryContent}
      />
      <PcbHistoryModal isOpen={pcbHistoryModalIsOpen} onClose={() => setPcbHistoryModalIsOpen(false)} history={pcbHistoryModalContext} />

      <Form onSubmit={handleSaveProject}>
        <Segment raised disabled={pageDisabled}>
          <Form.Field width={4}>
						<Popup
                wide
                content={t('page.project.popup.name', "Enter the name of your project/BOM")}
                trigger={
                  <Form.Input required label={t('label.name', "Name")} placeholder={t('page.project.placeholder.name', "My Project")} name="name" value={project.name || ''} onChange={handleChange} />
                }
              />
					</Form.Field>
					<Popup
						wide
						content={t('page.project.popup.description', "Enter a description that summarizes your project")}
						trigger={
							<Form.Field width={6} control={TextArea} label={t('label.description', "Description")} value={project.description} onChange={handleChange} name='description' style={{height: '60px'}} />
						}
					/>
					<Form.Group>
						<Popup
							wide
							content={<p>
                <Trans i18nKey="page.project.popup.location">
                Your project's location <i>(optional)</i>
                </Trans>
              </p>}
							trigger={
								<Form.Input width={6} label={t('label.location', "Location")} placeholder={t('page.project.placeholder.location', "New York")} focus value={project.location} onChange={handleChange} name='location' />
							}
						/>
						<Popup
							wide
							content={t('page.project.popup.color', "Associate a color with this project for easy identification")}
							trigger={
								<Form.Dropdown width={4} label={t('label.color', "Color")} selection value={project.color} options={colors} onChange={handleChange} name='color' />
							}
						/>
					</Form.Group>
					<Button primary type="submit" disabled={btnSubmitDisabled}><Icon name="save" /> {t('button.save', "Save")}</Button>
					<Button onClick={confirmDeleteProjectOpen} disabled={btnDeleteDisabled}><Icon name="trash" /> {t('button.delete', "Delete")}</Button>
        </Segment>

				<Segment disabled={pageDisabled}>
					<h2>{t('page.project.pcbs', "PCBs")}</h2>
					<Table>
						<Table.Header>
							<Table.Row>
                <Table.HeaderCell width={1}></Table.HeaderCell>
								<Table.HeaderCell width={2}>{t('label.name', "Name")}</Table.HeaderCell>
								<Table.HeaderCell width={2}>{t('label.description', "Description")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.quantity', "Quantity")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.cost', "Cost")}</Table.HeaderCell>
								<Table.HeaderCell>{t('label.serialNumberFormat', "Serial Number Format")}</Table.HeaderCell>
								<Table.HeaderCell>{t('label.lastSerialNumber', "Last Serial Number")}</Table.HeaderCell>
								<Table.HeaderCell>{t('label.partsCount', "Parts Count")}</Table.HeaderCell>
								<Table.HeaderCell></Table.HeaderCell>
							</Table.Row>
						</Table.Header>
						<Table.Body>
							{project.pcbs.map((p, key) => (
								<Table.Row key={key}>
                  <Table.Cell width={1}>{p.storedFile && p.storedFile.fileName 
                    ? <Image src={`api/storedFile/preview?fileName=${p.storedFile.fileName}&token=${getImagesToken()}`} height={32} />
                    : <Image src="/image/pcb.png" height={32} />
                  }</Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='name' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='description' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.description || ''} fluid /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='quantity' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.quantity || ''} fluid /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='cost' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.cost || '0.00'} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='serialNumberFormat' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.serialNumberFormat || ''} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='lastSerialNumber' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.lastSerialNumber || ''} fluid /></Table.Cell>
									<Table.Cell>{p.partsCount}</Table.Cell>
									<Table.Cell><Button circular size='mini' icon='delete' title='Delete pcb' onClick={e => confirmDeletePcbOpen(e, p)} /></Table.Cell></Table.Row>
							))}
						</Table.Body>
					</Table>
				</Segment>

        <Segment disabled={pageDisabled} style={{minHeight: '100px'}}>
					<h2 id="history">{t('page.project.produceHistory', "Production History")}</h2>
					<Table>
						<Table.Header>
							<Table.Row>
								<Table.HeaderCell width={3}>{t('label.date', "Date")}</Table.HeaderCell>
                <Table.HeaderCell><Popup wide content={<p>{t('page.project.popup.quantityProduced', "The production quantity specified")}</p>} trigger={<div>{t('label.quantity', "Quantity")}</div>} /></Table.HeaderCell>
                <Table.HeaderCell><Popup wide content={<p>{t('page.project.popup.includeUnassociated', "Indicates if parts not associated with a PCB were produced")}</p>} trigger={<div>{t('label.includeUnassociated', "Include Unassociated")}</div>} /></Table.HeaderCell>
                <Table.HeaderCell><Popup wide content={<p>{t('page.project.popup.totalConsumed', "The total number of parts consumed")}</p>} trigger={<div>{t('label.consumed', "Consumed")}</div>} /></Table.HeaderCell>
								<Table.HeaderCell width={5}><Popup content={<p>{t('page.project.popup.pcbsProduced', "The Pcbs that were produced")}</p>} trigger={<div>{t('label.pcbs', "Pcbs")}</div>} /></Table.HeaderCell>
								<Table.HeaderCell width={2}></Table.HeaderCell>
							</Table.Row>
						</Table.Header>
						<Table.Body>
							{project.produceHistory.length > 0 
              ? project.produceHistory.map((p, key) => (
								<Table.Row key={key}>
									<Table.Cell>{format(parseJSON(p.dateCreatedUtc), FormatFullDateTime)}</Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='quantity' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.quantity || ''} fluid /></Table.Cell>
									<Table.Cell><Checkbox toggle checked={p.produceUnassociated} disabled /></Table.Cell>
                  <Table.Cell>{p.partsConsumed}</Table.Cell>
									<Table.Cell>{Object.keys(_.indexBy(p.pcbs, i => i.pcb.name)).join(', ')}</Table.Cell>
									<Table.Cell>
                    <Button circular size='mini' icon='edit' title='View PCBs' disabled={p.pcbs.length === 0} onClick={e => openPcbHistoryModal(e, p)} /> 
                    <Button circular size='mini' icon='delete' title='Delete produce record' onClick={e => confirmDeleteProduceHistoryOpen(e, p)} />
                  </Table.Cell>
								</Table.Row>
							))
            : (<Table.Row><Table.Cell colSpan={6} textAlign="center">{t('message.noResults', "No Results")}</Table.Cell></Table.Row>)}
						</Table.Body>
					</Table>
				</Segment>
      </Form>
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Project {...props} params={useParams()} history={useNavigate()} location={window.location} />;
