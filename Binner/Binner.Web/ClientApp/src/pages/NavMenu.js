import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link, withRouter } from 'react-router-dom';
import { Form, Input } from 'semantic-ui-react';
import './NavMenu.css';

class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor(props) {
    super(props);
    this.state = {
      collapsed: true,
      searchKeyword: ''
    };
    this.handleChange = this.handleChange.bind(this);
    this.onSubmit = this.onSubmit.bind(this);
    this.toggleNavbar = this.toggleNavbar.bind(this);
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  handleChange(e, control) {
    switch (control.name) {
      case 'searchKeyword':
        this.setState({ searchKeyword: control.value });
        break;
    }
  }

  onSubmit(e) {
    e.preventDefault();
    e.stopPropagation();
    this.setState({ searchKeyword: '' });
    this.props.history.push(`/inventory?keyword=${this.state.searchKeyword}`);
  }

  render() {
    const { searchKeyword } = this.state;
    return (
      <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
          <Container>
            <NavbarBrand tag={Link} to="/">Binner</NavbarBrand>
            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
              <Form onSubmit={this.onSubmit}>
                <ul className="navbar-nav flex-grow">
                  <NavItem>
                    <Input icon={{ name: 'search', circular: true, link: true, onClick: this.onSubmit }} size='mini' placeholder='Search' onChange={this.handleChange} value={searchKeyword} name='searchKeyword' />
                  </NavItem>
                  <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                  </NavItem>
                  <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/inventory/add">Add Inventory</NavLink>
                  </NavItem>
                  <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/import">Order Import</NavLink>
                  </NavItem>
                </ul>
              </Form>
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
  }
}

const routedNavMenu = withRouter(NavMenu);

export { routedNavMenu as NavMenu };
