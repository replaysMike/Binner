import React, { useState } from "react";
import { Form, Popup } from "semantic-ui-react";
import { toast } from "react-toastify";
import { BarcodeScannerInput } from "../../components/BarcodeScannerInput";
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

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    // ignore single keypresses
    setRsDetected(false);
    setGsDetected(false);
    setEotDetected(false);

    if (input && input.rawValue) {
      const rawValueFormatted = input.rawValue
        .replaceAll("\u001e","<RS>")
        .replaceAll("\u005e","<RS>")
        .replaceAll("\u001d", "<GS>")
        .replaceAll("\u005d", "<GS>")
        .replaceAll("<RS>\u0004", "<RS><EOT>")
        .replaceAll("<RS>\u0044", "<RS><EOT>");
      const json = {...input, rawValueFormatted: rawValueFormatted};
      setBarcodeValue(JSON.stringify(json, null, 2));
      setRsDetected(input.rsDetected);
      setGsDetected(input.gsDetected);
      setEotDetected(input.eotDetected);
    } else{
      setBarcodeValue(JSON.stringify(input, null, 2));
    }
    toast.info(`Barcode type ${input.type} received`);
  };

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={4} swallowKeyEvent={false} />
      <h1>Barcode Scanner</h1>
      <p>Test your barcode scanner to see what values it outputs.</p>
      <Form>
        <div>
          <code><pre>{barcodeValue}</pre></code>
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
          </div>
        </div>
      </Form>
    </div>
  );
}
