import React, { useState, useEffect, useMemo } from "react";
import ReactDOM from "react-dom";
import { Link } from "react-router-dom";
import _ from "underscore";
import { Label, Button, Image, Form, Table, Segment, Dimmer, Checkbox, Loader, Popup, Icon } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { FormError } from "../components/FormError";
import { toast } from "react-toastify";
import { format, parseJSON } from "date-fns";
import { formatCurrency, isNumeric } from "../common/Utils";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import sha256 from 'crypto-js/sha256';

export function OrderImport(props) {
  OrderImport.abortController = new AbortController();
  const [orderLabel, setOrderLabel] = useState("Sales Order #");
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState({});
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [enableArrowPrepareEmail, setEnableArrowPrepareEmail] = useState(false);
  const [order, setOrder] = useState({
    orderId: "",
    supplier: "DigiKey",
    username: null,
    password: null
  });
  const [supplierOptions] = useState([
    {
      key: 1,
      value: "DigiKey",
      text: "DigiKey"
    },
    {
      key: 2,
      value: "Mouser",
      text: "Mouser"
    },
    {
      key: 3,
      value: "Arrow",
      text: "Arrow"
    }
  ]);

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    // ignore single keypresses
    let orderNumber = '';
    if (input.type === "datamatrix") {
      if (input.value.salesOrder) {
        orderNumber = input.value.salesOrder;
      } else {
        toast.error(`Hmmm, I don't recognize that 2D barcode!`, { autoClose: 10000 });
        return;
      }
    }

    if (isNumeric(input.value)) {
      orderNumber = input.value;
    } else if (input.value.length > 5) {
      // handle invalid input if it's not keyboard typing
      toast.error(`Hmmm, that doesn't look like a valid order number!`, { autoClose: 10000 });
      return;
    }

    const newOrder = { ...order, orderId: orderNumber };
    setOrder({ ...newOrder });
    toast.info(`Loading order# ${orderNumber}`, { autoClose: 10000 });
    getPartsToImport(e, newOrder);
  };

  const enableKeyboardListening = () => {
    setIsKeyboardListening(true);
  };

  const disableKeyboardListening = () => {
    setIsKeyboardListening(false);
  };

  const handleImportParts = async (e) => {
    setLoading(true);
    setError(null);

    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      username: order.username,
      password: order.password,
      parts: _.where(results.parts, { selected: true })
    };
    await fetchApi("part/importparts", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    }).then((response) => {
      const { data } = response;
      // reset form
      setLoading(false);
      setResults({});
      toast.success(`${data.length} parts were imported!`);
    });
  };

  const getPartsToImport = async (e, order) => {
    OrderImport.abortController.abort(); // Cancel the previous request
    OrderImport.abortController = new AbortController();
    setLoading(true);
    setError(null);
    setResults({});

    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      username: order.username,
      password: order.password
    };

    try {
      await fetchApi("part/import", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request),
        signal: OrderImport.abortController.signal
      }).then((response) => {
        const { data } = response;
        toast.dismiss();
        if (data.requiresAuthentication) {
          // redirect for authentication
          const errorMessage = data.errors.join("\n");
          setError(errorMessage);
          setLoading(false);
          window.open(data.redirectUrl, "_blank");
          return;
        }

        if (response.responseObject.status === 200) {
          if (data.errors && data.errors.length > 0) {
            // display error
            const errorMessage = data.errors.join("\n");
            setError(errorMessage);
            setLoading(false);
            toast.error(errorMessage);
          } else {
            data.response.parts.forEach((i) => {
              i.selected = true;
            });
            setResults(data.response);
            setLoading(false);
          }
        } else {
          // display error
          const errorMessage = "Internal server error ocurred!";
          setError(errorMessage);
          setLoading(false);
          toast.error(errorMessage);
        }
      });
    } catch (ex) {
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  const handleClear = (e) => {
    setOrder({ orderId: "", supplier: order.supplier });
    setResults({});
  };

  const handleArrowEmail = (e, order) => {
    e.preventDefault();
    e.stopPropagation();
    console.log('order', order);
    if (!order.username || !order.password || order.username.length === 0 || order.password.length === 0){
      setError("Please fill out the username/password fields first. We will encode them correctly for the email.");
      return;
    }
    const passwordHashed = sha256(order.password).toString().toUpperCase();
    const body = `Hi there!\r\n\r\nI'd like to request access to the Orders API for my account.\r\n\r\nLogin email: ${order.username}\r\nYour SHA-256 encoded password: ${passwordHashed}\r\nYour preferred Shipping Method for both domestic and international from the list of available options: FEDEX PRIORITY\r\n\r\nThanks!\r\n${order.username}\r\nGenerated by Binner - https://github.com/replaysMike/Binner`;
    window.open(`mailto:api@arrow.com?SUBJECT=${encodeURIComponent("Orders API")}&BODY=${encodeURIComponent(body)}`, "_blank");
  };

  const handleChange = (e, control) => {
    const newOrder = order;
    switch (control.name) {
      case "orderId":
        newOrder.orderId = control.value.replace("\t", "");
        break;
      case "supplier":
        newOrder.supplier = control.value;
        switch(newOrder.supplier) {
          case "Mouser":
            setOrderLabel("Web Order #");
            setMessage(
              <div>
                <i>Note:</i> Mouser only supports Web Order # so make sure when importing that you are using the Web Order # and <i>not the Sales Order #</i>
              </div>
            );
            break;
          case "DigiKey":
            setOrderLabel("Sales Order #");
            setMessage("");
            break;
          case "Arrow":
            setOrderLabel("Order Number");
            setEnableArrowPrepareEmail(true);
            setMessage(
              <div>
                <i>Note:</i> Arrow requires that you first request access to their Order API by sending them an email. See <a href="https://developers.arrow.com/api/index.php/site/page?view=orderApi" target="_blank" rel="noreferer">Arrow Order Api</a>
              </div>
            );
            break;
          default:
          break;
        }
        break;
      default:
        newOrder[control.name] = control.value;
        break;
    }
    setOrder({ ...newOrder });
  };

  const getMountingTypeById = (mountingTypeId) => {
    switch (mountingTypeId) {
      default:
      case 0:
        return "none";
      case 1:
        return "through hole";
      case 2:
        return "surface mount";
    }
  };

  const handleHighlightAndVisit = (e, url) => {
    handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains("positive")) row.classList.remove("positive");
    }
    targetNode.classList.toggle("positive");
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

  const handleChecked = (e, p) => {
    //const newResults = [...results];
    //const foundPart = _.find(newResults.parts, { supplierPartNumber: p.supplierPartNumber });
    //newResults.selected = !foundPart.selected;
    //console.log('handleChecked', newResults, foundPart);
    //setResults(newResults);
    //const otherResults = _.filter(results.parts, x => x.supplierPartNumber !== p.supplierPartNumber);
    p.selected = !p.selected;
    //const newResults = {...results, parts: [...otherResults, p]};
    setResults({...results});
    //console.log('newResults', newResults);
  };

  const formatTrackingNumber = (trackingNumber) => {
    if (trackingNumber && trackingNumber.includes("https:"))
      return (
        <a href={trackingNumber} target="_blank" rel="noreferrer">
          View Tracking
        </a>
      );
    return trackingNumber || "Unspecified";
  };

  const renderAllMatchingParts = (order) => {
    return (
      <div>
        <Form onSubmit={handleImportParts}>
          <Table compact celled selectable size="small" className="partstable">
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell colSpan="11">
                  <Table>
                    <Table.Body>
                      <Table.Row>
                        <Table.Cell>
                          <Label>Customer Id:</Label>
                          {order.customerId || "Unspecified"}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Order Amount:</Label>${order.amount.toFixed(2)} {order.currency}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Order Date:</Label>
                          {format(parseJSON(order.orderDate), "MMM dd, yyyy", new Date()) || "Unspecified"}
                        </Table.Cell>
                        <Table.Cell>
                          <Label>Tracking Number:</Label>
                          {formatTrackingNumber(order.trackingNumber)}
                        </Table.Cell>
                      </Table.Row>
                    </Table.Body>
                  </Table>
                </Table.HeaderCell>
              </Table.Row>
              <Table.Row>
                <Table.HeaderCell>Import?</Table.HeaderCell>
                <Table.HeaderCell>Part</Table.HeaderCell>
                <Table.HeaderCell>Part#</Table.HeaderCell>
                <Table.HeaderCell>Manufacturer</Table.HeaderCell>
                <Table.HeaderCell>Part Type</Table.HeaderCell>
                <Table.HeaderCell>Supplier Part</Table.HeaderCell>
                <Table.HeaderCell>Cost</Table.HeaderCell>
                <Table.HeaderCell>Qty</Table.HeaderCell>
                <Table.HeaderCell>Image</Table.HeaderCell>
                <Table.HeaderCell></Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {order.parts.map((p, index) => (
                <Table.Row key={index}>
                  <Table.Cell>
                    <Checkbox toggle checked={p.selected} onChange={(e) => handleChecked(e, p)} data={p} />
                  </Table.Cell>
                  <Table.Cell style={{ maxWidth: "200px", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }} title={p.description}>
                    {p.description}
                  </Table.Cell>
                  <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                  <Table.Cell style={{ maxWidth: "200px", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }} title={p.manufacturer}>
                    {p.manufacturer}
                  </Table.Cell>
                  <Table.Cell>{p.partType}</Table.Cell>
                  <Table.Cell>{p.supplierPartNumber}</Table.Cell>
                  <Table.Cell>{formatCurrency(p.cost)}</Table.Cell>
                  <Table.Cell>{p.quantityAvailable}</Table.Cell>
                  <Table.Cell>
                    <Image src={p.imageUrl} size="mini"></Image>
                  </Table.Cell>
                  <Table.Cell>
                    {p.datasheetUrls.map((d, dindex) => (
                      <Link key={dindex} onClick={(e) => handleHighlightAndVisit(e, d)} to="">
                        View Datasheet
                      </Link>
                    ))}
                  </Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table>
          <Button primary disabled={_.filter(results.parts, i => i.selected).length === 0}>Import Parts</Button>
        </Form>
      </div>
    );
  };

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={4} />
      <h1>Order Import</h1>
      <Form onSubmit={(e) => getPartsToImport(e, order)}>
        <Form.Group>
          <Form.Dropdown label="Supplier" selection value={order.supplier} options={supplierOptions} onChange={handleChange} name="supplier" />
          <Popup
            position="bottom left"
            wide
            content={
              <div>
                Enter your order number for the supplier.
                <br />
                <br />
                For <b>DigiKey</b> orders, this is the <i>Sales Order #</i>.<br />
                For <b>Mouser</b> orders, this is the <i>Web Order #</i>.<br />
                For <b>Arrow</b> orders, this is the <i>Order Number</i>.
              </div>
            }
            trigger={
              <Form.Input
                label={orderLabel}
                placeholder="1023840"
                icon="search"
                focus
                value={order.orderId}
                onChange={handleChange}
                onFocus={disableKeyboardListening}
                onBlur={enableKeyboardListening}
                name="orderId"
              />
            }
          />
          {order.supplier === "Arrow" &&
          <>
            <Popup 
              content={<p>Enter your {order.supplier} account username</p>}
              trigger={<Form.Input
                label="Username/login"
                placeholder="johndoe@example.com"
                icon="user"
                value={order.username || ''}
                onChange={handleChange}
                onFocus={disableKeyboardListening}
                onBlur={enableKeyboardListening}
                name="username"
              />}
            />
            <Popup 
              content={<p>Enter your {order.supplier} account password</p>}
              trigger={<Form.Input
                type="password"
                label="Password"
                icon="key"
                value={order.password || ''}
                onChange={handleChange}
                onFocus={disableKeyboardListening}
                onBlur={enableKeyboardListening}
                name="password"
              />}
            />
          </>}
        </Form.Group>
        <div style={{ height: "30px" }}>{message}</div>
        <Button primary disabled={loading || order.orderId.length === 0}>
          Search
        </Button>
        <Button onClick={handleClear} disabled={!(results.parts && results.parts.length > 0)}>
          Clear
        </Button>
      </Form>
      <div style={{ marginTop: "20px" }}>
        <Segment style={{ minHeight: "100px" }} className="centered">
          {FormError(error)}
          <Dimmer active={loading} inverted>
            <Loader inverted />
          </Dimmer>
          {(!loading && results && results.parts && renderAllMatchingParts(results)) 
          || <div style={{ lineHeight: "100px" }}>
              {enableArrowPrepareEmail && <div><Popup wide='very' position="top center" hoverable content={<p>Clicking this button will open an email template in your default mail application. You will need to send this email to api@arrow.com</p>} trigger={<Button primary onClick={e => handleArrowEmail(e, order)}><Icon name="mail" /> Create Arrow Email</Button>}/></div>}
              No Results
            </div>}
        </Segment>
      </div>
    </div>
  );
}
