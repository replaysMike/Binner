import React, { useState } from "react";
import ReactDOM from "react-dom";
import { useTranslation, Trans } from "react-i18next";
import { Link } from "react-router-dom";
import _ from "underscore";
import { Label, Button, Image, Form, Table, Segment, Dimmer, Checkbox, Loader, Popup, Icon, Confirm } from "semantic-ui-react";
import ProtectedInput from "../components/ProtectedInput";
import { fetchApi } from "../common/fetchApi";
import { FormError } from "../components/FormError";
import { toast } from "react-toastify";
import { format, parseJSON } from "date-fns";
import { formatCurrency, isNumeric } from "../common/Utils";
import { BarcodeScannerInput } from "../components/BarcodeScannerInput";
import sha256 from 'crypto-js/sha256';
// overrides BarcodeScannerInput audio support
const enableSound = true;
const soundSuccess = new Audio('/audio/scan-success.mp3');
const soundFailure = new Audio('/audio/scan-failure.mp3');

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
  const [enableArrowPrepareEmail, setEnableArrowPrepareEmail] = useState(false);
  const [requestProductInfo, setRequestProductInfo] = useState(false);
  const [confirmAuthIsOpen, setConfirmAuthIsOpen] = useState(false);
  const [authorizationApiName, setAuthorizationApiName] = useState('');
  const [authorizationUrl, setAuthorizationUrl] = useState(null);
  const [confirmReImport, setConfirmReImport] = useState(false);
  const [confirmReImportAction, setConfirmReImportAction] = useState(null);

  const [order, setOrder] = useState({
    orderId: "",
    supplier: "DigiKey",
    username: null,
    password: null,
    invoice: null,
    packlist: null,
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
    let invoice = '';
    let packlist = '';
    if (input.type === "datamatrix") {
      if (input.value.salesOrder) {
        orderNumber = input.value.salesOrder;
      } else {
        toast.error(t('error.invalid2dBarcode', "Hmmm, I don't recognize that 2D barcode!"), { autoClose: 10000 });
        return;
      }
      if (input.value.invoice)
        invoice = input.value.invoice;
      if (input.value.packlist)
        packlist = input.value.packlist;
    }

    if (isNumeric(input.value)) {
      orderNumber = input.value;
    } else if (input.value.length > 5) {
      // handle invalid input if it's not keyboard typing
      toast.error(t('error.invalidOrder', "Hmmm, that doesn't look like a valid order number!"), { autoClose: 10000 });
      return;
    }

    const newOrder = { ...order, orderId: orderNumber, invoice: invoice, packlist: packlist };
    setOrder({ ...newOrder });
    getPartsToImport(e, newOrder);
  };

  const handleImportParts = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      invoice: order.invoice,
      packlist: order.packlist,
      username: order.username,
      password: order.password,
      parts: _.where(orderImportSearchResult.parts, { selected: true })
    };
    await fetchApi("/api/part/importparts", {
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

  const validateExistingOrderImport = async (search) => {
    OrderImport.validateExistingOrderImport?.abort();
    OrderImport.validateExistingOrderImport = new AbortController();

    // check if we have imported this order before
    return await fetchApi(`/api/orderImportHistory?orderNumber=${encodeURIComponent(search.orderId.trim())}&supplier=${encodeURIComponent(search.supplier.trim()) }`, {
      signal: OrderImport.validateExistingOrderImport.signal,
      method: "GET"
    }).then((response) => {
      if (response.responseObject.status === 404) {
        // no history record, proceed
        return true;
      } else if (response.responseObject.ok) {
        // record exists, we have imported this order before
        return response.data;
      }
      // error
      return false;
    }).catch((ex) => {
      if (ex?.name === "AbortError") {
        // Continuation logic has already been skipped, so return normally
      } else {
        // other error
        const { data } = ex;
        if (data.status === 404) {
          // no history record, proceed
          return true;
        } else {
          console.error('http error', ex);
          toast.error(`Server returned ${data.status} error.`);
        }
      }
      return false;
    });
  };

  const getPartsToImport = async (e, order, allowReImport = false) => {
    e.preventDefault();
    OrderImport.getPartsToImport?.abort(); // Cancel the previous request
    OrderImport.getPartsToImport = new AbortController();
    setError(null);
    setOrderImportSearchResult(null);

    const validateResult = await validateExistingOrderImport(order);
    if (validateResult === true) {
      // ================================
      // no order import history, proceed
      // ================================
    } else if (validateResult === false) {
      // error occurred
      toast.error(`An error occurred while validating the order import.`);
      if (enableSound) soundFailure.play();
      return;
    }

    if (!allowReImport) {
      // confirm do you want to import it again?
      setConfirmReImportAction(() => async (confirmEvent) => await getPartsToImport(e, order, true));
      setConfirmReImport(true);
      if (enableSound) soundFailure.play();
      return;
    }

    // barcode scan successful
    if (enableSound) soundSuccess.play();
    toast.info(t('message.loadingOrder', "Loading order# {{order}}", { order: order.orderId }), { autoClose: 10000 });

    setLoading(true);
    const request = {
      orderId: order.orderId,
      supplier: order.supplier,
      username: order.username,
      password: order.password,
      // select if we should fetch all product information for each line item in the order
      requestProductInfo: requestProductInfo
    };

    try {
      await fetchApi("/api/part/import", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request),
        signal: OrderImport.getPartsToImport.signal
      }).then((response) => {
        const { data } = response;
        toast.dismiss();
        if (data.requiresAuthentication) {
          // redirect for authentication
          const errorMessage = data.errors.join("\n");
          setError(errorMessage);
          setLoading(false);

          // redirect for authentication
          setAuthorizationApiName(data.apiName);
          setAuthorizationUrl(data.redirectUrl);
          setConfirmAuthIsOpen(true);
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
    setOrder({ orderId: "", supplier: order.supplier, invoice: null, packlist: null });
    setOrderImportSearchResult(null);
    setImportResult(null);
  };

  const handleArrowEmail = (e, order) => {
    e.preventDefault();
    e.stopPropagation();
    if (!order.username || !order.password || order.username.length === 0 || order.password.length === 0) {
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
        switch (newOrder.supplier) {
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
    setOrderImportSearchResult({ ...orderImportSearchResult });
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

  const handleAuthRedirect = (e) => {
    e.preventDefault();
    window.location.href = authorizationUrl;
  };

  const handleOrderIdClear = (e) => {
    setOrder({...order, orderId: ''});
  };

  const handleCancelReImport = (e) => {
    setConfirmReImportAction(null);
    setConfirmReImport(false); // close confirm
    toast.info(t('message.orderImportCancelled', 'Order import cancelled.'));
  }

  const handleConfirmReImport = async (e) => {
    // re-run command by executing the action set
    setConfirmReImport(false);  // close confirm
    if (confirmReImportAction) await confirmReImportAction(e);
    setConfirmReImportAction(null);
  };

  const handleViewPart = (e, part) => {
    if (part.partId && part.manufacturerPartNumber) {
      if (window?.open) window.open(`/inventory/${part.manufacturerPartNumber}:${part.partId}`, "_blank");
    }
  };

  const renderAllMatchingParts = (order) => {
    return (
      <div>
        <Table compact celled selectable size="small" className="partstable expandable-table">
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
                        <Label>{t('label.orderAmount', "Order Amount")}:</Label>{formatCurrency(order.amount, order.currency)} (<i>{order.currency}</i>)
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
            {order.messages.length > 0 && <Table.Row>
              <Table.Cell colSpan={11}>
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
              <Table.HeaderCell>{t('label.exists', "Exists")}</Table.HeaderCell>
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
              <Table.Row key={index} onClick={e => handleViewPart(e, p)}>
                <Table.Cell>
                  <Checkbox toggle checked={p.selected} onChange={(e) => handleChecked(e, p)} data={p} />
                </Table.Cell>
                <Table.Cell textAlign="center">
                  {p.partId 
                    ? <Popup content={<p>{t('popup.partExists', "Part exists in inventory.")}</p>} trigger={<Icon name="check circle" color="blue" size="large" />} /> 
                    : <Popup content={<p>{t('popup.newPart', "New part does not exist in inventory.")}</p>} trigger={<Icon name="plus circle" color="green" size="large" />} />
                  }
                </Table.Cell>
                <Table.Cell className="expandable-cell width-100">
                  <Popup wide hoverable content={<p>{p.description}</p>} trigger={<span>{p.description}</span>} />
                </Table.Cell>
                <Table.Cell className="expandable-cell width-100">
                  <Popup wide hoverable content={<p>{p.manufacturerPartNumber}{p.reference && <><br />{p.reference}</>}</p>} trigger={<div>
                    {p.manufacturerPartNumber}
                    {p.reference && <><br /><div className="part-reference expandable-cell width-100">{p.reference}</div></>}
                  </div>} />
                </Table.Cell>
                <Table.Cell className="expandable-cell width-100">
                  <Popup wide hoverable content={<p>{p.manufacturer}</p>} trigger={<span>{p.manufacturer}</span>} />
                </Table.Cell>
                <Table.Cell className="expandable-cell width-50" textAlign="center">{p.partType}</Table.Cell>
                <Table.Cell className="expandable-cell width-150"><Popup wide hoverable content={p.supplierPartNumber} trigger={<span>{p.supplierPartNumber}</span>} /></Table.Cell>
                <Table.Cell className="expandable-cell width-50">{formatCurrency(p.cost, p.currency || "USD")}<br/><i>{p.currency || "USD"}</i></Table.Cell>
                <Table.Cell className="expandable-cell width-50">{p.quantityAvailable}</Table.Cell>
                <Table.Cell className="expandable-cell width-50">
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
                <Table.Row onClick={e => handleViewPart(e, p)}>
                  <Table.Cell>
                    <Icon name={p.isImported ? "check circle" : "times circle"} color={p.isImported ? "green" : "red"} />
                  </Table.Cell>
                  <Table.Cell className="expandable-cell width-100">
                    {p.description}
                  </Table.Cell>
                  <Table.Cell className="expandable-cell width-100">
                    {p.manufacturerPartNumber}
                    {p.reference && <><br /><p className="part-reference expandable-cell width-100">{p.reference}</p></>}
                  </Table.Cell>
                  <Table.Cell className="expandable-cell width-100">
                    {p.manufacturer}
                  </Table.Cell>
                  <Table.Cell className="expandable-cell width-50" textAlign="center">{p.partType}</Table.Cell>
                  <Table.Cell className="expandable-cell width-100">{p.supplierPartNumber}</Table.Cell>
                  <Table.Cell className="expandable-cell width-50">{formatCurrency(p.cost, p.currency || "USD")}</Table.Cell>
                  <Table.Cell className="expandable-cell width-50">{p.quantityAvailable}</Table.Cell>
                  <Table.Cell className="expandable-cell width-50"><Image src={p.imageUrl} size="mini"></Image></Table.Cell>
                </Table.Row>
                <Table.Row style={{ backgroundColor: '#fafafa' }}>
                  <Table.Cell></Table.Cell>
                  <Table.Cell colSpan={8} style={{ fontSize: '0.8em' }}>
                    {p.errorMessage && <div style={{ color: '#c00', fontWeight: '500' }}>{t('label.error', "Error")}: {p.errorMessage}</div>}
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
      <Confirm
        className="confirm"
        header={t('page.settings.confirm.mustAuthenticateHeader', "Must Authenticate")}
        open={confirmAuthIsOpen}
        onCancel={() => setConfirmAuthIsOpen(false)}
        onConfirm={handleAuthRedirect}
        content={<p>
          <Trans i18nKey="page.settings.confirm.mustAuthenticate" name={authorizationApiName}>
            External Api (<b>{{ name: authorizationApiName }}</b>) is requesting that you authenticate first. You will be redirected back after authenticating with the external provider.
          </Trans>
        </p>
        }
      />
      <Confirm
              header={<div className="header"><Icon name="undo" color="grey" /> {t('confirm.importOrder', "Import Order")}</div>}
              open={confirmReImport}
              confirmButton={t('button.import', "Import")}
              cancelButton={t('button.cancel', "Cancel")}
              content={
                <p style={{ padding: "20px", fontSize: '1.2em', textAlign: "center" }}>
                  <span style={{ color: '#666' }}>{t('confirm.alreadyImportedOrder', "You have already imported this order.")}</span>
                  <br />
                  <br />
                  {t('confirm.confirmReImportOrder', "Do you want to import this order again?")}
                </p>
              }
              onCancel={handleCancelReImport}
              onConfirm={handleConfirmReImport}
            />
      <BarcodeScannerInput onReceived={handleBarcodeInput} swallowKeyEvent={false} minInputLength={4} enableSound={false} />
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
              <ProtectedInput
                label={orderLabel}
                placeholder="1023840"
                icon="search"
                focus
                value={order.orderId}
                onChange={handleChange}
                onClear={handleOrderIdClear}
                name="orderId"
              />
            }
          />
          {order.supplier === "Arrow" &&
            <>
              <Popup
                content={<p>Enter your {order.supplier} account username</p>}
                trigger={<ProtectedInput
                  label="Username/login"
                  placeholder="johndoe@example.com"
                  icon="user"
                  value={order.username || ''}
                  onChange={handleChange}
                  name="username"
                />}
              />
              <Popup
                content={<p>Enter your {order.supplier} account password</p>}
                trigger={<ProtectedInput
                  type="password"
                  label="Password"
                  icon="key"
                  value={order.password || ''}
                  onChange={handleChange}
                  name="password"
                />}
              />
            </>}
        </Form.Group>
        <div style={{ height: "30px" }}>{message}</div>

        <div style={{ marginTop: '20px', marginBottom: '10px' }}>
          <Popup
            wide
            position="top left"
            positionFixed
            content={<p>Enable to fetch all product information available for each item in the order. This will be much slower as each part requires an api request.</p>}
            trigger={<Checkbox toggle checked={requestProductInfo} name="requestProductInfo" onChange={handleToggleRequestProductInfo} label={t('page.orderImport.requestAllProductInformation', 'Request all product information (slower)')} style={{ fontSize: '0.8em' }} />}
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
