import React, { useState } from "react";
import { Form, Popup } from "semantic-ui-react";
import { toast } from "react-toastify";
import { BarcodeScannerInput } from "../../components/BarcodeScannerInput";
//import reactStringReplace from "react-string-replace";
import "./BarcodeScanner.css";

export function BarcodeScanner(props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [barcodeValue, setBarcodeValue] = useState('Waiting for input...');
  const [rsDetected, setRsDetected] = useState(false);
  const [gsDetected, setGsDetected] = useState(false);
  const [eotDetected, setEotDetected] = useState(false);
  const [invalidBarcodeDetected, setInvalidBarcodeDetected] = useState(false);

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
    toast.info(`Barcode type ${input.type} received`);
  };

  let barcodeObject = barcodeValue;
  //let barcodeObject = reactStringReplace(barcodeValue, "\u241E", (match, i) => (<span key={i*2} className="rs">{match}</span>));
  //barcodeObject = reactStringReplace(barcodeObject, "\u241D", (match, i) => (<span key={i*3} className="gs">{match}</span>));
  //barcodeObject = reactStringReplace(barcodeObject, "\u2404", (match, i) => (<span key={i*4} className="eot">{match}</span>));

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={4} swallowKeyEvent={false} />
      <h1>Barcode Scanner</h1>
      <p>Test your barcode scanner to see what values it outputs.</p>
      <Form>
        <div>
          <code><pre>{barcodeObject}</pre></code>
          <div className="block-container">
            <label>Detected:</label>
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
          </div>
        </div>
      </Form>
    </div>
  );
}
