import React, { useState, useRef, useEffect } from "react";
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { Form, Popup, Input, Icon, Button, Breadcrumb, Table } from "semantic-ui-react";
import { toast } from "react-toastify";
import { BarcodeScannerInput } from "../../components/BarcodeScannerInput";
import { Clipboard } from "../../components/Clipboard";
import { GetTypeName, BarcodeProfiles } from "../../common/Types";
import ProtectedInput from "../../components/ProtectedInput";
import { Format12HourTimeSeconds } from "../../common/datetime";
import { format } from "date-fns";

import reactStringReplace from "react-string-replace";
import "./BarcodeScanner.css";

export function BarcodeScanner(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [barcodeInput, setBarcodeInput] = useState(null);
  const [barcodeValue, setBarcodeValue] = useState(t('page.barcodeScanner.waitingForInput', "Waiting for input..."));
  const [rsDetected, setRsDetected] = useState(false);
  const [gsDetected, setGsDetected] = useState(false);
  const [eotDetected, setEotDetected] = useState(false);
  const [scanHistory, setScanHistory] = useState([]);
  const [config, setConfig] = useState({});
  const [invalidBarcodeDetected, setInvalidBarcodeDetected] = useState(false);
  const [customBufferTime, setCustomBufferTime] = useState(0);
  const [configOverride, setConfigOverride] = useState(null);
  const [dummy, setDummy] = useState(null);
  const [unprotectedDummy, setUnprotectedDummy] = useState(null);
  const [unprotectedDummyStartTime, setUnprotectedDummyStartTime] = useState(null);
  const [dummyStartTime, setDummyStartTime] = useState(null);
  const dummyTimerRef = useRef();
  const unprotectedDummyTimerRef = useRef();

  useEffect(() => {
    return () => {
      toast.dismiss();
    };
  }, []);

  const handleSetConfig = (config) => {
    setConfig(config);
    setCustomBufferTime(config?.bufferTime);
  };

  const handleChange = (e, control) => {
    switch(control.name){
      case 'customBufferTime':
        const val = control.value?.replace('ms', '').replace(' ', '');
        setCustomBufferTime(control.value);
        setConfigOverride({...control, bufferTime: parseInt(val)})
        break;
      case 'dummy':
        // measure how long a barcode scan event takes
        if (dummy === null || dummy.length === 0) setDummyStartTime(new Date().getTime());
        clearTimeout(dummyTimerRef.current);
        dummyTimerRef.current = setTimeout(() => { console.debug('Dummy took', new Date().getTime() - dummyStartTime - 500); }, 500);
        setDummy(control.value);
        break;
      case 'unprotectedDummy':
        if (unprotectedDummy === null || unprotectedDummy.length === 0) setUnprotectedDummyStartTime(new Date().getTime());
        console.debug('unprotectedDummy onChange', control.value);
        clearTimeout(unprotectedDummyTimerRef.current);
        unprotectedDummyTimerRef.current = setTimeout(() => { console.debug('Unprotected Dummy took', new Date().getTime() - unprotectedDummyStartTime - 500); }, 500);
        setUnprotectedDummy(control.value);
        break;
      default:
        break;
    }
  };

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    viewItem(input);
    const item = {
      ...input,
      success: !input.invalidBarcodeDetected,
      logDate: new Date(),
    };
    scanHistory.unshift(item);
    setScanHistory(scanHistory);
    toast.info(t('success.barcodeTypeReceived', "Barcode type {{type}} received", { type: input.type }));
  };

  const viewItem = (input) => {
    // ignore single keypresses
    setRsDetected(false);
    setGsDetected(false);
    setEotDetected(false);
    setInvalidBarcodeDetected(false);

    if (input && input.rawValue) {
      const rawValueFormatted = input.rawValue
        .replaceAll("^\u0044", "\u241e\u2404") // ^EOT
        .replaceAll("\u0004", "\u2404") // EOT
        .replaceAll("\u001e", "\u241e") // RS (30)
        .replaceAll("\u005e", "\u241e") // RS (94) ^
        .replaceAll("\u001d", "\u241d") // GS (29)
        .replaceAll("\u005d", "\u241d") // GS (93) ]
        .replaceAll("\r", "\u240d") // CR
        .replaceAll("\n", "\u240a") // LF
        .replaceAll("\u001c", "\u241c") // FS
        ;
      const json = { ...input, rawValueFormatted: rawValueFormatted };
      setBarcodeInput(input);
      setBarcodeValue(JSON.stringify(json, null, 2));
      setRsDetected(input.rsDetected);
      setGsDetected(input.gsDetected);
      setEotDetected(input.eotDetected);
      setInvalidBarcodeDetected(input.invalidBarcodeDetected);

    } else {
      setBarcodeInput(input);
      setBarcodeValue(JSON.stringify(input, null, 2));
    }
  };

  const handleReset = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setBarcodeValue(t('page.barcodeScanner.waitingForInput', "Waiting for input..."));
    setRsDetected(false);
    setGsDetected(false);
    setEotDetected(false);
    setInvalidBarcodeDetected(false);
    setDummy(null);
    setUnprotectedDummy(null);
  };

  const handleClear = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setScanHistory([]);
  };

  const loadHistory = (e, item) => {
    e.preventDefault();
    e.stopPropagation();
    viewItem(item);
  };

  const getRandomKey = () => Math.floor(Math.random() * 999999);
  let barcodeObject = reactStringReplace(barcodeValue, "\u241E", (match, i) => (<span key={getRandomKey()} className="control rs">{match}</span>));
  barcodeObject = reactStringReplace(barcodeObject, "\u241D", (match, i) => (<span key={getRandomKey()} className="control gs">{match}</span>));
  barcodeObject = reactStringReplace(barcodeObject, "\u2404", (match, i) => (<span key={getRandomKey()} className="control eot">{match}</span>));
  barcodeObject = reactStringReplace(barcodeObject, "\u240d", (match, i) => (<span key={getRandomKey()} className="control cr">{match}</span>));
  barcodeObject = reactStringReplace(barcodeObject, "\u240a", (match, i) => (<span key={getRandomKey()} className="control lf">{match}</span>));
  barcodeObject = reactStringReplace(barcodeObject, "\u241c", (match, i) => (<span key={getRandomKey()} className="control fs">{match}</span>));

  return (
    <div>
      <BarcodeScannerInput 
        onReceived={handleBarcodeInput} 
        minInputLength={4} 
        swallowKeyEvent={false} 
        config={configOverride} 
        onSetConfig={handleSetConfig} 
        onDisabled={() => toast.error('Barcode scanning support is currently disabled. See Settings page.', { autoClose: false })} 
      />
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.barcodeScanner', "Barcode Scanner")}</Breadcrumb.Section>
      </Breadcrumb>
      <h1>{t('page.barcodeScanner.title', "Barcode Scanner")}</h1>
      <p>{t('page.barcodeScanner.description', "Test your barcode scanner to see what values it outputs.")}</p>
      <Form>
        <div>
          <ProtectedInput 
            name="dummy" 
            placeholder="A protected input text box that can handle barcode events" 
            value={dummy || ''} 
            onChange={handleChange}
            focus
            />
          <Form.Input name="unprotectedDummy" placeholder="A regular unprotected input text box" value={unprotectedDummy || ''} onChange={handleChange} />
          {barcodeObject.length > 1 && (<div style={{ display: 'flex', marginBottom: '5px', float: 'right' }}>
            <div style={{ padding: '0 10px' }}>
              <Clipboard text={barcodeValue} /> <span>{t('button.copy', "Copy")}</span>
            </div>
            <div style={{ padding: '0 10px' }}>
              <Clipboard text={barcodeInput?.rawValue} /><span>{t('button.copyRaw', "Copy Raw")}</span>
            </div>
          </div>)}
          <code style={{marginBottom: '0'}}><pre>{barcodeObject}</pre></code>
          <h5 style={{marginTop: '10px'}}>Barcode Config</h5>
          <Form.Group>
            <Form.Field width={1}>
              <label>Enabled:</label> {config.enabled ? 'true' : 'false'} 
            </Form.Field>
            <Form.Field width={2}>
              <label>Profile:</label> {GetTypeName(BarcodeProfiles, config.profile)} 
            </Form.Field>
            <Form.Field width={2}>
              <label>BufferTime (ms):</label> 
              <Popup
                wide='very'
                hoverable
                content={<p><Icon name="arrow down" /> A lower value may miss keypress events from the barcode scanner.<br/>
                  <Icon name="arrow up" /> A higher value is more likely to catch all keypresses but will take longer to process.<br/>
                  <Icon name="setting" /> This can be configured permanently in <Link to='/settings'>Settings</Link>.<br/>
                  <i>Default: 80</i></p>}
                trigger={<Input transparent value={customBufferTime} name="customBufferTime" onChange={handleChange}>
                  <Icon name="clock" />
                  <input />
                </Input>}
              />
              
            </Form.Field>
            <Form.Field width={1}>
              <label>Prefix2D:</label> <pre>{config.prefix2D}</pre>
            </Form.Field>
            <Form.Field>
              <div className="tips">
                <label>Tips</label>
                <ul>
                  <li>Try increasing the BufferTime if the label is missing information or showing up as multiple scans.</li>
                  <li>Try decreasing the BufferTime if the scan is very slow.</li>
                  <li>Try scanning the same part many times. If the length changes then your scanner is not performing well.</li>
                  <li>Make sure the window is in focus by clicking on it, as barcode scanners emulate keyboard strokes.</li>
                </ul>
              </div>
            </Form.Field>
          </Form.Group>
            
          <div className="block-container">
            <label>{t('page.barcodeScanner.detected', "Detected")}:</label>
            <Popup 
              wide
              content={<p>RS, or record separator (<i>ASCII 30, unicode \u001e or ASCII 94, unicode \u005e</i>) is a hidden data code indicating the start of a record. It is optional for barcodes.</p>}
              trigger={<div className={`block ${rsDetected ? 'active' : ''}`}>RS</div>}
            />
            <Popup 
              wide
              content={<p>GS, or group separator (<i>ASCII 29, unicode \u001d or ASCII 93, unicode \u005d</i>) is a hidden data code indicating the start of a group of data. It is the most important encoding used for 2D Data Matrix, QR and Aztec barcodes.</p>}
              trigger={<div className={`block ${gsDetected ? 'active' : ''}`}>GS</div>}
            />
            <Popup 
              content={<p>EOT, or end of transmission (ASCII 04, unicode \u0004) separator is a hidden data code indicating the end of a barcode transmission. It is optional for barcodes.</p>}
              trigger={<div className={`block ${eotDetected ? 'active' : ''}`}>EOT</div>}
            />
            <Popup 
              wide
              content={<p>Some barcode scanners (cheaper models) incorrectly decode some barcodes. This is detected as any barcode with empty GS/RS separators. It is also known that some older DigiKey barcodes contain them at the end, and will be ignored.</p>}
              trigger={<div className={`block ${invalidBarcodeDetected ? 'active red' : ''}`}>Invalid</div>}
            />

            <Popup 
              content={<p>Reset the scan information.</p>}
              trigger={<Button type='button' onClick={handleReset}>Reset</Button>}
            />

            <Popup
              content={<p>Clear the history.</p>}
              trigger={<Button type='button' onClick={handleClear} style={{width: '150px'}}>Clear History</Button>}
            />
          </div>

          <div>
            <h5>{t('page.barcodeScanner.history', "History")}</h5>
            <Table className="history">
              <Table.Header>
                <Table.Row>
                  <Table.Cell></Table.Cell>
                  <Table.Cell>Successful</Table.Cell>
                  <Table.Cell>Description</Table.Cell>
                  <Table.Cell>Type</Table.Cell>
                  <Table.Cell>Length</Table.Cell>
                  <Table.Cell>Codes</Table.Cell>
                  <Table.Cell>Date</Table.Cell>
                </Table.Row>
              </Table.Header>
              <Table.Body>
                {scanHistory.length > 0 
                  ? scanHistory.map((item, key) => 
                  <Table.Row key={key}>
                    <Table.Cell><Link onClick={e => loadHistory(e, item)}>View</Link></Table.Cell>
                    <Table.Cell>{item.success ? <Icon name="check circle" color="green" /> : <Icon name="times circle" color="red" />}</Table.Cell>
                      <Table.Cell>{item.value.supplierPartNumber || item.value.description || item.value.mfgPartNumber || item.value.salesOrder || "unknown"}</Table.Cell>
                    <Table.Cell>{item.type}</Table.Cell>
                      <Table.Cell>{item.rawValue.length}</Table.Cell>
                    <Table.Cell>{item.rsDetected && "RS"} {item.gsDetected && "GS"} {item.eotDetected && "EOT"}</Table.Cell>
                    <Table.Cell>{format(item.logDate, Format12HourTimeSeconds)}</Table.Cell>
                  </Table.Row>
                )
                : <Table.Row><Table.Cell colSpan="7" textAlign="center"> No history available.</Table.Cell></Table.Row>}
              </Table.Body>
            </Table>
          </div>
        </div>
      </Form>
    </div>
  );
}
