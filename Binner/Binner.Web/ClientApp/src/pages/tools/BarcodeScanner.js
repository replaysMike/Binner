import React, { useState, useEffect, useMemo } from "react";
import { Link } from "react-router-dom";
import _ from "underscore";
import { Label, Button, Image, Form, Table, Segment, Dimmer, Checkbox, Loader, Popup } from "semantic-ui-react";
import { toast } from "react-toastify";
import { BarcodeScannerInput } from "../../components/BarcodeScannerInput";
import "./BarcodeScanner.css";

export function BarcodeScanner(props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const [isKeyboardListening, setIsKeyboardListening] = useState(true);
  const [barcodeValue, setBarcodeValue] = useState('Waiting for input...');

  // debounced handler for processing barcode scanner input
  const handleBarcodeInput = (e, input) => {
    // ignore single keypresses
    if (input && input.rawValue) {
      const json = {...input, rawValueFormatted: input.rawValue.replaceAll("\u001e","<RS>").replaceAll("\u001d", "<GS>")};
      setBarcodeValue(JSON.stringify(json, null, 2));  
    } else{
      setBarcodeValue(JSON.stringify(input, null, 2));
    }
    toast.info(`Barcode type ${input.type} received`);
  };

  return (
    <div>
      <BarcodeScannerInput onReceived={handleBarcodeInput} listening={isKeyboardListening} minInputLength={4} />
      <h1>Barcode Scanner</h1>
      <p>Test your barcode scanner to see what values it outputs.</p>
      <Form>
        <div>
          <code><pre>{barcodeValue}</pre></code>
        </div>
      </Form>
    </div>
  );
}
