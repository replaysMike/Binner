import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Button, Form, Segment, Icon, Divider, Label } from "semantic-ui-react";
import { setUserAccount, deAuthenticateUserAccount } from "../common/authentication";
import { fetchApi, doNothing } from "../common/fetchApi";

export function Login (props) {
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [form, setForm] = useState({
    username: "",
    password: ""
  });
  let navigate = useNavigate();

  const onSubmit = async (e) => {
    e.preventDefault();

    setLoading(true);
    setErrorMessage("");

		await handleLogin(e);
  }

  const handleLogin = async () => {
    const request = {
      ...form
    };
    fetchApi("api/authentication/login", { 
      method: "POST", 
      body: request
    })
    .then((response) => {
      const { data } = response;
      if (data.isAuthenticated) {
        // set the current user account
        setUserAccount({
          id: data.id,
          isAuthenticated: data.isAuthenticated,
          name: data.name,
          subscriptionLevel: data.subscriptionLevel,
          accessToken: data.jwtToken,
          imagesToken: data.imagesToken,
          isAdmin: data.isAdmin
        });
        navigate("/", { replace: true, state: { } });
      } else {
        setErrorMessage(data.message);
        setLoading(false);
        //deAuthenticateUserAccount();
      }
    })
    .catch((ex) => {
			console.error('exception', ex);
		});
  };

  const handleChange = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    setForm(form => ({ 
      ...form,
      [e.target.name]: control.value 
    }));
  }

  return (
    <div>
      <div className="centered">
        <h1>Login</h1>
        <Segment raised className="half-width" style={{ margin: "auto", marginBottom: '50px' }}>
          <Form onSubmit={onSubmit} loading={loading} autoComplete="on">
            <Form.Input
              focus
              required
              autoComplete="email"
              name="username"
              value={form.username || ""}
              placeholder="Username"
              icon="user outline"
              iconPosition="left"
              onChange={handleChange}
            />
            <Form.Input
              required
              autoComplete="current-password"
              type="password"
              name="password"
              value={form.password || ""}
              placeholder="Password"
              icon="lock"
              iconPosition="left"
              onChange={handleChange}
            />
            <div className="centered">
              {errorMessage && errorMessage.length > 0 && (
                <div>
                  <Label basic pointing color="red" style={{ marginTop: -10 }}>
                    {errorMessage}
                  </Label>
                </div>
              )}
              <Button type="submit" primary style={{ marginTop: "10px" }}>
                LOGIN
                <Icon name="arrow circle right" />
              </Button>
            </div>
            <div style={{ marginTop: "15px" }}>
              <Link to="/passwordrecovery" className="small">Forgot your password?</Link>
            </div>
          </Form>
        </Segment>
      </div>
    </div>
  );
}
