import { Modal, Image, Grid } from "semantic-ui-react";
import { useTranslation } from "react-i18next";
import React, { useState, useEffect } from "react";

export function BarcodeExamplesModal({ isOpen, onClose }) {
  const { t } = useTranslation();
  const [ open, setOpen ] = useState(isOpen);

  useEffect(() => {
    setOpen(isOpen);
  }, [isOpen]);

  const handleCloseModal = () => {
    if (onClose) onClose();
  };

  return (
    <Modal centered open={open} onClose={handleCloseModal} closeOnDimmerClick={true} closeOnDocumentClick={true}>
      <Modal.Header>Barcode Examples</Modal.Header>
      <Modal.Content scrolling>
        <Modal.Description>
          <blockquote>Scanning part labels on the screen may or may not work well depending on your display and barcode scanner.<br/>Alternatively you can print them out on paper.</blockquote>
          <h3>Part Labels</h3>

          <div style={{display: 'flex', flexWrap: 'wrap'}}>
            <div style={{flex: '1'}}>
              <img src="/image/barcode/examples/Datamatrix_Part-CL21A475KAQNNNE.png" />
              <p>DigiKey Part<br />CL21A475KAQNNNE<br/>CAP CER 4.7UF 25V X5R 0805<br/>Qty: 63</p>
            </div>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Datamatrix_Part-NCV7805BDTRKG.png" />
              <p>DigiKey Part<br />NCV7805BDTRKG<br/>IC REG LINEAR 5V 1A DPAK<br/>Qty: 25</p>
            </div>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Datamatrix_Part-RMCF0805FT10R0.png" />
              <p>DigiKey Part<br />RMCF0805FT10R0<br/>RES 10 OHM 1% 1/8W 0805<br/>Qty: 784</p>
            </div>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Datamatrix_Part-1206.png" />
              <p>DigiKey Part<br />1206<br/>SMT ADAPTERS 3 PACK 20SOIC/TSSOP<br/>Qty: 2</p>
            </div>
          </div>

          <hr />
          <h3>Packlist</h3>
          
          <div style={{ display: 'flex', flexWrap: 'wrap' }}>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Datamatrix_Packlist_Order_90837700.png" />
              <p>DigiKey Packlist<br />Sales Order: 90837700<br/>Invoice: 109914231</p>
            </div>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Datamatrix_Packlist_LineItem-CL21A106KOQNNNGG.png" />
              <p>DigiKey Packlist LineItem<br />MFG Part: CL21A106KOQNNNG<br />BINNER 24V CAB C10<br/>Qty: 500</p>
            </div>
            <div style={{ flex: '1' }}>
              <img src="/image/barcode/examples/Code128_Part-LPN_007388165.png" />
              <p>DigiKey Packlist LineItem<br />CL21A106KOQNNNG</p>
            </div>
          </div>

          <hr />
          <footer>
          </footer>
        </Modal.Description>
      </Modal.Content>
    </Modal>
  );
}
