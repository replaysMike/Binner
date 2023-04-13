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
  Confirm
} from "semantic-ui-react";
import { FormHeader } from "../components/FormHeader";
import _ from "underscore";
import { fetchApi } from "../common/fetchApi";
import { ProjectColors } from "../common/Types";
import { toast } from "react-toastify";
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
    pcbs: []
  };
	const [loading, setLoading] = useState(true);
  const [project, setProject] = useState(defaultProject);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [confirmDeleteProjectIsOpen, setConfirmDeleteProjectIsOpen] = useState(false);
	const [confirmDeletePcbIsOpen, setConfirmDeletePcbIsOpen] = useState(false);
	const [btnDeleteProjectDisabled, setBtnDeleteProjectDisabled] = useState(true);
	const [changeTracker, setChangeTracker] = useState([]);
  const [confirmProjectDeleteContent, setConfirmDeleteProjectContent] = useState(null);
	const [confirmPcbDeleteContent, setConfirmDeletePcbContent] = useState(null);
	const [selectedPcb, setSelectedPcb] = useState(null);
	const [btnSubmitDisabled, setBtnSubmitDisabled] = useState(false);
	const [btnDeleteDisabled, setBtnDeleteDisabled] = useState(false);
	const [pageDisabled, setPageDisabled] = useState(false);
	
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
    const response = await fetchApi(`bom?name=${projectName}`)
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
    const response = await fetchApi('project', {
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
    const response = await fetchApi('bom/pcb', {
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

	const inlineSave = async (pcb) => {
    pcb.cost = Number(pcb.cost).toFixed(2);
    const request = {
			...pcb,
      projectId: project.projectId,		
    };
    const response = await fetchApi('bom/pcb', {
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
    const response = await fetch("project", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    });
    if (response.ok) {
      const data = await response.json();
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

	const confirmDeletePcbClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setConfirmDeletePcbIsOpen(false);
		setSelectedPcb(null);
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

  return (
    <div>
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
        content={confirmProjectDeleteContent}
      />
			<Confirm
        className="confirm"
        header={t('page.project.confirm.deletePcbHeader', "Delete Pcb")}
        open={confirmDeletePcbIsOpen}
        onCancel={confirmDeletePcbClose}
        onConfirm={handleDeletePcb}
        content={confirmPcbDeleteContent}
      />

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
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='name' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.name || ''} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='description' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.description || ''} fluid /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='quantity' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.quantity || ''} fluid /></Table.Cell>
                  <Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='cost' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.cost || '0.00'} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='serialNumberFormat' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.serialNumberFormat || ''} fluid /></Table.Cell>
									<Table.Cell><Input labelPosition='left' className="inline-editable" transparent type='text' name='lastSerialNumber' onFocus={focusColumn} onClick={focusColumn} onBlur={e => saveColumn(e, p)} onChange={(e, control) => handleInlineChange(e, control, p)} value={p.lastSerialNumber || ''} fluid /></Table.Cell>
									<Table.Cell>{p.partsCount}</Table.Cell>
									<Table.Cell><Button circular size='mini' icon='delete' title='Delete pcb' onClick={e => confirmDeletePcbOpen(e, p)} /></Table.Cell>
								</Table.Row>
							))}
						</Table.Body>
					</Table>
				</Segment>
      </Form>
    </div>
  );
}

// eslint-disable-next-line import/no-anonymous-default-export
export default (props) => <Project {...props} params={useParams()} history={useNavigate()} location={window.location} />;
