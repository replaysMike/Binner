import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import _ from 'underscore';
import { Label, Segment, Form, Dropdown, Statistic, Breadcrumb, Tab, TabPane } from 'semantic-ui-react';
import { encodeResistance } from '../../common/Utils';

export function SmdResistorCodeCalculator(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState(-1);

  useEffect(() => {
    
  }, []);

  const handleTabChange = (e, data) => {
    setActiveTab(data.activeIndex);
  };

  const tabs = [
    { menuItem: '3 Digit EIA', render: () => <Tab.Pane>3 Digit EIA</Tab.Pane> },
    { menuItem: '4 Digit EIA', render: () => <Tab.Pane>4 Digit EIA</Tab.Pane> },
    { menuItem: 'EIA-96', render: () => <Tab.Pane>EIA-96</Tab.Pane> },
  ]

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.smdResistanceCalc', "SMD Resistor Code Calculator")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.tool.smdResistanceCalc.title', 'SMD Resistor Code Calculator')}</h1>
      <Form>
        <Segment>
          <Tab panes={tabs} onTabChange={handleTabChange}></Tab>
        </Segment>
      </Form>
    </div>
  );
}
