import _ from "underscore";
import { CableIcon, CapacitorIcon, ConnectorIcon, CrystalIcon, DiodeIcon, HardwareIcon, ICIcon, InductorIcon, KitIcon, LEDIcon, ModuleIcon, RelayIcon, ResistorIcon, SCRIcon, SensorIcon, SwitchIcon, TransformerIcon, TransistorIcon } from "./icons";

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
export const getIcon = (name, parentName) => {
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
		case "Cables":
      return CableIcon;
    case "Capacitor":
		case "Capacitors":
      return CapacitorIcon;
		case "Connector":
		case "Connectors":
      return ConnectorIcon;
		case "Crystal":
		case "Crystals":
      return CrystalIcon;
    case "Diode":
		case "Diodes":
      return DiodeIcon;
		case "Hardware":
			return HardwareIcon;
    case "IC":
		case "ICs":
      return ICIcon;
		case "Inductor":
		case "Inductors":
      return InductorIcon;
		case "Kit":
		case "Kits":
			return KitIcon;
		case "LED":
		case "LEDs":
      return LEDIcon;
		case "Module":
		case "Modules":
      return ModuleIcon;
		case "Relay":
		case "Relays":
			return RelayIcon;
    case "Resistor":
		case "Resistors":
      return ResistorIcon;
		case "MOSFET":
		case "MOSFETs":
		case "TRIAC":
		case "SCR":
      return SCRIcon;
		case "Sensor":
		case "Sensors":
      return SensorIcon;
		case "Switch":
		case "Switches":
      return SwitchIcon;
		case "Transformer":
		case "Transformers":
      return TransformerIcon;
		case "Transistor":
		case "Transistors":
      return TransistorIcon;
    default:
      return null;
  }
};
