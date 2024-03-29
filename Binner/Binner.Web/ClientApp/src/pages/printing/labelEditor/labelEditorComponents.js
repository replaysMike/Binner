import { QRCodeIcon, DataMatrixIcon, AztecCodeIcon, Code128NoTextIcon, PDF417Icon } from "./icons";

export const getChildrenByName = (name) => {
	switch(name) {
		case 'qrCode':
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
		case 'location':
			return "Location";
		case 'binNumber':
			return "Bin Number";
		case 'binNumber2':
			return "Bin Number2";
		default:
			return name;
	}
};