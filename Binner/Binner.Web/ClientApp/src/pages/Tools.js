import React from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Segment, Statistic, Icon } from "semantic-ui-react";

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
      <h1>{t('page.tools.title', "Tools")}</h1>
      <p>{t('page.tools.description', "Binner includes a suite of free utilities common to daily life in electrical engineering.")}</p>
      <Segment style={{ padding: "40px 40px" }}>
        <Statistic.Group widths="three">
          <Statistic onClick={(e) => route(e, "/tools/resistor")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="wrench" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.resistorColorCodeCalc', "Resistor Color Code Calculator")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/tools/ohmslaw")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="wrench" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.ohmsLawCalc', "Ohms Law Calculator")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/tools/voltagedivider")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="wrench" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.voltageDividerCalc', "Voltage Divider Calculator")}</Statistic.Label>
          </Statistic>
          <Statistic onClick={(e) => route(e, "/tools/barcodescanner")} style={{ cursor: "pointer" }}>
            <Statistic.Value>
              <Icon name="barcode" />
            </Statistic.Value>
            <Statistic.Label>{t('page.tools.barcodeScanner', "Barcode Scanner")}</Statistic.Label>
          </Statistic>
        </Statistic.Group>
      </Segment>
    </div>
  );
}
