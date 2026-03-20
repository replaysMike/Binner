import React from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormHeader } from "../components/FormHeader";
import { Segment, Statistic, Icon, Breadcrumb } from "semantic-ui-react";
import { SmdResistorIcon, SmdCapacitorIcon, ThroughHoleResistorIcon } from "../common/icons";

export function Tools(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const route = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    navigate(url);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.tools', "Tools")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.tools.title', "Tools")} to="/">
        {t('page.tools.description', "Binner includes a suite of free utilities common to daily life in electrical engineering.")}
      </FormHeader>
      <Segment style={{ padding: "40px 40px" }} className="tools">
        <Statistic.Group widths="four">
          <Statistic onClick={(e) => route(e, "/tools/resistor")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <ThroughHoleResistorIcon style={{ width: '96px', height: 'auto' }} />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.resistorColorCodes', "Resistor Color Codes")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/smdresistor")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <SmdResistorIcon style={{width: '96px', height: 'auto' }} />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.smdResistorCodes', "SMD Resistor Codes")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/smdcapacitor")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <SmdCapacitorIcon style={{ width: '96px', height: 'auto' }} />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.smdCapacitorCodes', "SMD Capacitor Codes")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/ohmslaw")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="calculator" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.ohmsLawCalc', "Ohms Law Calculator")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/voltagedivider")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="calculator" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.voltageDividerCalc', "Voltage Divider Calculator")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/barcodescanner")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="barcode" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.barcodeScanner', "Barcode Scanner")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/keyboarddump")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="keyboard" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.keyboardDump', "Keyboard Dump")}</Statistic.Label>
          </Statistic>

          <Statistic onClick={(e) => route(e, "/tools/referenceDesignators")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="at" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.referenceDesignators', "Reference Designators")}</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>
    </div>
  );
}
