import React, { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useTranslation, Trans } from "react-i18next";
import { Button, Form, Segment, Icon, Label, Grid, Image, Breadcrumb, Popup, Table, Confirm } from "semantic-ui-react";
import { toast } from "react-toastify";
import { useDropzone } from "react-dropzone";
import axios from "axios";
import { config } from "../common/config";
import { MD5 } from "../common/Utils";
import { fetchApi, getErrorsString } from "../common/fetchApi";
import { Hide } from "../components/Hide";
import { FormHeader } from "../components/FormHeader";
import { getAuthToken } from "../common/authentication";
import { AddTokenModal } from "../components/modals/AddTokenModal";
import { format, parseJSON } from "date-fns";
import { FormatShortDate } from "../common/datetime";
import { Clipboard } from "../components/Clipboard";
import { UserTokenType } from "../common/UserTokenType";
import { GetTypeName } from "../common/Types";
import _ from "underscore";

export function Account(props) {
  const { t } = useTranslation();
  const [addTokenIsOpen, setAddTokenIsOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [account, setAccount] = useState({
    name: "",
    emailAddress: "",
    partsInventoryCount: 0,
    partTypesCount: 0,
    subscriptions: [],
    payments: [],
    profileImage: null
  });
  const [isDirty, setIsDirty] = useState(false);
  const [image, setImage] = useState(null);
  const [errorPassword, setErrorPassword] = useState(false);
  const [passwordErrorMessage, setPasswordErrorMessage] = useState(null);
  const [confirmDeleteTokenIsOpen, setConfirmDeleteTokenIsOpen] = useState(false);
  const [deleteTokenSelectedItem, setDeleteTokenSelectedItem] = useState(null);
  const navigate = useNavigate();
  const { acceptedFiles, isDragAccept, isDragReject, getRootProps, getInputProps } = useDropzone({
    maxFiles: 1,
    onDrop: (acceptedFiles) => {
      // do accept manually
      const acceptedMimeTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp"];
      let errorMsg = "";
      for (let i = 0; i < acceptedFiles.length; i++) {
        if (!acceptedMimeTypes.includes(acceptedFiles[i].type)) {
          errorMsg += `File '${acceptedFiles[i].name}' with mime type '${acceptedFiles[i].type}' is not an accepted image type!\r\n`;
        }
      }
      if (errorMsg.length > 0) {
        toast.error(errorMsg);
      } else {
        setImage(URL.createObjectURL(acceptedFiles[0]));
        setIsDirty(true);
      }
    }
  });

  useEffect(() => {
    fetchUser();

    function fetchUser() {
      setLoading(true);
      fetchApi(`/api/account`).then((response) => {
        const { data } = response;
        if (data) {
          setAccount(data);
          setLoading(false);
        }
      });
    }
  }, []);

  const updateAccount = (e) => {
    if (acceptedFiles && acceptedFiles.length > 0) {
      const formData = new FormData();
      formData.append("file", acceptedFiles[0], acceptedFiles[0].name);
      // first fetch some data using fetchApi, to leverage 401 token refresh
      fetchApi("/api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: "/api/account/upload",
            data: formData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((data) => {
            if (data.status === 200) {
              console.debug("upload success", data);
            } else {
              console.debug("upload failed", data);
            }
          })
          .catch((error) => {
            toast.dismiss();
            console.error("error", error);
            toast.error(`upload failed!`);
          });
      });
    }

    setErrorPassword(false);
    fetchApi(`/api/account`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(account)
    }).then((response) => {
      if (response.responseObject.ok) {
        const { data } = response;
        console.debug("account response", data);
        if (data.isSuccessful) {
          setIsDirty(false);
          toast.success(t("success.accountUpdated", "Account updated!"));
          navigate(-1);
        } else {
          if (data.message === "Incorrect password.") setErrorPassword(true);
          toast.error(data.message || "error");
        }
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
    });
  };

  const handleChange = (e, control) => {
    account[control.name] = control.value;
    setAccount({ ...account });
    setIsDirty(true);
  };

  const handlePasswordChange = (e, control) => {
    account[control.name] = control.value;
    setAccount({ ...account });
    setIsDirty(true);
    if (account.newPassword !== account.confirmNewPassword) {
      setPasswordErrorMessage(t("error.passwordsDoNotMatch", "Passwords do not match"));
    } else if (passwordErrorMessage.length > 0) {
      setPasswordErrorMessage(null);
    }
  };

  const handleResetPreferences = (e) => {
    e.preventDefault();
    e.stopPropagation();
    // todo: move these to a centralized location
    localStorage.removeItem(`doNotAskAgain-inventoryConfirmDiscard`);
    localStorage.removeItem(`showWelcome`);
    localStorage.removeItem(`partsGridViewPreferences`);
    localStorage.removeItem(`i18nextLng`);
    localStorage.removeItem(`inventory`);
    toast.info(t("success.uiPrefsReset", "UI preferences have been reset to default values."));
  };

  const handleOpenCreateTokenModal = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setAddTokenIsOpen(true);
  };

  const handleCreateNewToken = (e, form) => {
    const request = {
      tokenType: form.tokenType,
      tokenConfig: form.tokenConfig
    };
    fetchApi(`/api/account/token`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    }).then((response) => {
      if (response.responseObject.ok) {
        const { data } = response;
        // remove any tokens of the same type, only 1 allowed per user
        account.tokens = _.filter(account.tokens, (item) => item.tokenType !== request.tokenType);
        account.tokens.push(data);
        setAccount(account);
        toast.success(t("success.tokenCreated", "Token created!"));
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
      setAddTokenIsOpen(false);
    });
  };

  const deleteToken = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    setLoading(true);
    const request = {
      tokenType: deleteTokenSelectedItem.tokenType,
      value: deleteTokenSelectedItem.value,
    }
    await fetchApi(`/api/account/token`, {
      method: "DELETE",
      body: JSON.stringify(request)
    }).then((response) => {
      if (response.responseObject.ok) {
        account.tokens = _.filter(account.tokens, (item) => item.value !== request.value);
        setAccount(account);
        toast.success("Token was deleted.");
      } else if (response.responseObject.status === 404) {
        toast.error("Token not found!");
      } else if (response.responseObject.status === 400) {
        toast.error(response.data.message);
      } else {
        const errorMessage = getErrorsString(response);
        console.error(errorMessage);
        toast.error(errorMessage);
      }
      setLoading(false);
      setConfirmDeleteTokenIsOpen(false);
    });
  };

  const confirmDeleteTokenClose = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setDeleteTokenSelectedItem(null);
    setConfirmDeleteTokenIsOpen(false);
  };

  const confirmDeleteTokenOpen = (e, token) => {
    e.preventDefault();
    e.stopPropagation();
    setDeleteTokenSelectedItem(token);
    setConfirmDeleteTokenIsOpen(true);
  };

  const handleCreateNewTokenClose = (e) => {
    setAddTokenIsOpen(false);
  };

  const handleDownloadKiCadToken = (e, token) => {
    e.preventDefault();
    e.stopPropagation();
    fetchApi("/api/authentication/identity").then((_) => {
      axios
        .request({
          method: "get",
          url: `/api/download/kicad?token=${token}`,
          headers: { Authorization: `Bearer ${getAuthToken()}` },
          responseType: "blob"
        })
        .then((blob) => {
          // specifying blob filename, must create an anchor tag and use it as suggested: https://stackoverflow.com/questions/19327749/javascript-blob-filename-without-link
          var file = window.URL.createObjectURL(blob.data);
          var a = document.createElement("a");
          document.body.appendChild(a);
          a.style = "display: none";
          a.href = file;
          const today = new Date();
          a.download = `Binner.kicad_httplib`;
          a.click();
          window.URL.revokeObjectURL(file);
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(t('message.downloadFailed', "Download failed!"));
        });
    });
  };

  return (
    <div>
      <Confirm
        className="confirm"
        open={confirmDeleteTokenIsOpen}
        onCancel={confirmDeleteTokenClose}
        onConfirm={deleteToken}
        content={<p>{t("page.account.confirm.deleteToken", "Are you sure you want to delete this token?")}</p>}
      />
      <AddTokenModal isOpen={addTokenIsOpen} onAdd={handleCreateNewToken} onClose={handleCreateNewTokenClose} />
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>
          {t("bc.home", "Home")}
        </Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.accountSettings", "Account Settings")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.accountSettings.title", "Account Settings")} to="..">
        <Trans i18nKey="page.accountSettings.description">Edit account settings</Trans>
      </FormHeader>

      <Segment loading={loading} secondary>
        <Form onSubmit={updateAccount}>
          <Grid columns={2}>
            <Grid.Row>
              <Grid.Column width={5}>
                <div {...getRootProps({ className: "centered dropzone " + (isDragReject ? "rejected" : "") + (isDragAccept ? "accepted" : "") })}>
                  <div style={{ width: "250px", height: "250px", padding: "15px" }}>
                    <aside className="centered small">{acceptedFiles && acceptedFiles.length > 0 && "PREVIEW"}</aside>
                    {image !== null ? (
                      <Image src={image} className="profileimage" />
                    ) : account && account.profileImage && account.profileImage !== null && account.profileImage.length > 0 ? (
                      <Image className="profileimage" src={`data:image/png;base64,${account.profileImage}`} />
                    ) : (
                      <Image
                        className="profileimage"
                        src={`https://www.gravatar.com/avatar/${MD5(account.emailAddress)}?s=250&d=${config.STATIC_URL}/images/userprofile-square.png`}
                      />
                    )}
                  </div>
                  <h5>{t("page.profileImage", "Profile Image")}</h5>
                  <span style={{ fontSize: "0.6em" }}>{t("page.accountSettings.dragImage", "Drag an image to upload")}</span>
                  <input {...getInputProps()} />
                </div>
              </Grid.Column>
              <Grid.Column>
                <Form.Input label="Name" required focus placeholder="John Doe" value={account.name || ""} name="name" onChange={handleChange} />
                <Form.Input
                  label="Username/Email"
                  iconPosition="left"
                  required
                  placeholder="john@example.com"
                  value={account.emailAddress || ""}
                  name="emailAddress"
                  onChange={handleChange}
                >
                  <Icon name="at" />
                  <input />
                </Form.Input>
                <Form.Input
                  label="Phone"
                  iconPosition="left"
                  placeholder="555-555-1212"
                  value={account.phoneNumber || ""}
                  name="phoneNumber"
                  onChange={handleChange}
                  autoComplete="phone"
                >
                  <Icon name="phone" />
                  <input />
                </Form.Input>
              </Grid.Column>
            </Grid.Row>
          </Grid>

          <Segment tertiary>
            <h4>{t("label.changePassword", "Change Password")}</h4>
            <Form.Input
              type="password"
              label="Current Password"
              placeholder=""
              value={account.password || ""}
              name="password"
              onChange={handleChange}
              autoComplete="current-password"
              error={errorPassword}
            />
            <Form.Input
              type="password"
              label="New Password"
              placeholder=""
              value={account.newPassword || ""}
              name="newPassword"
              onChange={handlePasswordChange}
              autoComplete="new-password"
            />
            <Form.Input
              type="password"
              label="Confirm Password"
              placeholder=""
              value={account.confirmNewPassword || ""}
              name="confirmNewPassword"
              onChange={handlePasswordChange}
              autoComplete="new-password"
            />
            {passwordErrorMessage && passwordErrorMessage.length > 0 && (
              <div>
                <Label basic pointing color="red" style={{ marginTop: -10 }}>
                  {passwordErrorMessage}
                </Label>
              </div>
            )}
          </Segment>

          <Segment color="blue">
            <h2>User Tokens</h2>
            <div style={{float: 'right', marginBottom: '10px'}}>
              <Button type="button" onClick={handleOpenCreateTokenModal}><Icon name="plus" /> Create</Button>
            </div>
            <Table compact celled striped size="small">
              <Table.Header>
                <Table.Row>
                  <Table.HeaderCell>Type</Table.HeaderCell>
                  <Table.HeaderCell>Token</Table.HeaderCell>
                  <Table.HeaderCell>Created</Table.HeaderCell>
                  <Table.HeaderCell>Expires</Table.HeaderCell>
                  <Table.HeaderCell></Table.HeaderCell>
                  <Table.HeaderCell></Table.HeaderCell>
                </Table.Row>
              </Table.Header>
              <Table.Body>
                {account.tokens?.length > 0
                  ? account.tokens.map((token, key) => (
                    <Table.Row key={key}>
                      <Table.Cell>{GetTypeName(UserTokenType, token.tokenType)}</Table.Cell>
                      <Table.Cell>
                        <div className="token" name="token">{token.value}</div>
                        <div style={{float: 'right'}}>
                          <Clipboard text={token.value} style={{marginRight: '10px'}} />
                          <Hide element="token" />
                        </div>
                        </Table.Cell>
                      <Table.Cell>{format(parseJSON(token.dateCreatedUtc), FormatShortDate)}</Table.Cell>
                      <Table.Cell>{token.dateExpiredUtc ? format(parseJSON(token.dateExpiredUtc), FormatShortDate) : "Never"}</Table.Cell>
                      <Table.Cell textAlign="center">
                        {token.tokenType === UserTokenType.KiCadApiToken.value && 
                          <Popup 
                            wide
                          content={<p>Download the KiCad configuration file and within KiCad add it to the Symbol Library<br /><span className="small">(KiCad <Icon name="arrow right" size='small' /> Preferences <Icon name="arrow right" size='small' /> Manage Symbol Libraries... <Icon name="arrow right" size='small' /> <Icon name="folder" color="grey" />)</span></p>}
                            trigger={<Link to={`/api/download/kicad?token=${token.value}`} onClick={e => handleDownloadKiCadToken(e, token.value)}><Icon name="download" /> Download Config</Link>} 
                          />
                        }
                      </Table.Cell>
                      <Table.Cell textAlign="center">
                        <Button
                          size="mini"
                          icon="delete"
                          onClick={(e) => confirmDeleteTokenOpen(e, token)}
                          title={t("button.delete", "Delete")}
                        />
                      </Table.Cell>
                    </Table.Row>
                  ))
                  : <Table.Row><Table.Cell colSpan={6} textAlign="center">No tokens available.</Table.Cell></Table.Row>}
              </Table.Body>
            </Table>
          </Segment>

          <Button type="submit" primary disabled={!isDirty} style={{ marginTop: "10px" }}>
            <Icon name="save" />
            {t("button.save", "Save")}
          </Button>

          <Popup
            wide
            content={<p>{t("page.accountSettings.popup.resetPreferences", "Reset remembered UI preferences to their default values.")}</p>}
            trigger={<Button type="button" style={{ marginTop: "10px" }} onClick={handleResetPreferences}>
              <Icon name="refresh" />
              {t('button.resetPreferences', "Reset Preferences")}
            </Button>}
          />
        </Form>
      </Segment>
    </div>
  );
}
