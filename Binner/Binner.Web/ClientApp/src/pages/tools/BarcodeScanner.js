import React, { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Form, Popup, Input, Icon, Button } from "semantic-ui-react";
import { toast } from "react-toastify";
import { BarcodeScannerInput } from "../../components/BarcodeScannerInput";
import { GetTypeName, BarcodeProfiles } from "../../common/Types";
import ProtectedInput from "../../components/ProtectedInput";
import reactStringReplace from "react-string-replace";
import "./BarcodeScanner.css";

export function BarcodeScanner(props) {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [barcodeValue, setBarcodeValue] = useState(t('page.barcodeScanner.waitingForInput', "Waiting for input..."));
  const [rsDetected, setRsDetected] = useState(false);
  const [gsDetected, setGsDetected] = useState(false);
  const [eotDetected, setEotDetected] = useState(false);
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
        setCustomBufferTime(control.value);
        setConfigOverride({...control, bufferTime: parseInt(control.value)})
        break;
      case 'dummy':
        // measure how long a barcode scan event takes
        if (dummy === null || dummy.length === 0) setDummyStartTime(new Date().getTime());
        clearTimeout(dummyTimerRef.current);
        dummyTimerRef.current = setTimeout(() => { console.log('Dummy took', new Date().getTime() - dummyStartTime - 500); }, 500);
        setDummy(control.value);
        break;
      case 'unprotectedDummy':
        if (unprotectedDummy === null || unprotectedDummy.length === 0) setUnprotectedDummyStartTime(new Date().getTime());
        console.log('unprotectedDummy onChange', control.value);
        clearTimeout(unprotectedDummyTimerRef.current);
        unprotectedDummyTimerRef.current = setTimeout(() => { console.log('Unprotected Dummy took', new Date().getTime() - unprotectedDummyStartTime - 500); }, 500);
        setUnprotectedDummy(control.value);
        break;
      default:
        break;
    }
  };

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    // ignore single keypresses
    setRsDetected(false);
    setGsDetected(false);
    setEotDetected(false);
    setInvalidBarcodeDetected(false);

    if (input && input.rawValue) {
      const rawValueFormatted = input.rawValue
        .replaceAll("^\u0044", "\u241e\u2404") // ^EOT
        .replaceAll("\u0004", "\u2404") // EOT
        .replaceAll("\u001e","\u241e") // RS (30)
        .replaceAll("\u005e","\u241e") // RS (94) ^
        .replaceAll("\u001d", "\u241d") // GS (29)
        .replaceAll("\u005d", "\u241d") // GS (93) ]
        .replaceAll("\r", "\u240d") // CR
        .replaceAll("\n", "\u240a") // LF
        .replaceAll("\u001c", "\u241c") // FS
        ;
      const json = {...input, rawValueFormatted: rawValueFormatted};
      setBarcodeValue(JSON.stringify(json, null, 2));
      setRsDetected(input.rsDetected);
      setGsDetected(input.gsDetected);
      setEotDetected(input.eotDetected);
      setInvalidBarcodeDetected(input.invalidBarcodeDetected);
    } else{
      setBarcodeValue(JSON.stringify(input, null, 2));
    }
    toast.info(t('success.barcodeTypeReceived', "Barcode type {{type}} received", { type: input.type }));
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
          <code><pre>{barcodeObject}</pre></code>
          <h5>Barcode Config</h5>
          <Form.Group>
            <Form.Field width={1}>
              <label>Enabled:</label> {config.enabled ? 'true' : 'false'} 
            </Form.Field>
            <Form.Field width={2}>
              <label>Profile:</label> {GetTypeName(BarcodeProfiles, config.profile)} 
            </Form.Field>
            <Form.Field width={2}>
              <label>BufferTime (ms):</label> 
              <Input transparent value={customBufferTime} name="customBufferTime" onChange={handleChange}> 
                <Icon name="clock" />
                <input />
              </Input>
            </Form.Field>
            <Form.Field width={1}>
              <label>BarcodePrefix2D:</label> {config.barcodePrefix2D}
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
              content={<p>Some older DigiKey barcodes are encoded incorrectly. If an older part label that was encoded incorrectly is detected it will be indicated here, and Binner has corrected for it.</p>}
              trigger={<div className={`block ${invalidBarcodeDetected ? 'active red' : ''}`}>Invalid</div>}
            />

            <Popup 
              content={<p>Reset the form output.</p>}
              trigger={<Button type='button' onClick={handleReset}>Reset</Button>}
            />
          </div>
        </div>
      </Form>
    </div>
  );
}
