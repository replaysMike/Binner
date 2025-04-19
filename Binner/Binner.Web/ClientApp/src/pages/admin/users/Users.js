import React, { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Table, Form, Segment, Button, Icon, Popup, Confirm, Breadcrumb } from "semantic-ui-react";
import { fetchApi } from "../../../common/fetchApi";
import { InifiniteScrollTable } from "../../../components/InfiniteScrollTable";
import { generatePassword } from "../../../common/Utils";
import { AccountTypes, BooleanTypes, GetTypeDropdown } from "../../../common/Types";
import { getFriendlyElapsedTime, getTimeDifference, getFormattedTime } from "../../../common/datetime";
import { FormHeader } from "../../../components/FormHeader";
import ClearableInput from "../../../components/ClearableInput";
import { toast } from "react-toastify";
import { getUserAccount } from "../../../common/authentication";

export function Users(props) {
  const { t } = useTranslation();
  const maxResults = 20;
  const defaultAddUser = {
    name: "",
    emailAddress: "",
    phoneNumber: "",
    password: "",
    isAdmin: false,
    isEmailConfirmed: false
  };
  const [loading, setLoading] = useState(true);
  const [addVisible, setAddVisible] = useState(false);
  const [addUser, setAddUser] = useState(defaultAddUser);
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [hasMoreData, setHasMoreData] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [deleteSelectedItem, setDeleteSelectedItem] = useState(null);
  const accountTypes = GetTypeDropdown(AccountTypes);
  const emailConfirmedTypes = GetTypeDropdown(BooleanTypes);
  const usersDataRef = useRef([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetchUsers();
    function fetchUsers(page = 1) {
      if (hasMoreData) {
        setLoading(true);
        fetchApi(`/api/user/list?page=${page}&results=${maxResults}`).then((response) => {
          const { data } = response;
          if (data) {
            // update the page of data, as long as its not already in the data
            usersDataRef.current = data;
            setLoading(false);
          }
          if (data.length < maxResults) {
            // no more data, received back 0 or less than maxResults
            setHasMoreData(false);
          }
        });
      }
    }
  }, [hasMoreData]);

  const deleteUser = (e, user) => {
    e.preventDefault();
    e.stopPropagation();
    setLoading(true);
    fetchApi(`/api/user`, {
      method: "DELETE",
      body: user.userId
    }).then(() => {
      const newUsersData = _.filter(usersDataRef.current, (item) => item.userId !== user.userId);
      usersDataRef.current = [...newUsersData];
      setLoading(false);
      setConfirmDeleteIsOpen(false);
    });
  };

  const handleAddUser = (e, user) => {
    setLoading(true);
    const request = {
      name: user.name,
      emailAddress: user.emailAddress,
      password: user.password,
      phoneNumber: user.phoneNumber,
      isAdmin: user.isAdmin === 1 ? true : false,
      isEmailConfirmed: user.isEmailConfirmed === 1 ? true : false
    };
    fetchApi(`/api/user`, {
      method: "POST",
      body: JSON.stringify(request)
    }).then(() => {
      setLoading(false);
      setAddVisible(false);
      refreshClean();
      toast.success(t("page.admin.users.userAdded", "User {{emailAddress}} added!", { emailAddress: user.emailAddress }));
    });
  };

  const refreshClean = () => {
    // refresh
    usersDataRef.current = [];
    setAddUser(defaultAddUser);
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

  const fetchNextPage = () => {
    if (hasMoreData) setCurrentPage(currentPage + 1);
  };

  const handleChange = (e, control) => {
    addUser[control.name] = control.value;
    setAddUser({ ...addUser });
  };

  const handleShowAdd = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setAddVisible(!addVisible);
  };

  const headerRow = (
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
  );

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
      <Form onSubmit={(e) => handleAddUser(e, addUser)}>
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
                  <ClearableInput required label={t("label.name", "Name")} className="labeled" placeholder="John Doe" value={addUser.name} onChange={handleChange} name="name" />
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
                    value={addUser.emailAddress}
                    onChange={handleChange}
                    name="emailAddress"
                  />
                </Form.Field>
              }
            />
            <Form.Field width={10}>
              <Form.Input action required label={t("label.password", "Password")} placeholder="Set a password" value={addUser.password || ""} name="password" onChange={handleChange}>
                <input />
                <Button
                  onClick={(e) => {
                    e.preventDefault();
                    setAddUser({ ...addUser, password: generatePassword() });
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
                value={addUser.isAdmin || false}
                className={addUser.isAdmin ? "blue" : ""}
                name="isAdmin"
                options={accountTypes}
                onChange={handleChange}
              />
            </Form.Field>
            <Form.Group>
              <Form.Button primary type="submit" icon>
                <Icon name="save" /> {t("button.addUser", "Add User")}
              </Form.Button>
            </Form.Group>
          </Segment>
        )}
      </Form>

      <Segment loading={loading} secondary>
        <InifiniteScrollTable id="usersTable" compact celled sortable selectable striped unstackable size="small" headerRow={headerRow} nextPage={() => fetchNextPage()}>
          {usersDataRef.current.map((user, i) => (
            <Table.Row key={i} onClick={(e) => openUser(e, user)}>
              <Table.Cell>{user.userId}</Table.Cell>
              <Table.Cell style={{ textAlign: "center" }}>{user.isAdmin ? <Icon name="user secret" title="Admin" color="red" size="large" /> : <Icon name="user" title="Normal Account" />}</Table.Cell>
              <Table.Cell style={{ textAlign: "center" }}>
                {user.dateLockedUtc ? <Icon name="remove circle" color="red" title="Is Locked" /> : <Icon name="check circle" color="green" title="Not Locked" />}
              </Table.Cell>
              <Table.Cell>{user.name}</Table.Cell>
              <Table.Cell>{user.emailAddress}</Table.Cell>
              <Table.Cell>{user.dateLastActiveUtc !== null ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(user.dateLastActiveUtc)), true) : "(never)"}</Table.Cell>
              <Table.Cell>{getFormattedTime(user.dateCreatedUtc)}</Table.Cell>
              <Table.Cell>
                <Button
                  size="mini"
                  icon="delete"
                  disabled={user.userId === 1 || user.userId === getUserAccount().id}
                  onClick={(e) => confirmDeleteOpen(e, user)}
                  title={user.userId === 1 ? t("page.admin.users.canNotDeleteAdmin", "Can not delete master admin") : t("button.delete", "Delete")}
                />
              </Table.Cell>
            </Table.Row>
          ))}
        </InifiniteScrollTable>
      </Segment>
    </div>
  );
}
