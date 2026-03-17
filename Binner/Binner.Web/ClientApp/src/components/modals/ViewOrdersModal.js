import { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Button, Modal, Table, Form, Icon, Popup } from "semantic-ui-react";
import PropTypes from "prop-types";
import { fetchApi } from "../../common/fetchApi";
import { BinnerLoader } from "../BinnerLoader";
import { getLocalData, setLocalData } from "../../common/storage";
import { FormatDateOnly } from "../../common/datetime";
import { parse, parseISO, format } from "date-fns";
import DatePicker from "react-datepicker";
import { toast } from "react-toastify";
import "react-datepicker/dist/react-datepicker.css";
import "./DatePicker.css";

/**
 * View my orders for an api supplier
 */
export function ViewOrdersModal({ isOpen = false, onClose, onSelectOrder, ...rest }) {
  ViewOrdersModal.abortController = new AbortController();
  const getViewPreference = (preferenceName, defaultValue) => {
    return getLocalData(preferenceName, { settingsName: 'viewOrdersModal', defaultValue })
  };

  const setViewPreference = (preferenceName, value) => {
    setLocalData(preferenceName, value, { settingsName: 'viewOrdersModal' });
  };
  const defaultForm = {
    startDate: parseISO(getViewPreference('startDate', format(new Date() - (365 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd'))), // Default to 1 year ago
    endDate: parseISO(getViewPreference('endDate', format(new Date(), 'yyyy-MM-dd'))),
    pageNumber: 1,
    pageSize: 50
  };

  const { t } = useTranslation();
  const [_isOpen, setIsOpen] = useState(false);
  const [form, setForm] = useState(defaultForm);
  const [orders, setOrders] = useState([]);
  const [ordersRequested, setOrdersRequested] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingText, setIsLoadingText] = useState(t('message.loading', "Loading..."));

  const viewOrders = async () => {
    ViewOrdersModal.viewOrders?.abort(); // Cancel the previous request
    ViewOrdersModal.viewOrders = new AbortController();
    setOrdersRequested(true);
    const request = {
      supplier: rest.supplier,
      startDate: typeof form.startDate === 'string' ? parse(form.startDate, 'yyyy-MM-dd', new Date()) : form.startDate,
      endDate: typeof form.endDate === 'string' ? parse(form.endDate, 'yyyy-MM-dd', new Date()) : form.endDate,
      pageNumber: 1,
      pageSize: 50,
    };
    try {
      setIsLoading(true);
      await fetchApi("/api/part/orders", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request),
        signal: ViewOrdersModal.viewOrders.signal
      }).then((response) => {
        if (response.responseObject.ok) {
          const { data } = response;

          setIsLoading(false);
          if (data.errors && data.errors.length > 0) {
            toast.error(data.errors[0] || 'Error fetching orders');
          } else {
            setOrders(data.response.orders || []);
          }
        } else {
          toast.error('Error fetching orders');
        }
      });
    } catch (ex) {
      setIsLoading(false);
      if (ex.name === "AbortError") {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
  };

  useEffect(() => {
    setIsOpen(isOpen);
  }, [isOpen]);

  useEffect(() => {
    setOrders([]);
    setOrdersRequested(false);
  }, [rest.supplier]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (onClose) onClose();
  };

  const handleChange = (e, control) => {
    form[control.name] = control.value;
    switch (control.name) {
      case 'startDate':
        setViewPreference('startDate', control.value);
        break;
      case 'endDate':
        // don't remember end date, always use current date by default
        break;
    }
    setForm({ ...form });
  };

  const getImportedIcon = (isImported) => {
    if (isImported) {
      return <Icon name="check circle" color="green" />;
    }
    return <Icon name="times circle" color="grey" />;
  }

  return (<Modal centered open={_isOpen} onClose={handleModalClose} className="viewOrdersModal">
    <Modal.Header>{t('comp.viewOrdersModal.header', "My Orders")} - {rest.supplier}</Modal.Header>
    <Modal.Content scrolling style={{ minHeight: '350px' }}>
      <div className="centered">
        <Form>
          <Form.Group style={{ display: 'flex', flexDirection: 'row', justifyContent: 'center' }}>
            <Popup
              content={<p>Choose the oldest date you want to start listing orders for (<i>Default: 1 year</i>)</p>}
              trigger={<Form.Field>
                <label>{t('label.startDate', "Start Date")}</label>
                <DatePicker showIcon selected={form.startDate} onChange={(date) => handleChange({}, { name: 'startDate', value: date })} />
              </Form.Field>}
            />
            <Popup
              content={<p>Choose the most recent date you want to list orders up until (<i>Default: Today</i>)</p>}
              trigger={<Form.Field>
                <label>{t('label.endDate', "End Date")}</label>
                <DatePicker showIcon selected={form.endDate} onChange={(date) => handleChange({}, { name: 'endDate', value: date })} />
              </Form.Field>}
            />

          </Form.Group>

          <Button primary type="button" onClick={viewOrders} disabled={isLoading}>View Orders</Button>
        </Form>

        <BinnerLoader loading={isLoading} text={isLoadingText}>
          {ordersRequested && <Table compact celled size="small" style={{ margin: '10px auto', width: '60%', minHeight: '120px' }}>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell width={2}></Table.HeaderCell>
                <Table.HeaderCell width={1} textAlign="center">{t('page.orderImport.imported', "Imported?")}</Table.HeaderCell>
                <Table.HeaderCell width={4}>{t('page.orderImport.orderNum', "Order Number")}</Table.HeaderCell>
                <Table.HeaderCell width={4}>{t('label.orderDate', "Order Date")}</Table.HeaderCell>
                <Table.HeaderCell width={3}>{t('page.orderImport.orderStatus', "Order Status")}</Table.HeaderCell>
                <Table.HeaderCell width={2}>{t('page.orderImport.orderItems', "Order Items")}</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {orders.length > 0
                ? orders.map((order, key) => (
                  <Table.Row key={key}>
                    <Table.Cell textAlign="center"><Button primary type="button" onClick={(e) => onSelectOrder(e, order.orderId)} size='mini'>Select</Button></Table.Cell>
                    <Table.Cell textAlign="center" style={{ verticalAlign: 'middle' }}>{getImportedIcon(order.isImported)}</Table.Cell>
                    <Table.Cell style={{ verticalAlign: 'middle' }}><a href="#" onClick={(e) => onSelectOrder(e, order.orderId)}>{order.orderId}</a></Table.Cell>
                    <Table.Cell style={{ verticalAlign: 'middle' }}>{format(order.orderDate, FormatDateOnly)}</Table.Cell>
                    <Table.Cell style={{ verticalAlign: 'middle' }}>{order.orderStatus}</Table.Cell>
                    <Table.Cell style={{ verticalAlign: 'middle' }}>{order.orderItemsTotal}</Table.Cell>
                  </Table.Row>
                ))
                : <Table.Row>
                  <Table.Cell colSpan="6" textAlign="center" style={{ verticalAlign: 'middle' }}>{t('message.noResults', "No results.")}</Table.Cell>
                </Table.Row>
              }
            </Table.Body>
          </Table>}
        </BinnerLoader>
      </div>
    </Modal.Content>
    <Modal.Actions>
      <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
    </Modal.Actions>
  </Modal>);
};

ViewOrdersModal.propTypes = {
  /** Supplier to view orders for */
  supplier: PropTypes.string,
  /** Event handler when selecting an order */
  onSelectOrder: PropTypes.func,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
