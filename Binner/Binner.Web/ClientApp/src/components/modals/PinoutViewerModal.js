import { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Icon, Button, Modal, Popup, Grid, Image, Placeholder } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";
import { getUrlForResource } from "../../common/resources";
import "./PinoutViewerModal.css";

/**
 * View a pinout
 */
export function PinoutViewerModal({ isOpen = false, pinout, datasheet, onClose, ...rest }) {
  const { t } = useTranslation();
  const [_isOpen, setIsOpen] = useState(false);
  const [_pinout, setPinout] = useState(pinout);

  useEffect(() => {
    setIsOpen(isOpen);
  }, [isOpen]);

  useEffect(() => {
    if (pinout && pinout.pinoutDefinition) {
      // deserialize the pinout definition which is stored as json
      const pinDefinition = JSON.parse(pinout.pinoutDefinition);
      const newPinout = { ...pinout, ...pinDefinition };
      setPinout(newPinout);
    } else {
      setPinout(pinout);
    }
  }, [pinout]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (onClose) onClose();
  };

  const handlePrint = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    if (!_pinout) return;

    let pinsList = '';
    for (let i = 0; i < _pinout.pins.length; i++) {
      const pin = _pinout.pins[i];
      pinsList += `<li class="pin">
        <b>${(i + 1)}</b>: ${pin.label}<br/>
        <span style="font-size: 0.7em">${pin.description}</span>
        ${renderPinFeaturesString(pin)}
        </li>`;
    }
    const style = `<style>
  @media print {
    html, body {
      margin: 0;
      padding: 0;
      box-sizing: border-box;
      break-inside: avoid;
      page-break-inside: avoid;
      height: 98vh;
    }
    .print-container {
      height: 95vh; 
      margin: 0; 
      padding: 0;
      box-sizing: border-box;
      display: flex;
      flex-direction: column;
    }
    .image-container {
      text-align: center;
      padding: 0;
      margin: 0;
      margin-top: 5px;
      flex: 1;
    }
    .image-container img {
      width: auto; 
      height: 100%
    }
    .pinout {
      min-height: 30%;
    }
    .footer {
      font-size: 1.75em;
      text-align: center;
      position: static;
      bottom: 0px;
      width: 100%;
      box-sizing: border-box;
      page-break-after: auto;
    }
    .footer .logo {
      display: inline-block;
      width: 24px;
      height: 24px;
      background-size: cover;
      background-position: center center;
      background-image: url('data:image/svg+xml,<svg width="512" height="512" viewBox="0 0 512 512" version="1.1" id="logo-light" xml:space="preserve" xmlns:sodipodi="http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd" xmlns="http://www.w3.org/2000/svg" xmlns:svg="http://www.w3.org/2000/svg"><namedview id="view1" pagecolor="%23e6e6e6" bordercolor="%23000000" borderopacity="0.25" showgrid="false" /><defs id="defs1"><mask maskUnits="userSpaceOnUse" id="mask27"><rect style="fill:%23ffffff;fill-opacity:1" id="rect27" width="512" height="512" x="1.5301447" y="2.2148144" rx="140" /></mask></defs><g id="layer1" style="fill:none"><g id="g5" transform="translate(-1.5301447,-2.2148145)" mask="url(%23mask27)"><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5" width="128" height="128" x="385.53015" y="386.21481" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-2" width="128" height="128" x="193.53014" y="386.21481" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-3" width="128" height="128" x="1.530138" y="386.21481" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-26" width="128" height="128" x="385.53015" y="194.21481" rx="8" /><rect class="middle" style="fill:%231770ff;fill-opacity:1" id="rect5-9" width="160" height="160" x="177.53015" y="178.21481" rx="16" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-37" width="128" height="128" x="1.530138" y="194.21481" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-28" width="128" height="128" x="385.53015" y="2.2148159" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-6" width="128" height="128" x="193.53014" y="2.2148159" rx="8" /><rect style="fill:%234e4e4e;fill-opacity:1" id="rect5-31" width="128" height="128" x="1.530138" y="2.2148159" rx="8" /></g></g></svg>');
      -webkit-print-color-adjust: exact !important;
      print-color-adjust: exact !important;
    }
    .logo-container {
      line-height: 1.75em;
    }
    .manufacturer {
      font-size: 1.5em
    }
    .title {
      font-size: 4em; 
      margin-bottom: 5px;
    }
    li.pin {
      display: block; 
      margin-bottom: 10px; 
      padding: 10px; 
      box-sizing: border-box; 
      text-overflow: wrap; 
    }
    .pinout>h1 {
      font-size: 3.5em;
      margin-bottom: 20px;
    }
    .pinout>ul {
      height: 100%; 
      width: 90%; 
      font-size: 2.5em; 
      column-width: 300px; 
      column-gap: 20px; 
      list-style-type: square;
    }
    .feature {
      display: inline-block;
      font-family: "Source Code Pro", monospace;
      border: 1px solid #f6f6f6;
      padding: 2px 5px;
      margin: 1px 3px;
      -webkit-print-color-adjust: exact !important;
      print-color-adjust: exact !important;
    }
    .feature.gpio {
      background-color: #c00;
      color: #fff;
    }
    .feature.analog {
      background-color: #0b0;
      color: #fff;
    }
    .feature.power.positive {
      background-color: #c00;
      color: #fff;
    }
    .feature.power.negative {
      background-color: #666;
      color: #fff;
    }
    .feature.input {
      background-color: rgb(47, 109, 201);
      color: #fff;
    }
    .feature.output {
      background-color: rgba(15, 129, 0, 0.87);
      color: #fff;
    }
  }
    </style>`;
    let contents = `<div class="print-container">
        <h1 class="title">${_pinout.manufacturerPartName || _pinout.partName} (${_pinout.packageName})</h1>
        <span class="manufacturer">Manufacturer: ${_pinout.manufacturerName}</span>
        <div class="image-container"><img src="${getUrlForResource(_pinout.exportImage)}" onload="window.print();window.close()" /></div>
        <div class="pinout">
          <h1>Pins</h1>
          <ul>${pinsList}</ul>
        </div>
        <div class="footer">
          <div class="logo-container"><div class="logo"></div> Binner.io</div>
        </div>
      </div>`;
    var win = window.open('');
    win.document.write(`<html><head>${style}</head><body>${contents}</body></html>`);
    win.focus();
  };

  const renderPinBounds = (pinout) => {
    if (!(pinout?.pins?.length > 0)) return <></>;

    return (
      <div className="pinout-label-container">
        {pinout.pins.map((pin, key) => (
          <Popup
            key={key}
            wide='very'
            hoverable
            position="top center"
            content={<div>
              <div><b>Pin:</b> #{key + 1}</div>
              <div><b>Label:</b> {pin.label}</div>
              <div><b>Type:</b> {pin.type}</div>
              <div><b>IsGPIO:</b> {pin.isGPIO ? "Yes" : "No"}</div>
              <div><b>IsAnalog:</b> {pin?.isAnalog ? "Yes" : "No"}</div>
              <div><b>Functions:</b> <ul className="functions">{pin.functions?.map((func, fkey) => (<li key={fkey} style={{ color: func.color }}>{func.function}</li>))}</ul></div>
              <div><b>Description:</b> {pin?.description}</div>
            </div>
            }
            trigger={<div className="pinout-label" style={{
              //top: `${((pinBound.Bounds.Y - 5))}px`,
              //left: `${((pinBound.Bounds.X - 5))}px`,
              //width: `${((pinBound.Bounds.Width) + 10)}px`,
              //height: `${((pinBound.Bounds.Height) + 10)}px`,
              top: `${pin.bounds.yPerc}%`,
              left: `${pin.bounds.xPerc}%`,
              width: `${pin.bounds.widthPerc}%`,
              height: `${pin.bounds.heightPerc}%`
            }} />}
          />
        ))}
      </div>
    );
  };

  const renderPinFeaturesString = (pin) => {
    let isNegative = false;
    for (let f = 0; f < pin.functions.length; f++) {
      const func = pin.functions[f].function.toLowerCase() || '';
      if (func.includes('gnd') || func.includes('negative') || func.includes('ground'))
        isNegative = true;
    }

    let output = '<div style="font-size: 0.5em;">';
    if (pin.type === 'input') output += '<div class="feature input">INPUT</div>';
    if (pin.type === 'output') output += `<div class="feature output">OUTPUT</div>`;
    if (pin.type === 'power') output += `<div class="feature power ${isNegative ? 'negative' : 'positive'}">POWER</div>`;
    if (pin.isGPIO) output += '<div class="feature gpio">GPIO</div>';
    if (pin.isAnalog) output += '<div class="feature analog">ANALOG</div>';
    return output + '</div>'
  };

  const renderPinFeatures = (pin) => {
    let isNegative = false;
    for (let f = 0; f < pin.functions.length; f++) {
      const func = pin.functions[f].function.toLowerCase() || '';
      if (func.includes('gnd') || func.includes('negative') || func.includes('ground'))
        isNegative = true;
    }
    return (<div className="features">
      {pin.type === 'input' && <div className="feature input">INPUT<Icon name="arrow left" /></div>}
      {pin.type === 'output' && <div className="feature output">OUTPUT<Icon name="arrow right" /></div>}
      {pin.type === 'power' && <div className={`feature power ${isNegative ? 'negative' : 'positive'}`}>POWER</div>}
      {pin.isGPIO && <div className="feature gpio">GPIO</div>}
      {pin.isAnalog && <div className="feature analog">ANALOG</div>}
    </div>);
  };

  const renderPinLabel = (i, pinout) => {
    const pin = pinout.pins[i];
    return (<div key={`pin-label-${i}`} className="labelitem">
      <div className="title">Pin <b>#{i + 1}</b>: ({pin?.label})</div>
      <div className="description">{pin?.description}</div>
      {renderPinFeatures(pin)}
    </div>);
  };

  const renderPinout = (pinout) => {
    if (!pinout?.pinoutDefinition) return (<></>);

    const leftControls = [];
    const rightControls = [];
    const pinCount = pinout?.pins?.length || 0;
    const halfPinCount = Math.ceil(pinCount / 2.0);

    for (let i = 0; i < halfPinCount; i++) {
      leftControls.push(renderPinLabel(i, pinout));
    }

    for (let i = pinCount - 1; i >= halfPinCount; i--) {
      rightControls.push(renderPinLabel(i, pinout));
    }

    return (
      <Grid celled columns={3}>
        <Grid.Row>
          <Grid.Column>
            <div className="labels left">
              {leftControls.map((input, key) => input)}
            </div>
          </Grid.Column>
          <Grid.Column style={{ textAlign: "center", padding: '0' }}>
            <div>
              <div className="pinout-container">
                {renderPinBounds(pinout)}
                {pinout?.exportImage
                  ? (
                    <div>
                      <Image src={getUrlForResource(pinout.exportImage)} size="large" />
                    </div>
                  )
                  : (
                    <Placeholder>
                      <Placeholder.Image square />
                    </Placeholder>
                  )}
              </div>
              <div className="details">
                <div>Manufacturer: {pinout.manufacturerName}</div>
                <div>Part: <b>{pinout.manufacturerPartName || pinout.partName}</b></div>
                <div>Package: <b>{pinout.packageName}</b></div>
                <div>{datasheet && <a href={datasheet} rel="noreferer" target="_blank"><Icon name="file pdf" color="blue" /> View Datasheet</a>}</div>
              </div>
            </div>
          </Grid.Column>
          <Grid.Column>
            <div className="labels right">
              {rightControls.map((input, key) => input)}
            </div>
          </Grid.Column>
        </Grid.Row>
      </Grid>
    );
  };

  if (!pinout) return (<></>);

  return (<div>
    <Modal centered open={_isOpen || false} onClose={handleModalClose} className="pinoutViewerModal">
      <Modal.Header>{t('comp.pinoutViewerModal.header', "Pinout for")} {pinout?.manufacturerPartName || pinout?.partName} ({pinout?.packageName})</Modal.Header>
      <Modal.Content>
        <div className="centered">
          {renderPinout(_pinout)}
        </div>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handlePrint}><Icon name="print" /> {t('button.print', "Print")}</Button>
        <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
      </Modal.Actions>
    </Modal>
  </div>);
};

PinoutViewerModal.propTypes = {
  /** Event handler when adding a new part */
  pinout: PropTypes.object,
  /** Optional datasheet if known */
  datasheet: PropTypes.string,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
