import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { Form, Segment, Button, Icon, Confirm, Breadcrumb, Header, Flag } from "semantic-ui-react";
import { toast } from "react-toastify";
import { fetchApi, getErrorsString, getText } from "../../../common/fetchApi";
import { generatePassword } from "../../../common/Utils";
import { AccountTypes, BooleanTypes, GetTypeDropdown } from "../../../common/Types";
import { CustomFieldTypes } from "../../../common/customFieldTypes";
import { getFriendlyElapsedTime, getTimeDifference, getFormattedTime } from "../../../common/datetime";
import { FormHeader } from "../../../components/FormHeader";
import ClearableInput from "../../../components/ClearableInput";
import { getSystemSettings } from "../../../common/applicationSettings";
import { CustomFieldValues } from "../../../components/CustomFieldValues";

export function User(props) {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState({
    name: "",
    emailAddress: "",
    isAdmin: false,
    isEmailConfirmed: false,
    isLocked: false,
    partsInventoryCount: 0,
    partTypesCount: 0,
    projects: [],
    subscriptions: [],
    oAuthCredentials: [],
    oAuthRequests: [],
    payments: [],
    userIntegrationConfigurations: [],
    userPrinterConfigurations: [],
    userPrinterTemplateConfigurations: [],
    customFields: []
  });
  const [isDirty, setIsDirty] = useState(false);
  const [confirmDeleteIsOpen, setConfirmDeleteIsOpen] = useState(false);
  const [deleteSelectedItem, setDeleteSelectedItem] = useState(null);
  const [systemSettings, setSystemSettings] = useState({ currency: "USD", customFields: [] });

  const accountTypes = GetTypeDropdown(AccountTypes);
  const emailConfirmedTypes = GetTypeDropdown(BooleanTypes);
  const dateLockedTypes = GetTypeDropdown(BooleanTypes);

  const params = useParams();
  const { userId } = params;
  const navigate = useNavigate();

  useEffect(() => {
    const fetchUser = async () => {
      setLoading(true);
      getSystemSettings().then(async (systemSettings) => {
        setSystemSettings(systemSettings);
        
        await fetchApi(`/api/user?userId=${userId}`).then((response) => {
          if (response.responseObject.ok) {
            const { data } = response;
            if (data) {
              const newUser = { ...data, 
                isLocked: data.dateLockedUtc != null, 
                //customFields: _.filter(systemSettings?.customFields, x => x.customFieldTypeId === CustomFieldTypes.User.value)?.map((field) => ({ field: field.name, value: '' })) || [] 
              };
              setUser(newUser);
              setLoading(false);
            }
          }
        }).catch((err) => {
          if (err.responseObject.status === 404) {
            toast.error("User Not found!");
          } else if (err.responseObject.status === 400) {
            toast.error(err.data.message);
          } else {
            const errorMessage = getErrorsString(response);
            console.error(errorMessage);
            toast.error(errorMessage);
          }
        });
      });

      
    };

    fetchUser();
  }, []);

  const updateUser = async (e) => {
    if (user.isLocked && user.dateLockedUtc === null)
      user.dateLockedUtc = new Date();
    else if (!user.isLocked && user.dateLockedUtc !== null)
      user.dateLockedUtc = null;

    const userRequest = {
      ...user,
      isEmailConfirmed: user.isEmailConfirmed,
      isAdmin: user.isAdmin,
      dateLockedUtc: user.dateLockedUtc
    };

    await fetchApi(`/api/user`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(userRequest)
    }).then((response) => {
      if (response.responseObject.ok) {
        setIsDirty(false);
        toast.success("Saved user!");
        navigate(-1);
      } else if (response.responseObject.status === 400) {
        toast.error(response.data.message);
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
    });
  };

  const deleteUser = async (e, user) => {
    e.preventDefault();
    e.stopPropagation();
    setLoading(true);
    await fetchApi(`/api/user`, {
      method: "DELETE",
      body: user.userId
    }).then((response) => {
      setLoading(false);
      setConfirmDeleteIsOpen(false);
      if (response.responseObject.ok) {
        navigate(-1);
      } else if (response.responseObject.status === 400) {
        toast.error(response.data.message);
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
    });
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

  const handleCustomFieldChange = (e, control, field, fieldDefinition) => {
    e.preventDefault();
    e.stopPropagation();
    if (field) {
      field.value = control.value;
      const otherCustomFields = _.filter(user.customFields, x => x.field !== control.name);
      setUser({...user, customFields: [ ...otherCustomFields, field ] });
      setIsDirty(true);
    } else {
      console.error('field not found', control.name, user.customFields);
    }
  };

  const handleChange = (e, control) => {
    user[control.name] = control.value;
    setUser({ ...user });
    setIsDirty(true);
  };

  return (
    <div className="mask">
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin")}>{t('bc.admin', "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin/users")}>{t('bc.users', "Users")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.user', "User")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.users.title", "User Management")} to="..">
        <Trans i18nKey="page.admin.users.description">
          Administration of users.
        </Trans>
      </FormHeader>

      <Segment loading={loading} secondary>
        <Confirm
          className="confirm"
          open={confirmDeleteIsOpen}
          onCancel={confirmDeleteClose}
          onConfirm={(e) => deleteUser(e, deleteSelectedItem)}
          content={t('page.admin.users.confirm.areYouSure', "Are you sure you want to delete this user?")}
        />
        <Form onSubmit={updateUser}>
          {user.userId === 1 &&
            <Header as='h4'>{t('page.admin.users.masterAccount', "Master Admin Account")}</Header>
          }
          <ClearableInput required label={t('label.name', "Name")} focus placeholder="John Doe" value={user.name || ""} name="name" onChange={handleChange} />
          <ClearableInput required label={t('label.usernameEmail', "Username / Email")} iconPosition="left" placeholder="john@example.com" value={user.emailAddress || ""} name="emailAddress" onChange={handleChange}>
            <Icon name='user' />
            <input />
          </ClearableInput>
          <ClearableInput label={t('label.changePassword', "Change Password")} placeholder="Change existing password" action value={user.password || ""} name="password" onChange={handleChange}>
            <input />
            <Button onClick={(e) => { e.preventDefault(); setUser({ ...user, password: generatePassword() }); }}>{t('button.generate', "Generate")}</Button>
          </ClearableInput>
          <Form.Dropdown
            required
            label={t('label.accountType', "Account Type")}
            placeholder={t('label.accountType', "Account Type")}
            selection
            value={user.isAdmin || false}
            className={user.isAdmin ? "blue" : ""}
            name="isAdmin"
            options={accountTypes}
            onChange={handleChange}
          />
          <Form.Dropdown
            label={t('label.accountLocked', "Account Locked")}
            placeholder={t('label.accountLocked', "Account Locked")}
            selection
            value={user.isLocked || false}
            className={user.isLocked ? "red" : "green"}
            name="isLocked"
            options={dateLockedTypes}
            onChange={handleChange}
          />
          <Form.Group className="celled">
            <Form.Field>
              <label>{t('label.id', "Id")}</label>
              {user.userId}
            </Form.Field>
            <Form.Field>
              <label>{t('label.lastActive', "Last Active")}</label>
              <div>
                {user.dateLastActiveUtc !== null
                  ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(user.dateLastActiveUtc)), true)
                  : '(never)'}
              </div>
              <span className="small">{getFormattedTime(user.dateLastActiveUtc)}</span>
            </Form.Field>
            <Form.Field>
              <label>{t('label.lastLogin', "Last Login")}</label>
              <div>
                {user.dateLastLoginUtc !== null
                  ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(user.dateLastLoginUtc)), true)
                  : '(never)'}
              </div>
              <span className="small">{getFormattedTime(user.dateLastLoginUtc)}</span>
            </Form.Field>
            <Form.Field>
              <label>{t('label.ip', "IP")}</label>
              <div>
                {user.location && user.location.country &&
                  <Flag name={user.location.country.toLowerCase()} />
                }
                {user.ipAddress}
              </div>
              {user.location && <span className="small">{user.location.city}, {user.location.mostSpecificSubdivision}, {user.location.country}, {user.location.continent} {user.location.postal}</span>}
            </Form.Field>
            <Form.Field>
              <label>{t('label.dateCreated', "Date Created")}</label>
              <div>
                {user.dateCreatedUtc !== null
                  ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(user.dateCreatedUtc)), true)
                  : '(never)'}
              </div>
              <span className="small">{getFormattedTime(user.dateCreatedUtc)}</span>
            </Form.Field>
            <Form.Field>
              <label>{t('label.dateModified', "Date Modified")}</label>
              <div>
                {user.dateModifiedUtc !== null
                  ? getFriendlyElapsedTime(getTimeDifference(Date.now(), Date.parse(user.dateModifiedUtc)), true)
                  : '(never)'}
              </div>
              <span className="small">{getFormattedTime(user.dateModifiedUtc)}</span>
            </Form.Field>
            <Form.Field>
              <label>{t('label.dateLocked', "Date Locked")}</label>
              {getFormattedTime(user.dateLockedUtc)}
            </Form.Field>
          </Form.Group>
          <Form.Group>
            {_.filter(systemSettings.customFields, x => x.customFieldTypeId === CustomFieldTypes.User.value)?.length > 0 && <hr />}
            <CustomFieldValues 
              type={CustomFieldTypes.User}
              header={t('label.customFields', "Custom Fields")}
              headerElement="h3"
              customFieldDefinitions={systemSettings.customFields} 
              customFieldValues={user.customFields} 
              onChange={handleCustomFieldChange}
            />
          </Form.Group>

          <Button type="submit" primary disabled={!isDirty} style={{ marginTop: "10px" }}>
            <Icon name="save" />
            {t('button.save', "Save")}
          </Button>
          <Button type="button" title="Delete" onClick={(e) => confirmDeleteOpen(e, user)}>
            <Icon name="delete" />
            {t('button.delete', "Delete")}
          </Button>
        </Form>

      </Segment>
    </div>
  );
}