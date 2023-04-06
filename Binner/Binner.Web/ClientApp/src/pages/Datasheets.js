import React, { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import ReactDOM from 'react-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import AwesomeDebouncePromise from 'awesome-debounce-promise';
import { Table, Form, Segment, Breadcrumb } from 'semantic-ui-react';
import { FormHeader } from "../components/FormHeader";
import { fetchApi } from '../common/fetchApi';

export function Datasheets (props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  Datasheets.abortController = new AbortController();
  const [parts, setParts] = useState([]);
  const [part, setPart] = useState({ partNumber: '', partType: '', mountingType: '' });
  const [column, setColumn] = useState(null);
  const [direction, setDirection] = useState(null);
  const [loading, setLoading] = useState(false);
  const [partTypes, setPartTypes] = useState([]);
  const [mountingTypes] = useState([
    {
      key: 999,
      value: null,
      text: '',
    },
    {
      key: 1,
      value: 'through hole',
      text: 'Through Hole',
    },
    {
      key: 2,
      value: 'surface mount',
      text: 'Surface Mount',
    },
  ]);

  useEffect(() => {
    fetchPartTypes();
  }, []);

  const fetchPartMetadata = async (input) => {
    Datasheets.abortController.abort(); // Cancel the previous request
    Datasheets.abortController = new AbortController();
    setLoading(true);
    try {
      const response = await fetchApi(`part/info?partNumber=${input}&partType=${part.partType}&mountingType=${part.mountingType}`, {
        signal: Datasheets.abortController.signal
      });
      const responseData = response.data;
      if (responseData.requiresAuthentication) {
        // redirect for authentication
        window.open(responseData.redirectUrl, '_blank');
        return;
      }
      const { response: data } = responseData;
      setParts(data.parts);
    } catch (ex) {
      if (ex.name === 'AbortError') {
        return; // Continuation logic has already been skipped, so return normally
      }
      throw ex;
    }
    setLoading(false);
  };

  const searchDebounced = AwesomeDebouncePromise(fetchPartMetadata, 500);

  const fetchPartTypes = async () => {
    setLoading(true);
    const response = await fetchApi('partType/list');
    const { data } = response;
    const blankRow = { key: 999, value: null, text: '' };
    let partTypes = _.sortBy(data.map((item) => {
      return {
        key: item.partTypeId,
        value: item.name,
        text: item.name,
      };
    }), 'text');
    partTypes.unshift(blankRow);
    setPartTypes(partTypes);
    setLoading(false);
  };

  const handleSort = (clickedColumn) => () => {
    if (column !== clickedColumn) {
      setColumn(clickedColumn);
      setParts(_.sortBy(parts, [clickedColumn]));
      setDirection('ascending');
    } else {
      setParts(parts.reverse());
      setDirection(direction === 'ascending' ? 'descending' : 'ascending');
    }
  };

  const handleChange = (e, control) => {
    part[control.name] = control.value;
    switch (control.name) {
      case 'partNumber':
        if (control.value && control.value.length > 0)
          searchDebounced(control.value);
        break;
      case 'partType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          searchDebounced(control.value);
        break;
      case 'mountingType':
        if (control.value && control.value.length > 0 && part.partNumber.length > 0)
          searchDebounced(control.value);
        break;
      default:
        break;
    }
    setPart({...part});
  };

  const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  };

  const handleHighlightAndVisit = (e, url) => {
    handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains('positive')) row.classList.remove('positive');
    }
    targetNode.classList.toggle('positive');
  };

  const getHostnameFromRegex = (url) => {
    // run against regex
    const matches = url.match(/^https?:\/\/([^/?#]+)(?:[/?#]|$)/i);
    // extract hostname (will be null if no match is found)
    return matches && matches[1];
  };

  const renderParts = (parts, column, direction) => {
    const partsWithDatasheets = _.filter(parts, function (x) { return x.datasheetUrls.length > 0 && _.first(x.datasheetUrls).length > 0; });
    return (
      <div>
        <Form>
          <Form.Group>
            <Form.Input label={t('label.part', "Part")} required placeholder='LM358' icon='search' focus value={part.partNumber} onChange={handleChange} name='partNumber' />
            <Form.Dropdown label={t('label.partType', "Part Type")} placeholder='Part Type' search selection value={part.partType} options={partTypes} onChange={handleChange} name='partType' />
            <Form.Dropdown label={t('label.mountingType', "Mounting Type")} placeholder='Mounting Type' search selection value={part.mountingType} options={mountingTypes} onChange={handleChange} name='mountingType' />
          </Form.Group>
        </Form>
        <Segment loading={loading}>
          <Table compact celled sortable selectable striped size='small'>
            <Table.Header>
              <Table.Row>
                <Table.HeaderCell sorted={column === 'manufacturerPartNumber' ? direction : null} onClick={handleSort('manufacturerPartNumber')}>{t('label.part', "Part")}</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'manufacturer' ? direction : null} onClick={handleSort('manufacturer')}>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'website' ? direction : null} onClick={handleSort('website')}>{t('label.website', "Website")}</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'datasheetUrl' ? direction : null} onClick={handleSort('datasheetUrl')}>{t('label.datasheet', "Datasheet")}</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'package' ? direction : null} onClick={handleSort('package')}>{t('label.package', "Package")}</Table.HeaderCell>
                <Table.HeaderCell sorted={column === 'status' ? direction : null} onClick={handleSort('status')}>{t('label.status', "Status")}</Table.HeaderCell>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {partsWithDatasheets.map((p, i) =>
                <Table.Row key={i}>
                  <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
                  <Table.Cell>{p.manufacturer}</Table.Cell>
                  <Table.Cell>{getHostnameFromRegex(_.first(p.datasheetUrls))}</Table.Cell>
                  <Table.Cell>{p.datasheetUrls.map((url, dindex) =>
                    <a href={url} alt={url} key={dindex} onClick={e => handleHighlightAndVisit(e, url)}>{t('button.viewDatasheet', "View Datasheet")}</a>
                  )}
                  </Table.Cell>
                  <Table.Cell>{p.packageType.replace(/\([^()]*\)/g, '')}</Table.Cell>
                  <Table.Cell>{p.status}</Table.Cell>
                </Table.Row>
              )}
            </Table.Body>
          </Table>
        </Segment>
      </div>
    );
  };

  const contents = renderParts(parts, column, direction);

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.datasheet.title', "Datasheet Search")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.datasheet.title', "Datasheet Search")} to={".."}>
			</FormHeader>
      {contents}
    </div>
  );
}
