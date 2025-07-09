import { useState, useEffect, useRef, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Table, Form, Segment, Button, Icon, Popup, Confirm, Breadcrumb, Pagination } from "semantic-ui-react";
import { fetchApi, getErrorsString } from "../../../common/fetchApi";
import { generatePassword } from "../../../common/Utils";
import { AccountTypes, GetTypeDropdown } from "../../../common/Types";
import { getFriendlyElapsedTime, getTimeDifference, getFormattedTime } from "../../../common/datetime";
import { FormHeader } from "../../../components/FormHeader";
import ClearableInput from "../../../components/ClearableInput";
import { CustomFieldTypes } from "../../../common/customFieldTypes";
import { getSystemSettings } from "../../../common/applicationSettings";
import { CustomFieldValues } from "../../../components/CustomFieldValues";
import { toast } from "react-toastify";

export function Users() {
  const { t } = useTranslation();
  const maxResults = 20;
  const defaultNewUser = {
    name: "",
    emailAddress: "",
    phoneNumber: "",
    password: "",
    isAdmin: false,
    isEmailConfirmed: false,
    customFields: []
  };
  const [systemSettings, setSystemSettings] = useState({ currency: "USD", customFields: [] });
  const [loading, setLoading] = useState(true);
  const [addVisible, setAddVisible] = useState(false);
  const [newUser, setNewUser] = useState(defaultNewUser);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [hasMoreData, setHasMoreData] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalRecords, setTotalRecords] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [deleteSelectedItem, setDeleteSelectedItem] = useState(null);
  const accountTypes = GetTypeDropdown(AccountTypes);
  const usersDataRef = useRef([]);
  const navigate = useNavigate();

  const fetchUsers = async (page = 1) => {
    if (hasMoreData) {
      setLoading(true);
      await fetchApi(`/api/user/list?page=${page}&results=${maxResults}`).then((response) => {
        const { data } = response;
        if (data) {
          // update the page of data, as long as its not already in the data
          usersDataRef.current = data.items;
          setTotalRecords(data.totalItems);
          setTotalPages(data.totalPages);
          setLoading(false);
        }
        if (data.length < maxResults) {
          // no more data, received back 0 or less than maxResults
          setHasMoreData(false);
        }
      });
    }
  };

  useEffect(() => {
    getSystemSettings().then(async (systemSettings) => {
      setSystemSettings(systemSettings);
      // map defined custom fields to the new user object
      const newUser = { ...defaultNewUser, customFields: _.filter(systemSettings?.customFields, x => x.customFieldTypeId === CustomFieldTypes.User.value)?.map((field) => ({ field: field.name, value: '' })) || [] };
      setNewUser(newUser);
      await fetchUsers(1);
    });
  }, []);


  const deleteUser = async (e, user) => {
    e.preventDefault();
    e.stopPropagation();
    setLoading(true);
    await fetchApi(`/api/user`, {
      method: "DELETE",
      body: user.userId
    }).then((response) => {
      if (response.responseObject.ok) {
        const newUsersData = _.filter(usersDataRef.current, (item) => item.userId !== user.userId);
        usersDataRef.current = [...newUsersData];
      } else if (response.responseObject.status === 404) {
        toast.error("User not found!");
      } else if (response.responseObject.status === 400) {
        toast.error(response.data.message);
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
      setLoading(false);
      setConfirmDeleteIsOpen(false);
    });
  };

  const onCreateUser = async (e) => {
    setLoading(true);
    const request = {
      ...newUser,
      isAdmin: newUser.isAdmin === 1 ? true : false,
      isEmailConfirmed: newUser.isEmailConfirmed === 1 ? true : false
    };
    await fetchApi(`/api/user`, {
      method: "POST",
      body: JSON.stringify(request)
    }).then(() => {
      setLoading(false);
      setAddVisible(false);
      refreshClean();
      toast.success(t("page.admin.users.userAdded", "User {{emailAddress}} added!", { emailAddress: newUser.emailAddress }));
    }).catch((ex) => {
      const { data, responseObject } = ex;
      if (responseObject.status === 426) {
        // license err will be handled
        console.info('License requirement exceeded.', data.message);
        return;
      }
      console.error('Unexpected server error', ex);
      toast.error(`Server returned ${responseObject.status} error.`);
    });
  };

  const refreshClean = () => {
    // refresh
    usersDataRef.current = [];
    // map defined custom fields to the new user object
    setNewUser({ ...defaultNewUser, customFields: _.filter(systemSettings?.customFields, x => x.customFieldTypeId === CustomFieldTypes.User.value)?.map((field) => ({ field: field.name, value: '' })) || [] });
    setHasMoreData(true);
    setCurrentPage(1);
  };

  const openUser = (e, user) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(`${user.userId}`);
  };

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      usersDataRef.current = _.sortBy(usersDataRef.current, [clickedColumn]);
      setDirection("ascending");
    } else {
      usersDataRef.current = usersDataRef.current.reverse();
      setDirection(direction === "ascending" ? "descending" : "ascending");
    }
  };

  const confirmDeleteOpen = (e, user) => {
    e.preventDefault();
    e.stopPropagation();
    setDeleteSelectedItem(user);
    setConfirmDeleteIsOpen(true);
  };

  const confirmDeleteClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setDeleteSelectedItem(null);
    setConfirmDeleteIsOpen(false);
  };

  const handleChange = useCallback((e, control) => {
    newUser[control.name] = control.value;
    setNewUser({ ...newUser });
  }, [newUser]);

  const handleCustomFieldChange = useCallback((e, control, field, fieldDefinition) => {
    e.preventDefault();
    e.stopPropagation();
    if (field) {
      field.value = control.value;
      const otherCustomFields = _.filter(newUser.customFields, x => x.field !== control.name);
      setNewUser({ ...newUser, customFields: [ ...otherCustomFields, field ] });
      //setIsDirty(true);
    } else {
      console.error('field not found', control.name, newUser.customFields);
    }
  }, [newUser]);

  const handleShowAdd = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setAddVisible(!addVisible);
  };

  const handleInternalPageChange = async (e, control) => {
    setCurrentPage(control.activePage);
    await fetchUsers(control.activePage);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>
          {t("bc.home", "Home")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin")}>
          {t("bc.admin", "Admin")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.users", "Users")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.users.title", "User Management")} to="..">
        <Trans i18nKey="page.admin.users.description">Administration of users.</Trans>
      </FormHeader>

      <Confirm
        className="confirm"
        open={confirmDeleteIsOpen}
        onCancel={confirmDeleteClose}
        onConfirm={(e) => deleteUser(e, deleteSelectedItem)}
        content={<p>{t("page.admin.users.confirm.areYouSure", "Are you sure you want to delete this user?")}</p>}
      />
      <Form onSubmit={onCreateUser}>
        <Form.Group>
          <Button type="button" onClick={handleShowAdd} icon size="mini">
            <Icon name="file" /> {t("button.addUser", "Add User")}
          </Button>
        </Form.Group>
        {addVisible && (
          <Segment secondary>
            <Popup
              hideOnScroll
              content={t("page.admin.users.popup.name", "Specify the user's first and last name")}
              trigger={
                <Form.Field width={10}>
                  <ClearableInput required label={t("label.name", "Name")} className="labeled" placeholder="John Doe" value={newUser.name} onChange={handleChange} name="name" />
                </Form.Field>
              }
            />
            <Popup
              hideOnScroll
              content={t("page.admin.users.popup.emailAddress", "Specify the user's username/email")}
              trigger={
                <Form.Field width={10}>
                  <ClearableInput
                    required
                    label={t("label.usernameEmail", "Username / Email")}
                    className="labeled"
                    placeholder="john@example.com"
                    value={newUser.emailAddress}
                    onChange={handleChange}
                    name="emailAddress"
                  />
                </Form.Field>
              }
            />
            <Form.Field width={10}>
              <Form.Input action required label={t("label.password", "Password")} placeholder="Set a password" value={newUser.password || ""} name="password" onChange={handleChange}>
                <input />
                <Button
                  onClick={(e) => {
                    e.preventDefault();
                    setNewUser({ ...newUser, password: generatePassword() });
                  }}
                >
                  {t("button.generate", "Generate")}
                </Button>
              </Form.Input>
            </Form.Field>
            <Form.Field width={10}>
              <Form.Dropdown
                required
                label={t("page.admin.users.accountType", "Account Type")}
                placeholder={t("page.admin.users.accountType", "Account Type")}
                selection
                value={newUser.isAdmin || false}
                className={newUser.isAdmin ? "blue" : ""}
                name="isAdmin"
                options={accountTypes}
                onChange={handleChange}
              />
            </Form.Field>
            <Form.Group>
              {_.filter(systemSettings.customFields, x => x.customFieldTypeId === CustomFieldTypes.User.value)?.length > 0 && <hr />}
              <CustomFieldValues 
                type={CustomFieldTypes.User}
                header={t('label.customFields', "Custom Fields")}
                headerElement="h3"
                customFieldDefinitions={systemSettings.customFields} 
                customFieldValues={newUser.customFields} 
                onChange={handleCustomFieldChange}
              />
            </Form.Group>
            <Form.Group>
              <Form.Button primary type="submit" icon>
                <Icon name="save" /> {t("button.addUser", "Add User")}
              </Form.Button>
            </Form.Group>
          </Segment>
        )}
      </Form>

      <Segment loading={loading} secondary>
        <Table id="usersTable" compact celled sortable selectable striped unstackable size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell sorted={column === "userId" ? direction : null} onClick={handleSort("userId")}>
                {t("label.id", "Id")}
              </Table.HeaderCell>
              <Table.HeaderCell sorted={column === "isAdmin" ? direction : null} onClick={handleSort("isAdmin")}>
                {t("label.type", "Type")}
              </Table.HeaderCell>
              <Table.HeaderCell sorted={column === "dateLockedUtc" ? direction : null} onClick={handleSort("dateLockedUtc")}>
                {t("label.allowLogin", "Allow Login")}
              </Table.HeaderCell>
              <Table.HeaderCell sorted={column === "name" ? direction : null} onClick={handleSort("name")}>
                {t("label.name", "Name")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ maxWidth: "250px" }} sorted={column === "emailAddress" ? direction : null} onClick={handleSort("emailAddress")}>
                {t("label.usernameEmail", "Username / Email")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ maxWidth: "130px" }} sorted={column === "dateLastActiveUtc" ? direction : null} onClick={handleSort("dateLastActiveUtc")}>
                {t("label.lastActive", "Last Active")}
              </Table.HeaderCell>
              <Table.HeaderCell style={{ maxWidth: "130px" }} sorted={column === "dateAddedUtc" ? direction : null} onClick={handleSort("dateAddedUtc")}>
                {t("label.dateAdded", "Date Added")}
              </Table.HeaderCell>
              <Table.HeaderCell></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
          {usersDataRef.current.map((userRow, i) => (
            <Table.Row key={i} onClick={(e) => openUser(e, userRow)}>
              <Table.Cell>{userRow.userId}</Table.Cell>
              <Table.Cell style={{ textAlign: "center" }}>{userRow.isAdmin ? <Icon name="user secret" title="Admin" color="red" size="large" /> : <Icon name="user" title="Normal Account" />}</Table.Cell>
              <Table.Cell style={{ textAlign: "center" }}>
                {userRow.dateLockedUtc ? <Icon name="remove circle" color="red" title="Is Locked" /> : <Icon name="check circle" color="green" title="Not Locked" />}
              </Table.Cell>
              <Table.Cell>{userRow.name}</Table.Cell>
              <Table.Cell>{userRow.emailAddress}</Table.Cell>
              <Table.Cell>{userRow.dateLastActiveUtc !== null ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(userRow.dateLastActiveUtc)), true) : "(never)"}</Table.Cell>
              <Table.Cell>{getFormattedTime(userRow.dateCreatedUtc)}</Table.Cell>
              <Table.Cell textAlign="center">
                <Button
                  size="mini"
                  icon="delete"
                  onClick={(e) => confirmDeleteOpen(e, userRow)}
                  title={t("button.delete", "Delete")}
                />
              </Table.Cell>
            </Table.Row>
          ))}
          </Table.Body>
        </Table>
        <div className="small" style={{ float: 'right' }}>{t("label.totalRecords", "Total records:")} {totalRecords}</div>
        <Pagination
          activePage={currentPage}
          totalPages={totalPages}
          firstItem={null}
          lastItem={null}
          onPageChange={handleInternalPageChange}
          size="mini"
          style={{ marginTop: '5px' }}
        />
      </Segment>
    </div>
  );
}
