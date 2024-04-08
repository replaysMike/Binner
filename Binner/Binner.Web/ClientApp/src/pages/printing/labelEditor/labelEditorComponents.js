import { QRCodeIcon, DataMatrixIcon, AztecCodeIcon, Code128NoTextIcon, PDF417Icon } from "./icons";

export const getChildrenByName = (name) => {
  switch (name) {
    case 'qrCode':
      return <QRCodeIcon />;
    case 'qrCodeLink':
      return <QRCodeIcon />;
    case 'dataMatrix2d':
      return <DataMatrixIcon />;
    case 'aztecCode':
      return <AztecCodeIcon />;
    case 'code128':
      return <Code128NoTextIcon style={{ width: "160px" }} />;
    case 'pdf417':
      return <PDF417Icon style={{ width: "160px" }} />;
    case 'text':
      return <span>Regular Text</span>;
    case 'partNumber':
      return "Part Number";
    case 'partId':
      return "Part Id";
    case 'manufacturerPartNumber':
      return "Mfr Part Number";
    case 'manufacturer':
      return "Manufacturer";
    case 'description':
      return "Description";
    case 'partType':
      return "Part Type";
    case 'mountingType':
      return "Mounting Type";
    case 'packageType':
      return "Package Type";
    case 'cost':
      return "Cost";
    case 'keywords':
      return "Keywords";
    case 'quantity':
      return "Quantity";
    case 'digikeyPartNumber':
      return "DigiKey P/N";
    case 'mouserPartNumber':
      return "Mouser P/N";
    case 'arrowPartNumber':
      return "Arrow P/N";
    case 'tmePartNumber':
      return "TME P/N";
    case 'location':
      return "Location";
    case 'binNumber':
      return "Bin Number";
    case 'binNumber2':
      return "Bin Number2";
    case 'extensionValue1':
      return "Extension Value 1";
    case 'extensionValue2':
      return "Extension Value 2";
    case 'footprintName':
      return "Footprint Name";
    case 'symbolName':
      return "Symbol Name";
    case 'projectId':
      return "Project Id";
    case 'partTypeId':
      return "Part Type Id";
    default:
      return name;
  }
};