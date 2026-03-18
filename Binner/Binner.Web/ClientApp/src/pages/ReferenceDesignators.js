import { useState } from "react";
import { Segment, Table, Input } from "semantic-ui-react";

export function ReferenceDesignators() {
  const [search, setSearch] = useState("");

  const designators = [
    { designator: "A", partType: "Separable assembly or sub-assembly", alternates: "" },
    { designator: "AN", partType: "Antenna", alternates: "AS" },
    { designator: "AR", partType: "Amplifier", alternates: "" },
    { designator: "AT", partType: "Attenuator or isolator", alternates: "ATT" },
    { designator: "B", partType: "Blower", alternates: "" },
    { designator: "BR", partType: "Bridge rectifier", alternates: "D" },
    { designator: "BT", partType: "Battery or battery holder", alternates: "B, BAT" },
    { designator: "C", partType: "Capacitor", alternates: "" },
    { designator: "CB", partType: "Circuit breaker", alternates: "" },
    { designator: "CN", partType: "Capacitor network", alternates: "" },
    { designator: "CP", partType: "Coupler", alternates: "" },
    { designator: "D", partType: "Diode (all types, including LED), thyristor, varacter", alternates: "CR" },
    { designator: "DC", partType: "Directional coupler", alternates: "" },
    { designator: "DL", partType: "Delay line", alternates: "" },
    { designator: "DN", partType: "Diode network", alternates: "D" },
    { designator: "DP", partType: "Diplexer", alternates: "" },
    { designator: "DS", partType: "Display, general light source, lamp, signal light", alternates: "" },
    { designator: "E", partType: "Terminal", alternates: "" },
    { designator: "F", partType: "Fuse", alternates: "" },
    { designator: "FB", partType: "Ferrite bead", alternates: "L, E, FEB" },
    { designator: "FD", partType: "Fiducial", alternates: "" },
    { designator: "FL", partType: "Filter", alternates: "" },
    { designator: "G", partType: "Generator or oscillator", alternates: "OSC" },
    { designator: "GL", partType: "Graphical logo", alternates: "" },
    { designator: "GN", partType: "General network", alternates: "" },
    { designator: "H", partType: "Hardware, e.g., screws, nuts, washers, also used for drilled holes", alternates: "HW, MP" },
    { designator: "HY", partType: "Circulator or directional coupler", alternates: "" },
    { designator: "IR", partType: "Infrared diode", alternates: "D" },
    { designator: "J", partType: "Jack connector (least-movable connector of a connector pair)", alternates: "" },
    { designator: "JP", partType: "Jumper (link)", alternates: "" },
    { designator: "K", partType: "Relay or contactor", alternates: "RLA, RY" },
    { designator: "L", partType: "Inductor, coil, choke or ferrite bead", alternates: "" },
    { designator: "LD", partType: "LED", alternates: "D, LED" },
    { designator: "LS", partType: "Loudspeaker or buzzer", alternates: "SPK" },
    { designator: "M", partType: "Motor, meter or other measuring instrument", alternates: "" },
    { designator: "MH", partType: "Mounting hole", alternates: "" },
    { designator: "MK", partType: "Microphone", alternates: "MIC" },
    { designator: "MP", partType: "Mechanical part (including screws and fasteners)", alternates: "H, HW" },
    { designator: "Ne", partType: "Neon", alternates: "" },
    { designator: "OP", partType: "Operational amplifier (opamp) or Opto-isolator", alternates: "U" },
    { designator: "P", partType: "Plug connector", alternates: "" },
    { designator: "PS", partType: "Power supply", alternates: "" },
    { designator: "PD", partType: "Photodiode", alternates: "" },
    { designator: "Q", partType: "Transistor or transducer", alternates: "TR" },
    { designator: "R", partType: "Resistor or potentiometer", alternates: "" },
    { designator: "RC", partType: "Filter network (RC)", alternates: "" },
    { designator: "RT", partType: "Thermistor", alternates: "TH, R" },
    { designator: "RN", partType: "Resistor network", alternates: "R" },
    { designator: "RV", partType: "Varistor, variable resistor", alternates: "R" },
    { designator: "S", partType: "Switch (all types, including buttons)", alternates: "SW" },
    { designator: "SA", partType: "Spark arrester", alternates: "" },
    { designator: "T", partType: "Transistor or transducer", alternates: "Q, TR" },
    { designator: "TC", partType: "Thermocouple", alternates: "" },
    { designator: "TN", partType: "Tuner", alternates: "TUN" },
    { designator: "TP", partType: "Test point", alternates: "" },
    { designator: "TR", partType: "Transformer or transducer", alternates: "T, L, XMER" },
    { designator: "U", partType: "Integrated circuit (IC) or chip", alternates: "IC" },
    { designator: "V", partType: "Vacuum tube, electron tube, photoelectric cell", alternates: "" },
    { designator: "VR", partType: "Voltage regulator", alternates: "U, R" },
    { designator: "W", partType: "Wire, cable or busbar", alternates: "" },
    { designator: "X", partType: "Socket connector", alternates: "XV, XF, XA, XU, XDS" },
    { designator: "XTAL", partType: "Crystal, resonator or powered oscillator", alternates: "XTAL, X, Y" },
    { designator: "ZD", partType: "Zener diode", alternates: "D, DZ" },
  ];

  const handleChange = (e, control) => {
    setSearch(control.value);
  };

  let results = designators;
  if (search.length > 0) {
    if (search.length <= 2) {
      results = results.filter(d => d.designator.toLowerCase() === search.toLowerCase() 
        || d.alternates.toLowerCase().replaceAll(' ', '').split(',').includes(search.toLowerCase()));
    }
    if (search.length > 2 || results.length === 0) results = results.filter(d => d.designator.toLowerCase().includes(search.toLowerCase())
      || d.partType.toLowerCase().includes(search.toLowerCase())
      || d.alternates.toLowerCase().includes(search.toLowerCase()));
  }

  return (
    <Segment>
      <h1>Reference Designators</h1>
      <Input icon='search' name='search' placeholder='Search...' value={search} onChange={handleChange} />
      <div className="small">As per the <a href="https://www.asme.org/codes-standards/find-codes-standards/y14-44-reference-designations-electrical-electronics-parts-equipment" target="_blank" rel="noopener noreferrer">ASME Y14.44-2008</a> standard.</div>
      <Table>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>Reference Designator</Table.HeaderCell>
            <Table.HeaderCell>Part Type</Table.HeaderCell>
            <Table.HeaderCell>Alternates</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {results.map((d, key) => (
            <Table.Row key={key}>
              <Table.Cell>{d.designator}</Table.Cell>
              <Table.Cell>{d.partType}</Table.Cell>
              <Table.Cell>{d.alternates}</Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
    </Segment>
  );
};
