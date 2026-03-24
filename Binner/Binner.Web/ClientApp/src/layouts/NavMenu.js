import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from "reactstrap";
import { Form, Menu, Dropdown, Icon, Popup } from "semantic-ui-react";
import { isAuthenticated, isAdmin, getSubscriptionTag, logoutUserAccountAsync, deAuthenticateUserAccount, getUserAccount, setUserAccount } from "../common/authentication";
import ProtectedInput from "../components/ProtectedInput";
import SystemHubProvider, { SystemHubEvents } from '../common/systemHub';
import "./NavMenu.css";

/**
 * Top Navigation menu
 */
export function NavMenu() {
  const { t } = useTranslation();
  const [collapsed, setCollapsed] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState("");
  const [currentUser, setCurrentUser] = useState(getUserAccount());
  const navigate = useNavigate();

  const toggleNavbar = () => {
    setCollapsed(!collapsed);
  };

  const handleChange = (e, control) => {
    setSearchKeyword(control.value);
  };

  const logout = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    await logoutUserAccountAsync().then(async (response) => {
      deAuthenticateUserAccount();
      // navigate("/", { replace: true, state: { } });
      setCurrentUser(null);
      window.location.href = "/";
    });
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    setSearchKeyword("");
    navigate(`/inventory?keyword=${searchKeyword}`, { replace: true });
  };

  const handleOnConnected = (instance) => {
    instance.subscribe(SystemHubEvents.SubscriptionLevellChange);
  };

  const handleReceiveEvent = (instance, systemHubEvent, response) => {
    switch(systemHubEvent) {
      case SystemHubEvents.SubscriptionLevellChange: {
        console.log('@@@ SUBSCRIPTION LEVEL CHANGE @@@', response);
        const userAccount = getUserAccount();
        userAccount.subscriptionLevel = response.subscriptionLevel; // validated on backend requests, so we can trust this value
        setUserAccount(userAccount);
        // update the current user state to trigger a re-render and update the subscription tag in the UI
        setCurrentUser(userAccount);
        break;
      }
    }
  };

  const isLoggedIn = isAuthenticated();

  return (
    <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
          <Container className={"header-container"}>
            <NavbarBrand tag={Link} to="/" className="svg"><div className="logo" /><span className={getSubscriptionTag(currentUser)}>Binner</span></NavbarBrand>
            <NavbarToggler onClick={toggleNavbar} className="mr-2" />
            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed} navbar>
              <Form onSubmit={onSubmit}>
                {isLoggedIn && 
                <SystemHubProvider onReceiveEvent={handleReceiveEvent} onConnected={handleOnConnected}>
                  <ul className="navbar-nav flex-grow">
                    <NavItem style={{lineHeight: '2.3'}}>
                      <Popup 
                        position="left center"
                        content={t('comp.navBar.help', "Help")}
                        trigger={<Link to="/help" className="help-icon"><Icon name="help circle" /></Link>}
                      />
                    </NavItem>
                    <NavItem>
                      <ProtectedInput
                        size="mini"
                        icon
                        placeholder={t('comp.navBar.search', "Search")}
                        onChange={handleChange}
                        value={searchKeyword}
                        name="searchKeyword"
                        allowEnter
                        hideIcon
                      >
                        <input />
                        <Icon name="search" circular link onClick={onSubmit} />
                      </ProtectedInput>
                    </NavItem>
                    <NavItem>
                      <NavLink tag={Link} className="text-dark" to="/">
                      {t('comp.navBar.home', "Home")}
                      </NavLink>
                    </NavItem>
                    <NavItem>
                      <NavLink tag={Link} className="text-dark" to="/inventory/add">
                      {t('comp.navBar.addInventory', "Add Inventory")}
                      </NavLink>
                    </NavItem>
                    <NavItem>
                      <NavLink tag={Link} className="text-dark" to="/import">
                      {t('comp.navBar.orderImport', "Order Import")}
                      </NavLink>
                    </NavItem>
                    <NavItem>
                        <Menu stackable style={{ minHeight: "2.4em", marginTop: "5px" }}>
                          <Dropdown direction="left" item trigger={<Icon name="user" />}>
                            <Dropdown.Menu>
                              <Dropdown.Item icon="edit" text="Account Settings" as={Link} to="/account" />
                              {isAdmin() && 
                                <Dropdown.Item icon="users" text="Manage Users" as={Link} to="/admin/users" />
                              }
                              <Dropdown.Item icon="help circle" text="Help" as={Link} to="/help" />
                              <Dropdown.Item icon="bug" text="Report a Bug" as={Link} to="https://github.com/replaysMike/Binner/issues" target="_blank" />
                              <Dropdown.Item icon="sign out" text="Logout" onClick={logout} />
                            </Dropdown.Menu>
                          </Dropdown>
                        </Menu>
                      </NavItem>
                  </ul>
                </SystemHubProvider>
                }
              </Form>
            </Collapse>
          </Container>
        </Navbar>
    </header>
  );
}
