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
      return CableIcon;
    case "Capacitor":
      return CapacitorIcon;
		case "Connector":
      return ConnectorIcon;
		case "Crystal":
      return CrystalIcon;
    case "Diode":
      return DiodeIcon;
		case "Hardware":
			return HardwareIcon;
    case "IC":
      return ICIcon;
		case "Inductor":
      return InductorIcon;
		case "Kit":
			return KitIcon;
		case "LED":
      return LEDIcon;
		case "Module":
      return ModuleIcon;
		case "Relay":
			return RelayIcon;
    case "Resistor":
      return ResistorIcon;
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
