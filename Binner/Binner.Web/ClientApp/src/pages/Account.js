import React, { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Button, Form, Segment, Icon, Label, Grid, Image, Popup } from "semantic-ui-react";
import { toast } from "react-toastify";
import { useDropzone } from "react-dropzone";
import axios from "axios";
import { config } from "../common/config";
import { formatNumber, MD5 } from "../common/Utils";
import { fetchApi, getErrorsString } from "../common/fetchApi";
import { getAuthToken } from "../common/authentication";

export function Account(props) {
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
  const navigate = useNavigate();
  const { acceptedFiles, isDragAccept, isDragReject, getRootProps, getInputProps } = useDropzone({
    maxFiles: 1,
    onDrop: (acceptedFiles) => {
      // do accept manually
      const acceptedMimeTypes = [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/bmp"
      ];
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
      fetchApi(`api/account`).then((response) => {
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
      fetchApi("api/authentication/identity").then((_) => {
        axios
          .request({
            method: "post",
            url: "api/account/upload",
            data: formData,
            headers: { Authorization: `Bearer ${getAuthToken()}` }
          })
          .then((data) => {
            if (data.status === 200){
              console.log("upload success", data);
            }else{
              console.log("upload failed", data);
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
    fetchApi(`api/account`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(account)
    }).then((response) => {
      if (response.responseObject.ok) {
        const { data } = response;
        console.log('account response', data);
        if (data.isSuccessful) {
          setIsDirty(false);
          toast.success("Account updated!");
          navigate(-1);
        } else {
          if (data.message === "Incorrect password.") setErrorPassword(true);
          toast.error(data.message || 'error');
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
      setPasswordErrorMessage("Passwords do not match");
    } else if (passwordErrorMessage.length > 0) {
      setPasswordErrorMessage(null);
    }
  };

  return (
    <div>
      <h1>Account Settings</h1>
      <p>Edit account settings</p>

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
                  <h5>Profile Image</h5>
                  <span style={{ fontSize: "0.6em" }}>Drag an image to upload</span>
                  <input {...getInputProps()} />
                </div>
              </Grid.Column>
              <Grid.Column>
                <Form.Input label="Name" required focus placeholder="John Doe" value={account.name || ""} name="name" onChange={handleChange} />
                <Form.Input
                  label="Email"
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
                <Popup
                    hideOnScroll
                    content="This is your private api key for the Swarm Api."
                    trigger={
                      <Form.Input
                        label="Swarm Api Key"
                        iconPosition="left"
                        value={(account.apiKey && account.apiKey.privateApiKey) || ""}
                        name="apiKey"
                      >
                        <Icon name="key" />
                        <input />
                        <Link to="/api" className="small" style={{lineHeight: '3.1em', marginLeft: '10px'}}>Manage</Link>
                      </Form.Input>
                    }
                  />
                
              </Grid.Column>
            </Grid.Row>
          </Grid>

          <Segment tertiary>
            <h4>Change Password</h4>
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

          <Form.Group className="celled">
            <Form.Field>
              <label>Inventory Count</label>
              {formatNumber(account.partsInventoryCount)}
            </Form.Field>
            <Form.Field>
              <label>Custom Part Types</label>
              {formatNumber(account.partTypesCount)}
            </Form.Field>
          </Form.Group>

          <Button type="submit" primary disabled={!isDirty} style={{ marginTop: "10px" }}>
            <Icon name="save" />
            Save
          </Button>
        </Form>
      </Segment>
    </div>
  );
}
