import React, { useState } from "react";
import ReactDOM from "react-dom";
import { useTranslation, Trans } from "react-i18next";
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
  const { t } = useTranslation();
  OrderImport.abortController = new AbortController();
  const [orderLabel, setOrderLabel] = useState(t('page.orderImport.salesOrderNum', "Sales Order #"));
  const [loading, setLoading] = useState(false);
  const [orderImportSearchResult, setOrderImportSearchResult] = useState(null);
  const [importResult, setImportResult] = useState(null);
  const [error, setError] = useState(null);
  const [apiMessages, setApiMessages] = useState([]);
  const [message, setMessage] = useState(null);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [enableArrowPrepareEmail, setEnableArrowPrepareEmail] = useState(false);
  const [requestProductInfo, setRequestProductInfo] = useState(true);

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
        toast.error(t('error.invalid2dBarcode', "Hmmm, I don't recognize that 2D barcode!"), { autoClose: 10000 });
        return;
      }
    }

    if (isNumeric(input.value)) {
      orderNumber = input.value;
    } else if (input.value.length > 5) {
      // handle invalid input if it's not keyboard typing
      toast.error(t('error.invalidOrder', "Hmmm, that doesn't look like a valid order number!"), { autoClose: 10000 });
      return;
    }

    const newOrder = { ...order, orderId: orderNumber };
    setOrder({ ...newOrder });
    toast.info(t('messasge.loadingOrder', "Loading order# {{order}}", { order: orderNumber }), { autoClose: 10000 });
    getPartsToImport(e, newOrder);
  };

  const enableKeyboardListening = () => {
    setIsKeyboardListening(true);
  };

  const disableKeyboardListening = () => {
    setIsKeyboardListening(false);
  };

  const handleImportParts = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      username: order.username,
      password: order.password,
      parts: _.where(orderImportSearchResult.parts, { selected: true })
    };
    await fetchApi("api/part/importparts", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(request)
    }).then((response) => {
      const { data } = response;
      // show results
      setLoading(false);
      setOrderImportSearchResult(null);
      setImportResult(data);
      toast.success(t('success.partsImported', "{{count}} of {{totalCount}} parts were imported!", { count: _.filter(data.parts, i => i.isImported).length, totalCount: data.parts.length }));
    });
  };

  const getPartsToImport = async (e, order) => {
    e.preventDefault();
    OrderImport.abortController.abort(); // Cancel the previous request
    OrderImport.abortController = new AbortController();
    setLoading(true);
    setError(null);
    setOrderImportSearchResult(null);

    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      username: order.username,
      password: order.password,
      // select if we should fetch all product information for each line item in the order
      requestProductInfo: requestProductInfo
    };

    try {
      await fetchApi("api/part/import", {
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
            setApiMessages(data.response.messages);
            data.response.parts.forEach((i) => {
              i.selected = true;
            });
            setOrderImportSearchResult(data.response);
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
    e.preventDefault();
    setOrder({ orderId: "", supplier: order.supplier });
    setOrderImportSearchResult(null);
    setImportResult(null);
  };

  const handleArrowEmail = (e, order) => {
    e.preventDefault();
    e.stopPropagation();
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
            setOrderLabel(t('page.orderImport.webOrderNum', "Web Order #"));
            setMessage(
              <div>
                <Trans i18nKey="page.orderImport.mouserNote">
                <i>Note:</i> Mouser only supports Web Order # so make sure when importing that you are using the Web Order # and <i>not the Sales Order #</i>
                </Trans>
              </div>
            );
            break;
          case "DigiKey":
            setOrderLabel(t('page.orderImport.salesOrderNum', "Sales Order #"));
            setMessage("");
            break;
          case "Arrow":
            setOrderLabel(t('page.orderImport.orderNum', "Order Number"));
            setEnableArrowPrepareEmail(true);
            setMessage(
              <div>
                <Trans i18nKey="page.orderImport.arrowNote">
                <i>Note:</i> Arrow requires that you first request access to their Order API by sending them an email. See <a href="https://developers.arrow.com/api/index.php/site/page?view=orderApi" target="_blank" rel="noreferrer">Arrow Order Api</a>
                </Trans>
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
    p.selected = !p.selected;
    setOrderImportSearchResult({...orderImportSearchResult});
  };

  const handleToggleRequestProductInfo = () => {
    setRequestProductInfo(!requestProductInfo);
  };

  const formatTrackingNumber = (trackingNumber) => {
    if (trackingNumber && trackingNumber.includes("https:"))
      return (
        <a href={trackingNumber} target="_blank" rel="noreferrer">
          {t('label.viewTracking', "View Tracking")}
        </a>
      );
    return trackingNumber || t('label.unspecified', "Unspecified");
  };

  const renderAllMatchingParts = (order) => {
    return (
      <div>
        <Table compact celled selectable size="small" className="partstable">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell colSpan="11">
                <Table>
                  <Table.Body>
                    <Table.Row>
                      <Table.Cell>
                        <Label>{t('label.customerId', "Customer Id")}:</Label>
                        {order.customerId || "Unspecified"}
                      </Table.Cell>
                      <Table.Cell>
                        <Label>{t('label.orderAmount', "Order Amount")}:</Label>${order.amount.toFixed(2)} {order.currency}
                      </Table.Cell>
                      <Table.Cell>
                        <Label>{t('label.orderDate', "Order Date")}:</Label>
                        {format(parseJSON(order.orderDate), "MMM dd, yyyy", new Date()) || "Unspecified"}
                      </Table.Cell>
                      <Table.Cell>
                        <Label>{t('label.trackingNumber', "Tracking Number")}:</Label>
                        {formatTrackingNumber(order.trackingNumber)}
                      </Table.Cell>
                    </Table.Row>
                  </Table.Body>
                </Table>
              </Table.HeaderCell>
            </Table.Row>
            {order.messages.length > 0  && <Table.Row>
              <Table.Cell colSpan={10}>
                <h5>{t('page.orderImport.apiMessages', "Api Messages")}</h5>
                <ul style={{ marginBottom: '10px' }} className="errors">
                  {order.messages.map((messageItem, key) => (
                    <li key={key} className={`${messageItem.isError ? 'error' : ''}`}>{messageItem.isError ? t('label.error', 'Error') + ': ' : ''}{messageItem.description}</li>
                  ))}
                </ul>
              </Table.Cell>
            </Table.Row>}
            <Table.Row>
              <Table.HeaderCell>{t('label.importQuestion', "Import?")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.part', "Part")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.partNumberShort', "Part#")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.supplierPart', "Supplier Part")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.cost', "Cost")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.quantityShort', "Qty")}</Table.HeaderCell>
              <Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
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
                <Table.Cell>{formatCurrency(p.cost, p.currency || "USD")}</Table.Cell>
                <Table.Cell>{p.quantityAvailable}</Table.Cell>
                <Table.Cell>
                  <Image src={p.imageUrl} size="mini"></Image>
                </Table.Cell>
                <Table.Cell>
                  {p.datasheetUrls.map((d, dindex) => (
                    <Link key={dindex} onClick={(e) => handleHighlightAndVisit(e, d)} to="">
                      {t('button.viewDatasheet', "View Datasheet")}
                    </Link>
                  ))}
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table>
        <Button primary onClick={handleImportParts} disabled={_.filter(orderImportSearchResult.parts, i => i.selected).length === 0}>{t('button.importParts', "Import Parts")}</Button>
      </div>
    );
  };

  const renderImportResult = (importResult) => {
    return (
      <div>        
          <Table compact celled selectable size="small" className="partstable">
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell></Table.HeaderCell>
                <Table.HeaderCell>{t('label.part', "Part")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.partNumberShort', "Part#")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.supplierPart', "Supplier Part")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.cost', "Cost")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.quantityShort', "Qty")}</Table.HeaderCell>
                <Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {importResult.parts.map((p, index) => (
                <React.Fragment key={index}>
                  <Table.Row>
                    <Table.Cell>
                      <Icon name={p.isImported ? "check circle" : "times circle"} color={p.isImported ? "green" : "red"} />
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
                    <Table.Cell>{formatCurrency(p.cost, p.currency || "USD")}</Table.Cell>
                    <Table.Cell>{p.quantityAvailable}</Table.Cell>
                    <Table.Cell><Image src={p.imageUrl} size="mini"></Image></Table.Cell>
                  </Table.Row>
                  <Table.Row style={{backgroundColor: '#fafafa'}}>
                    <Table.Cell></Table.Cell>
                    <Table.Cell colSpan={8} style={{fontSize: '0.8em'}}>
                      {p.errorMessage && <div style={{color: '#c00', fontWeight: '500'}}>{t('label.error', "Error")}: {p.errorMessage}</div>}
                      <div>{p.quantityAdded} {t('page.orderImport.addedToInventory', "added to inventory")}{p.quantityExisting > 0 && <span>, {p.quantityExisting} {t('page.orderImport.alreadyInInventory', "already in inventory")}</span>}</div>
                    </Table.Cell>
                  </Table.Row>
                </React.Fragment>
              ))}
            </Table.Body>
          </Table>
          <Button primary onClick={handleClear}>{t('button.reset', "Reset")}</Button>
      </div>
    );
  };

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={4} />
      <h1>{t('page.orderImport.title', "Order Import")}</h1>
      <Form>
        <Form.Group>
          <Form.Dropdown label="Supplier" selection value={order.supplier} options={supplierOptions} onChange={handleChange} name="supplier" />
          <Popup
            position="bottom left"
            wide
            content={
              <div>
                {t('page.orderImport.enterOrderNumber', "Enter your order number for the supplier.")}
                
                <br />
                <br />
                <Trans i18nKey="page.orderImport.instructions">
                For <b>DigiKey</b> orders, this is the <i>Sales Order #</i>.<br />
                For <b>Mouser</b> orders, this is the <i>Web Order #</i>.<br />
                For <b>Arrow</b> orders, this is the <i>Order Number</i>.
                </Trans>
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

        <div style={{marginTop: '20px', marginBottom: '10px'}}>
          <Popup 
            wide
            position="top left"
            positionFixed
            content={<p>Enable to fetch all product information available for each item in the order.</p>}
            trigger={<Checkbox toggle checked={requestProductInfo} name="requestProductInfo" onChange={handleToggleRequestProductInfo} label={t('page.orderImport.requestAllProductInformation', 'Request all product information')} style={{fontSize: '0.8em'}} />}
          />
          
        </div>
        <Button primary onClick={e => getPartsToImport(e, order)} disabled={loading || order.orderId.length === 0}>
          {t('button.search', "Search")}
        </Button>
        <Button onClick={handleClear} disabled={!(orderImportSearchResult?.parts?.length > 0 || importResult?.parts?.length > 0)}>
        {t('button.clear', "Clear")}
        </Button>

      </Form>
      <div style={{ marginTop: "20px" }}>
        <Segment style={{ minHeight: "100px" }} className="centered">
          {FormError(error)}
          <Dimmer active={loading} inverted>
            <Loader inverted />
          </Dimmer>
          {!loading && importResult && importResult.parts 
            ? renderImportResult(importResult)
            : (!loading && orderImportSearchResult && orderImportSearchResult.parts && renderAllMatchingParts(orderImportSearchResult)) 
                || <div style={{ lineHeight: "100px" }}>
                  {enableArrowPrepareEmail && <div><Popup wide='very' position="top center" hoverable content={<p>Clicking this button will open an email template in your default mail application. You will need to send this email to api@arrow.com</p>} trigger={<Button primary onClick={e => handleArrowEmail(e, order)}><Icon name="mail" /> Create Arrow Email</Button>}/></div>}
                  {t('message.noResults', "No Results")}
                </div>
            }
        </Segment>
      </div>
    </div>
  );
}
