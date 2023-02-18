import React, { useState } from "react";
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from "reactstrap";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { Form, Input, Icon, Popup } from "semantic-ui-react";
import "./NavMenu.css";

export function NavMenu(props) {
  const [collapsed, setCollapsed] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState("");
  const navigate = useNavigate();
  const location = useLocation();

  const toggleNavbar = () => {
    setCollapsed(!collapsed);
  };

  const handleChange = (e, control) => {
    switch (control.name) {
      case "searchKeyword":
        setSearchKeyword(control.value);
        break;
      default:
        break;
    }
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    setSearchKeyword("");
    navigate(`/inventory?keyword=${searchKeyword}`, { replace: true });
  };

  return (
    <header>
      <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
        <Container className={"binner-container"}>
          <NavbarBrand tag={Link} to="/" />
          <NavbarToggler onClick={toggleNavbar} className="mr-2" />
          <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed} navbar>
            <Form onSubmit={onSubmit}>
              <ul className="navbar-nav flex-grow">
                <NavItem style={{lineHeight: '2.3'}}>
                  <Popup 
                    position="left center"
                    content="Help"
                    trigger={<Link to="/help" className="help-icon"><Icon name="help circle" /></Link>}
                  />
                  
                </NavItem>
                <NavItem>
                  <Input
                    icon={{ name: "search", circular: true, link: true, onClick: onSubmit }}
                    size="mini"
                    placeholder="Search"
                    onChange={handleChange}
                    value={searchKeyword}
                    name="searchKeyword"
                  />
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/">
                    Home
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/inventory/add">
                    Add Inventory
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/import">
                    Order Import
                  </NavLink>
                </NavItem>
              </ul>
            </Form>
          </Collapse>
        </Container>
      </Navbar>
    </header>
  );
}
