import React, { useState, useEffect, useMemo } from "react";
import ReactDOM from "react-dom";
import { Link } from "react-router-dom";
import _ from "underscore";
import { Label, Button, Image, Form, Table, Segment, Dimmer, Checkbox, Loader, Popup } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { FormError } from "../components/FormError";
import { toast } from "react-toastify";
import { format, parseJSON } from "date-fns";
import { formatCurrency, isNumeric } from "../common/Utils";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";

export function OrderImport(props) {
  OrderImport.abortController = new AbortController();
  const [orderLabel, setOrderLabel] = useState("Sales Order #");
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState({});
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [order, setOrder] = useState({
    orderId: "",
    supplier: "DigiKey"
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
        toast.error(`Hmm, I don't recognize that 2D barcode!`, { autoClose: 10000 });
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
      supplier: order.supplier
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

  const handleChange = (e, control) => {
    const newOrder = order;
    switch (control.name) {
      case "orderId":
        newOrder.orderId = control.value.replace("\t", "");
        break;
      case "supplier":
        newOrder.supplier = control.value;
        if (newOrder.supplier === "Mouser") {
          setOrderLabel("Web Order #");
          setMessage(
            <div>
              <i>Note:</i> Mouser only supports Web Order # so make sure when importing that you are using the Web Order # and <i>not the Sales Order #</i>
            </div>
          );
        } else {
          setOrderLabel("Sales Order #");
          setMessage("");
        }
        break;
      default:
        break;
    }
    setOrder({ ...newOrder });
  };

  const getMountingTypeById = (mountingTypeId) => {
    switch (mountingTypeId) {
      default:
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
                For <b>Mouser</b> orders, this is the <i>Web Order #</i>.
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
          {(!loading && results && results.parts && renderAllMatchingParts(results)) || <div style={{ lineHeight: "100px" }}>No Results</div>}
        </Segment>
      </div>
    </div>
  );
}
