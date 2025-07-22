import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Table, Form, Segment, Breadcrumb, Pagination, Dropdown, Tab, TabPane, Popup } from "semantic-ui-react";
import { Clipboard } from "../../components/Clipboard";
import { fetchApi } from "../../common/fetchApi";
import { FormHeader } from "../../components/FormHeader";
import { format, parse } from "date-fns";
import "./SystemLogs.css";

export const SystemLogs = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [results, setResults] = useState(100);
  const [by, setBy] = useState('binner');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [logs, setLogs] = useState({ items: [] });
  const [currentTab, setCurrentTab] = useState(0);
  const [delimiter, setDelimiter] = useState('|');
  const itemsPerPageOptions = [
    { key: 1, text: "10", value: 10 },
    { key: 2, text: "25", value: 25 },
    { key: 3, text: "50", value: 50 },
    { key: 4, text: "100", value: 100 },
    { key: 5, text: "200", value: 200 },
    { key: 6, text: "500", value: 500 },
    { key: 7, text: "1000", value: 1000 },
    { key: 8, text: "5000", value: 5000 },
  ];

  const fetchSystemLogs = async (results, page, by) => {
    setLoading(true);
    await fetchApi(`/api/system/logs?orderBy=&direction=0&results=${results}&page=${page}&by=${by}&value=`)
      .then((response) => {
        const { data } = response;
        if (data) {
          setLogs(data);
        }
        setLoading(false);
      });
  }

  useEffect(() => {
    fetchSystemLogs(results, page, by);
  }, []);

  useEffect(() => {
    fetchSystemLogs(results, page, by);
  }, [results]);

  const handlePageChange = async (e, control) => {
    setPage(control.activePage);
    await fetchSystemLogs(results, control.activePage, by);
  };

  const handlePageSizeChange = (e, control) => {
    setResults(control.value);
    setPage(1);
  };

  const parseRowEntry = (log) => {
    const row = log.logEntry.split(delimiter);
    const result = {
      date: '',
      level: '',
      logEntry: ''
    };
    switch(currentTab) {
      case 0: // binner
        try {
          result.date = format(parse(row[0], 'yyyy-MM-dd HH:mm:ss.SSSS', new Date()), 'PP pp');
          result.level = row[1]?.toUpperCase();
          result.logEntry = row.slice(2).join(delimiter);
        } catch (err) { };
        break;
      case 1: // microsoft
        try {
          result.date = format(parse(row[0], 'yyyy-MM-dd HH:mm:ss.SSSS', new Date()), 'PP pp');
          result.level = row[1]?.toUpperCase();
          result.logEntry = row.slice(2).join(delimiter);
        } catch (err) { };
        break;
      case 2: // internal
        try {
          result.date = format(parse(row[0] + ' ' + row[1], 'yyyy-MM-dd HH:mm:ss.SSSS', new Date()), 'PP pp');
          result.level = row[2]?.toUpperCase();
          result.logEntry = row.slice(3).join(delimiter);
        } catch (err) { };
        break;
      case 3: // locale
        try {
          result.date = format(parse(row[0], 'yyyy-MM-dd', new Date()), 'PP');
        } catch (err) { };
        result.logEntry = row.slice(1).join(delimiter);
        break;
    }
    
    return result;
  };

  const renderTable = () => {
    return (<div>
      <Pagination activePage={page} totalPages={100} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" />

      <Table className="log">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell width={3}>{t('label.date', "Date")}</Table.HeaderCell>
            <Table.HeaderCell width={1}>{t('label.level', "Level")}</Table.HeaderCell>
            <Table.HeaderCell></Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {logs.items.map((log, key) => {
            const row = parseRowEntry(log);
            return (
              <Table.Row key={key}>
                <Table.Cell className="date">{row.date}</Table.Cell>
                <Table.Cell className={`level ${row.level}`}>{row.level}</Table.Cell>
                <Table.Cell className="message">
                  <Clipboard text={row.logEntry} />
                  <Popup wide='very' hoverable content={<p>{row.logEntry}</p>} trigger={<span>{row.logEntry}</span>} />
                </Table.Cell>
              </Table.Row>);
          }
          )}
        </Table.Body>
      </Table>
      <Pagination activePage={page} totalPages={100} firstItem={null} lastItem={null} onPageChange={handlePageChange} size="mini" />
      </div>);
  };

  const tabPanes = [
    {
      menuItem: { key: 'binner', icon: 'user', content: t('page.systemLogs.binner', "Binner") },
      render: () =>
        <TabPane style={{ padding: '20px' }}>
          {renderTable()}
        </TabPane>
    },
    {
      menuItem: { key: 'microsoft', icon: 'user', content: t('page.systemLogs.microsoft', "Microsoft") },
      render: () =>
        <TabPane style={{ padding: '20px' }}>
          {renderTable()}
        </TabPane>
    },
    {
      menuItem: { key: 'internal', icon: 'user', content: t('page.systemLogs.internal', "Internal") },
      render: () =>
        <TabPane style={{ padding: '20px' }}>
          {renderTable()}
        </TabPane>
    },
    {
      menuItem: { key: 'locale', icon: 'user', content: t('page.systemLogs.locale', "Locale") },
      render: () =>
        <TabPane style={{ padding: '20px' }}>
          {renderTable()}
        </TabPane>
    },
];

  const handleTabChange = (e, control) => {
    setCurrentTab(control.activeIndex);
    console.log('tab', control.activeIndex);
    let newBy = by;
    let newDelimiter = '|';
    switch (control.activeIndex) {
      case 0:
        newBy = 'binner';
        newDelimiter = '|';
        break;
      case 1:
        newBy = 'microsoft';
        newDelimiter = '|';
        break;
      case 2:
        newBy = 'internal';
        newDelimiter = ' ';
        break;
      case 3:
        newBy = 'missinglocalekeys';
        newDelimiter = '|';
        break;
    }
    setBy(newBy);
    setDelimiter(newDelimiter);
    fetchSystemLogs(results, page, newBy);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t("bc.home", "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/admin")}>{t("bc.admin", "Admin")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t("bc.systemLogs", "System Logs")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t("page.admin.systemLogs.title", "System Logs")} to={".."}>
        {t("page.admin.systemLogs.description", "Logs for your Binner installation.")}
      </FormHeader>

      <Segment loading={loading} raised color="green">
        <Form>
          <div style={{ display: 'flex', alignItems: 'center', float: 'right' }}>
            <Dropdown selection options={itemsPerPageOptions} value={results} className="labeled" onChange={handlePageSizeChange} style={{ width: '75px', minWidth: '75px', marginRight: '10px' }} />
            <div>
              <span>{t("comp.partsGrid.recordsPerPage", "records per page")}</span>
            </div>
          </div>
          
          <Tab panes={tabPanes} activeIndex={currentTab} onTabChange={handleTabChange} />

        </Form>
      </Segment>
    </div>
  );
};
