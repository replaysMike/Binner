import _ from "underscore";
import isSvg from "is-svg";
import {
  CableIcon,
  CapacitorIcon,
  CeramicCapacitorIcon,
  MicaCapacitorIcon,
  PaperCapacitorIcon,
	SuperCapacitorIcon,
  MylarCapacitorIcon,
  TantalumCapacitorIcon,
  VariableCapacitorIcon,
  PolyesterCapacitorIcon,
  ConnectorIcon,
  CrystalIcon,
  DiodeIcon,
	EvaluationIcon,
  HardwareIcon,
  ICIcon,
  InductorIcon,
  KitIcon,
  LEDIcon,
  ModuleIcon,
  RelayIcon,
  ResistorIcon,
	PotentiometerIcon,
  SCRIcon,
  SensorIcon,
  SwitchIcon,
  TransformerIcon,
  TransistorIcon
} from "./icons";

/**
 * Get the part type id using its name
 * @param {string} partType Name of part type
 * @param {array} partTypes Array of part type objects
 */
export const getPartTypeId = (partType, partTypes) => {
  const item = _.find(partTypes, (i) => i.text === partType);
  if (item) return item.value;
  return null;
};

/**
 * Get icon of part type
 * @param {string} name Name of part type
 * @param {string} parentName Name of parent type
 * @returns
 */
export const getIcon = (name, parentName, iconSvg) => {
	if (iconSvg?.length > 0) {
		if (isSvg(iconSvg)) {
			return () => (<div className="MuiSvgIcon-root MuiSvgIcon-fontSizeMedium parttype parttype-Connector css-i4bv87-MuiSvgIcon-root" aria-hidden={true} dangerouslySetInnerHTML={{__html: iconSvg}}></div>);
		} else {
			// use the icon name
			name = iconSvg.replace("Icon", "");
		}
	}
  let icon = getIconForType(name);
  if (icon === null && parentName) {
    icon = getIconForType(parentName);
  }

  if (!icon) return ICIcon;
  return icon;
};

const getIconForType = (name) => {
  switch (name) {
    case "Cable":
      return CableIcon;
    case "Capacitor":
      return CapacitorIcon;
    case "CeramicCapacitor":
      return CeramicCapacitorIcon;
    case "MicaCapacitor":
      return MicaCapacitorIcon;
    case "PaperCapacitor":
      return PaperCapacitorIcon;
		case "SupercapacitorCapacitor":
			return SuperCapacitorIcon;
    case "MylarCapacitor":
		case "SafetyCapacitor":
      return MylarCapacitorIcon;
    case "TantalumCapacitor":
      return TantalumCapacitorIcon;
    case "VariableCapacitor":
      return VariableCapacitorIcon;
    case "PolyesterCapacitor":
		case "PolycarbonateCapacitor":
      return PolyesterCapacitorIcon;
    case "Connector":
      return ConnectorIcon;
    case "Crystal":
      return CrystalIcon;
    case "Diode":
      return DiodeIcon;
		case "Evaluation":
      return EvaluationIcon;
    case "Hardware":
      return HardwareIcon;
    case "IC":
    case "ICs":
      return ICIcon;
    case "Inductor":
		case "WirewoundResistor":
      return InductorIcon;
    case "Kit":
		case "ResistorKit":
		case "CapacitorKit":
		case "DiodeKit":
				return KitIcon;
    case "LED":
      return LEDIcon;
    case "Module":
      return ModuleIcon;
    case "Relay":
      return RelayIcon;
    case "Resistor":
      return ResistorIcon;
		case "Potentiometer":
			return PotentiometerIcon;
    case "MOSFET":
    case "TRIAC":
    case "SCR":
      return SCRIcon;
    case "Sensor":
      return SensorIcon;
    case "Switch":
      return SwitchIcon;
    case "Transformer":
      return TransformerIcon;
    case "Transistor":
      return TransistorIcon;
    default:
      return null;
  }
};
