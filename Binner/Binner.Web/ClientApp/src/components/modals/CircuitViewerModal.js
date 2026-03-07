import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from 'react-i18next';
import { Icon, Button, Modal, Popup, Image, Card, Tab, TabPane, MenuItem, Placeholder, Checkbox } from "semantic-ui-react";
import PhotoSizeSelectLargeIcon from '@mui/icons-material/PhotoSizeSelectLarge';
import PhotoSizeSelectSmallIcon from '@mui/icons-material/PhotoSizeSelectSmall';
import PropTypes from "prop-types";
import _ from "underscore";
import { toast } from "react-toastify";
import axios from "axios";
import { fetchApi } from "../../common/fetchApi";
import { getAuthToken } from "../../common/authentication";
import { getUrlForResource } from "../../common/resources";
import { getLocalData, setLocalData } from "../../common/storage";
import { PartImageLibrary } from "../../common/PartImageLibrary";
import { Clipboard } from "../Clipboard";
import { config } from "../../common/config";
import "./CircuitViewerModal.css";

/**
 * View a circuit
 */
export function CircuitViewerModal({ isOpen = false, circuit = { outputImage: null }, onClose, ...rest }) {
  const getViewPreference = (preferenceName) => {
    return getLocalData(preferenceName, { settingsName: 'circuitViewer' })
  };

  const setViewPreference = (preferenceName, value, options) => {
    setLocalData(preferenceName, value, { settingsName: 'circuitViewer', ...options });
  };

  const { t } = useTranslation();
  const [_isOpen, setIsOpen] = useState(false);
  const [imageSize, setImageSize] = useState(getViewPreference('imageSize') || 'large');
  const [layout, setLayout] = useState(getViewPreference('layout') || 'grid');
  const [printDisplayParts, setPrintDisplayParts] = useState(true);

  useEffect(() => {
    setIsOpen(isOpen);
  }, [isOpen]);

  const handleModalClose = (e) => {
    setIsOpen(false);
    if (onClose) onClose();
  };

  const handleSetLayout = (e, newLayout) => {
    e.preventDefault();
    e.stopPropagation();
    setViewPreference('layout', newLayout);
    setLayout(newLayout);
  };

  const handleSetSize = (e, newSize) => {
    e.preventDefault();
    e.stopPropagation();
    setViewPreference('imageSize', newSize);
    setImageSize(newSize);
  };

  const handleViewDatasheet = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, '_blank');
  }

  const handlePrint = (e, control) => {
    e.preventDefault();
    e.stopPropagation();
    if (!circuit) return;

    const sorted = _.sortBy(circuit.parts, item => item.reference);
    const skipTypes = ['net', 'netsignal', 'supply', 'supplynet'];
    const partEntries = [];
    for (let i = 0; i < sorted.length; i++) {
      const part = sorted[i];
      if (skipTypes.some(t => part.partType.toLowerCase().replace('/', '').replace(' ', '').startsWith(t))) continue;
      partEntries.push(`<li>[<b>${part.reference}</b>] ${part.partName}</li>`);
    }

    // some arbitrary spacing and font size adjustments based on parts count to try to make the printed page look nicer
    let partsSpacing = '15px';
    let partsFontSize = '2.25em';
    if (partEntries.length > 20) {
      partsFontSize = '1.75em';
      partsSpacing = '10px';
    }
    else if (partEntries.length > 40) {
      partsFontSize = '1.25em';
      partsSpacing = '5px';
    }

    const partsList = partEntries.join('\n');

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
      border: 5px solid #000;
      box-sizing: border-box;
    }
    .image-container img {
      width: 100%
      height: auto;
      object-fit: contain;
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
    .description {
      width: 80%;
      padding: 20px;
      font-size: 1.5em;
    }
    .parts-list {
      min-height: 30%;
      width: 100%;
      padding: 10px;
      border: 5px solid #eee;
      box-sizing: border-box;
      container-type: inline-size;
    }
    .parts-list ul {
      width: 90%; 
      font-size: ${partsFontSize}; 
      column-width: 300px; 
      column-gap: 10px; 
      list-style-type: square;
    }
    .parts-list ul li {
      display: list-item;
      margin-bottom: ${partsSpacing};
      padding: ${partsSpacing}; 
      box-sizing: border-box; 
      text-overflow: wrap;
    }
    </style>`;

    
    let contents = `<div class="print-container">
      <h1>${circuit.name}</h1>
      <div class="image-container"><img src="${getUrlForResource(circuit?.printImage)}" onload="window.print();window.close()" /></div>
      <div class="description">${circuit.description}</div>
      ${printDisplayParts ? `<div class="parts-list">
        <h1>Parts List</h1>
        <ul>${partsList}</ul>
      </div>` : ''}
      <div class="footer">
        <div class="logo-container"><div class="logo"></div> Binner.io</div>
      </div>
    </div>`;
    var win = window.open('');
    win.document.write(`<html><head>${style}</head><body>${contents}</body></html>`);
    win.focus();
  };

  const handleExport = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    fetchApi("/api/authentication/identity").then((_) => {
      axios
        .request({
          method: "post",
          url: `/api/export/schematic/parts`,
          headers: { Authorization: `Bearer ${getAuthToken()}` },
          data: circuit,
          responseType: "blob"
        })
        .then((blob) => {
          const filename = blob.headers['content-disposition']?.split('filename=')[1] || `schematic_${circuit.globalId}.csv`;
          // specifying blob filename, must create an anchor tag and use it as suggested: https://stackoverflow.com/questions/19327749/javascript-blob-filename-without-link
          var file = window.URL.createObjectURL(blob.data);
          var a = document.createElement("a");
          document.body.appendChild(a);
          a.style = "display: none";
          a.href = file;
          a.download = `${filename || 'schematic.csv'}`;
          a.click();
          window.URL.revokeObjectURL(file);
          toast.success(t('page.exportData.exportSuccess', "Data exported successfully!"));
        })
        .catch((error) => {
          toast.dismiss();
          console.error("error", error);
          toast.error(t('page.exportData.exportFailed', "Export data failed!"));
        });
    });
  };

  const getPartImage = (part) => {
    if (!part) return null;
    const baseUrl = `${config.STATIC_URL}/partimagelibrary/`;
    const partType = part.partType?.toLowerCase().replace(' ', '').replace('/', '') || '';
    const partName = part.partName?.toLowerCase() || '';
    // first try to find an exact match for part type and value
    let partImage = _.find(PartImageLibrary, item => item.partType.some(i => partType.startsWith(i)) && item.values && item.values.some(i => partName.startsWith(i)));
    if (partImage) return baseUrl + partImage.image;
    // then try to find an exact match for value
    partImage = _.find(PartImageLibrary, item => item.values && item.values.includes(partName));
    if (partImage) return baseUrl + partImage.image;
    // then try to find a match for part type only
    partImage = _.find(PartImageLibrary, item => item.partType.some(i => partType.startsWith(i)));
    if (partImage) return baseUrl + partImage.image;

    return null;
  };

  const renderPartPopup = (part) => {
    const partImageUrl = getPartImage(part);
    return (<Card className="partPopup">
      {partImageUrl && <Image src={partImageUrl} />}
      <Card.Content>
        <Card.Header>{part.reference}</Card.Header>
        <Card.Meta>
          <Clipboard text={part.partName} /><span className="partName">{part.partName}</span>
          {part.partType && <span className="partType">{part.partType}</span>}
        </Card.Meta>
      </Card.Content>
      <Card.Description>
        {part.description &&
          <div className="partdescription">
            <div className="title">Description:</div>
            <div className="value">{part.description}</div>
          </div>
        }
        {part.role &&
          <div className="role">
            <div className="title">Role in this circuit:</div>
            <div className="value">{part.role}</div>
          </div>
        }
      </Card.Description>
      <Card.Content extra>
        {part.partNumber?.datasheetUrl && <a href="#" onClick={e => handleViewDatasheet(e, part.partNumber.datasheetUrl)}><Icon name='file pdf' color="blue" /> View Datasheet</a>}
        {part.partNumberManufacturer?.datasheetUrl && <a href="#" onClick={e => handleViewDatasheet(e, part.partNumberManufacturer.datasheetUrl)}><Icon name='file pdf' color="blue" /> View Datasheet</a>}
      </Card.Content>
    </Card>);
  };

  const drawOutputPartBoundaries = () => {
    const parts = circuit.parts;

    if (parts?.length > 0) {
      const bounds = [];
      for (let i = 0; i < parts.length; i++) {
        const part = parts[i];
        // if part has no bounds, skip it
        if (part.width === 0 || part.height === 0) continue;
        // dimensions must be expressed in percentages of the image size
        // the coordinates are normalized from 0-1000, so we need to convert them to percentages for display
        var x = part.x / 10;
        var y = part.y / 10;
        var width = part.width / 10;
        var height = part.height / 10;
        bounds.push(<Popup key={i}
          wide='very'
          hoverable
          content={renderPartPopup(part)}
          trigger={<div className="part-boundary" style={{ left: `${x}%`, top: `${y}%`, width: `${width}%`, height: `${height}%` }}></div>}
        />);
      }
      return (<div className="part-boundaries">{bounds}</div>);
    }
    return (<></>);
  };

  const renderPartsList = (parts) => {
    if (layout === 'grid') {
      return (_.sortBy(parts, item => item.reference)?.map((part, key) => (
        <div key={key} className="part">
          <div>
            <span className="reference">{part.reference} - </span>
            <span className="partName">{part.partName}</span>
            {part?.partNumberManufacturer?.datasheetUrl?.length > 0 && <span className="datasheet"><a href={part.partNumberManufacturer.datasheetUrl} rel="noreferer" target="_blank"><Icon name="file pdf outline" /></a></span>}
            {part.partType && <span className="type">{part.partType}</span>}
          </div>
          <div className="description">{part.description}</div>
        </div>
      )));
    }

    // list
    return (_.sortBy(parts, item => item.reference)?.map((part, key) => (
      <li key={key} className="part">
        <span>
          <span className="reference">{part.reference} - </span>
          <span className="partName">{part.partName}</span>
          {part?.partNumberManufacturer?.datasheetUrl?.length > 0 && <span className="datasheet"><a href={part.partNumberManufacturer.datasheetUrl} rel="noreferer" target="_blank"><Icon name="file pdf outline" /></a></span>}
          {part.partType && <span className="type">{part.partType}</span>}
        </span>
      </li>
    )));
  };

  if (!circuit || !isOpen) return (<></>);

  const tabPanes = [
    {
      menuItem: <MenuItem disabled={!circuit.outputImage} key={"schematic"}>Schematic</MenuItem>,
      render: () => <TabPane className="centered">
        {circuit.outputImage
          ? <div className={`image-container ${imageSize}`}>
            {drawOutputPartBoundaries()}
            <img src={getUrlForResource(circuit.outputImage)} id="outputImage" />
          </div>
          : <Placeholder fluid style={{ display: 'flex', height: '100%', alignItems: 'center', justifyContent: 'center' }}>No schematic available.</Placeholder>
        }
      </TabPane>
    },
    {
      menuItem: <MenuItem disabled={!circuit.printImage} key={"print"}><Icon name="print" /> Print</MenuItem>,
      render: () => <TabPane className="centered">
        <div className="tools print">
          <Popup content={<span>Include parts list in print output</span>} trigger={<Checkbox toggle label="Include parts list" onChange={() => setPrintDisplayParts(prev => !prev)} checked={printDisplayParts} className="small" />} />
          <a href="#" onClick={handlePrint}><Icon name="print" /> Print</a>
        </div>
        {/*drawOutputPartBoundaries()*/}
        {circuit.printImage
          ? <div className={`image-container ${imageSize}`}>
              <img src={getUrlForResource(circuit.printImage)} id="printImage" />
            </div>
          : <Placeholder fluid style={{ display: 'flex', height: '100%', alignItems: 'center', justifyContent: 'center' }}>No print image available.</Placeholder>
        }
      </TabPane>
    }
  ];

  return (<div>
    <Modal centered open={_isOpen || false} onClose={handleModalClose} className="circuitViewerModal">
      <Modal.Header>{t('comp.circuitViewerModal.header', "Schematic")} - {circuit.name}</Modal.Header>
      <Modal.Content>
        <div>
          <div className="tools imagesize">
            <div className="image-size">
              <Popup content={<p>{imageSize === 'large' ? 'Switch to small schematic' : 'Switch to large schematic'}</p>} trigger={imageSize === 'large' ? <PhotoSizeSelectLargeIcon onClick={e => handleSetSize(e, 'small')} /> : <PhotoSizeSelectSmallIcon onClick={e => handleSetSize(e, 'large')} />} />
            </div>
          </div>
          <div className="schematic-container">
            <div className="schematic">
              <Tab panes={tabPanes} />
            </div>
          </div>
          <div className="circuit-description">
            <div className="description">Description</div>
            <p>{circuit.description}</p>
          </div>
          <div className="partsListContainer">
            <h2>Parts List</h2>
            <div className="tools partslist">
              <div className="view-layout">Layout: <Link onClick={e => handleSetLayout(e, 'grid')}><Icon name="grid layout" color={layout === 'grid' ? 'blue' : 'black'} /></Link> <Link onClick={e => handleSetLayout(e, 'list')}><Icon name="list layout" color={layout === 'list' ? 'blue' : 'black'} /></Link></div>
              <div className="export"><a href="#" onClick={handleExport}><Icon name="share" /> Export to CSV</a></div>
            </div>
            <div className={`partsList ${layout === 'list' ? 'list' : 'grid'}`}>
              {renderPartsList(circuit.parts)}
            </div>
          </div>
        </div>
      </Modal.Content>
      <Modal.Actions>
        <Button onClick={handlePrint}><Icon name="print" /> {t('button.print', "Print")}</Button>
        <Button onClick={handleModalClose}>{t('button.close', "Close")}</Button>
      </Modal.Actions>
    </Modal>
  </div>);
};

CircuitViewerModal.propTypes = {
  /** Event handler when adding a new part */
  circuit: PropTypes.object,
  /** Event handler when closing modal */
  onClose: PropTypes.func,
  /** Set this to true to open model */
  isOpen: PropTypes.bool
};
