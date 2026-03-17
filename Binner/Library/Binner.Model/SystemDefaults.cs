namespace Binner.Model
{
    /// <summary>
    /// Global default data
    /// </summary>
    public static class SystemDefaults
    {
        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public const int LowStockThreshold = 1;

        /// <summary>
        /// Default part type definitions
        /// </summary>
        public enum DefaultPartTypes
        {
            [PartTypeInfo("A resistor is an electronic component that opposes the flow of electric current in a circuit.", "R", "Device:R", keywords: "res,resistors")]
            Resistor = 1,
            [PartTypeInfo("A capacitor is a passive electronic component that stores electrical energy in an electric field.", "C", "Device:C", keywords: "cap,capacitors")]
            Capacitor,
            [PartTypeInfo("An inductor, also known as a coil or choke, is a passive electronic component that stores energy in a magnetic field when electric current flows through it.", "L", "Device:L", keywords: "ind")]
            Inductor,
            [PartTypeInfo("A diode is a two-terminal electronic component that primarily allows current to flow in one direction, acting as a one-way switch.", "D", "Device:D")]
            Diode,
            [PartTypeInfo("A Light Emitting Diode (LED) is a semiconductor device that emits light when an electrical current passes through it.", "D", "Device:LED", keywords: "light emitting diode")]
            LED,
            [PartTypeInfo("A transistor is a fundamental semiconductor device used to amplify or switch electronic signals and power.", "Q", "Device:Q", keywords: "tran")]
            Transistor,
            [PartTypeInfo("A relay is an electrically operated switch. It uses an electromagnet to open or close electrical circuits in response to an input signal.", "K", "Relay")]
            Relay,
            [PartTypeInfo("A transformer is an electrical device that transfers electrical energy between circuits using electromagnetic induction.", "T", "Transformer")]
            Transformer,
            [PartTypeInfo("A crystal is a piezoelectric device, typically made of quartz, that acts as a frequency-determining component.", "Y", "Device:Crystal")]
            Crystal,
            [PartTypeInfo("A sensor is a device that detects a physical parameter and converts it into an electrical signal.", "U", "Sensor")]
            Sensor,
            [PartTypeInfo("A switch is a device that controls the flow of electrical current, switching a circuit on or off, typically using an electronic component rather than a mechanical one.", "SW", "Switch")]
            Switch,
            [PartTypeInfo("A cable is an assembly of one or more insulated wires, typically bundled together within a protective sheath, used to transmit electrical signals, power, or both between electronic devices.")]
            Cable,
            [PartTypeInfo("A connector is a device that facilitates electrical connections between different components, circuits, or systems, allowing for the transmission of power, signals, or data.", "J", "Connector", keywords: "conn")]
            Connector,
            [PartTypeInfo("An IC (Integrated Circuit), also known as a microchip or chip, is a miniature electronic circuit consisting of transistors, resistors, and capacitors fabricated on a single semiconductor chip, typically silicon.", "U", "Integrated Circuit")]
            IC,
            [PartTypeInfo("An electronics module is a self-contained, often small, unit that performs a specific electronic function, like power regulation or signal processing.", "U", "Module")]
            Module,
            [PartTypeInfo("An electronics evaluation kit is a pre-assembled package containing hardware (a printed circuit board with components) and software tools designed to help engineers and developers evaluate and experiment with specific integrated circuits or other electronic devices.", "U", "Evaluation", keywords: "evaluation board")]
            Evaluation,
            [PartTypeInfo("Hardware encapsulates different kinds of parts related to the physical assembly of a PCB. It may contain screws, stand-offs, bolts or nuts or any other type of physical object.")]
            Hardware,
            Other,
            [PartTypeInfo("An operational amplifier (op-amp) is a DC-coupled electronic voltage amplifier with a high gain, typically with a differential input and a single-ended output.", "U", "Amplifier_Operational", keywords: "ic opamp,opamp,operational amplifier,op-amp,op amp,operational amp")]
            [ParentPartType(DefaultPartTypes.IC)]
            OpAmp,
            [PartTypeInfo("An electronic amplifier is a device that increases the magnitude of a signal, whether it's voltage, current, or power.", "U", "Amplifier", keywords: "ic amp")]
            [ParentPartType(DefaultPartTypes.IC)]
            Amplifier,
            [PartTypeInfo("Electronic memory is a storage unit for recording data, primarily used in computers and other electronic devices.", "U", "Memory", keywords: "ic mem,ic eeprom,ic fifo,ic sram,cmos eeprom,bus eeprom,eeprom serial,ic fram,ic prom,ic ram,ic cbram,ic eprom")]
            [ParentPartType(DefaultPartTypes.IC)]
            MemoryIc,
            [PartTypeInfo("A logic IC, or integrated circuit, is a small chip containing a set of interconnected electronic components, primarily transistors, that perform logical operations on digital signals.", "U", "Logic", keywords: "ic gate")]
            [ParentPartType(DefaultPartTypes.IC)]
            LogicIc,
            [PartTypeInfo("An electronics interface IC is an integrated circuit that facilitates communication and data exchange between different electronic devices or systems.", "U", "Interface", keywords: "ic uart,ic async comm,ic duart,ic switch,ic mux,ic xpndr,ic xpnd,ic redriver,ic acceleratr,ic accelerator,ic filter,ic hart modem,ic interface,ic intface,ic transceiver")]
            [ParentPartType(DefaultPartTypes.IC)]
            InterfaceIc,
            [PartTypeInfo("A microcontroller is a small, self-contained computer on a single integratfed circuit (IC) designed to control the functions of an embedded system.", "U", "Interface", keywords: "esp32,attiny,pic,stm32,atmega,rp2040,raspberry pi,msp430,arduino,teensy,beaglebone")]
            [ParentPartType(DefaultPartTypes.IC)]
            Microcontroller,
            [PartTypeInfo("A clock IC, or integrated circuit, is a specialized electronic component that generates, manipulates, and distributes precise timing signals within electronic systems.", "U", "Timer", keywords: "ic clk,ic clock,ic clk buffer")]
            [ParentPartType(DefaultPartTypes.IC)]
            ClockIc,
            [PartTypeInfo("An Analog-to-Digital Converter (ADC) IC is a type of integrated circuit (IC) that transforms a continuous analog signal, such as a voltage or current, into a digital representation.", "U", "Analog_ADC", keywords: "ic adc")]
            [ParentPartType(DefaultPartTypes.IC)]
            ADC,
            [PartTypeInfo("A voltage regulator IC (Integrated Circuit) is an electronic component designed to maintain a stable and consistent output voltage, regardless of changes in input voltage or load conditions.", "U", "Regulator_Linear", keywords: "ic reg linear,ic reg lin")]
            [ParentPartType(DefaultPartTypes.IC)]
            VoltageRegulatorIc,
            [PartTypeInfo("Energy metering integrated circuits (ICs) are electronic components designed to measure electrical energy consumption in various power systems, including single, dual, and three-phase setups.", "U", "Sensor_Energy")]
            [ParentPartType(DefaultPartTypes.IC)]
            EnergyMeteringIc,
            [PartTypeInfo("An LED driver IC is a specialized integrated circuit that provides the necessary voltage and current regulation to power and control the brightness of LEDs.", "U", "Driver_LED", keywords: "")]
            [ParentPartType(DefaultPartTypes.IC)]
            LedDriverIc,
            [PartTypeInfo("An audio driver IC, or integrated circuit, is an electronic component that amplifies weak audio signals to a level strong enough to drive speakers or headphones.", "U", "Audio", keywords: "ic compandor,ic sample rate,ic audio,ic line driver,ic volume,if signal processor,ic detection switch,audio level sensor,compander ic,audio signal processor,surround processor,ic electronic volume,btsc decoder,ic long duration,ic demodulator,tone control,electronic volume,tone sound controller,tone control circuit,audio demodulator,ic v-b audio,consumer circuit")]
            [ParentPartType(DefaultPartTypes.IC)]
            AudioIc,
            [PartTypeInfo("A comparator IC is a type of integrated circuit (IC) that compares two analog voltage or current signals and produces a digital output indicating which signal is larger.", "U", "Comparator", keywords: "ic comparator,ic id comparator,ic mag comparator,ic addr comparator,ic comparator address,magnitude comparator,identity comparator,address comparator")]
            [ParentPartType(DefaultPartTypes.IC)]
            ComparatorIc,
            [PartTypeInfo("A counter IC is a digital integrated circuit that increments or decrements a value based on a clock signal or external events.", "U", "Counter", keywords: "ic binary counter,ic counter,ic bcd counter")]
            [ParentPartType(DefaultPartTypes.IC)]
            CounterIc,
            [PartTypeInfo("An electronics divider IC is a digital logic integrated circuit that divides an input signal's frequency or count by a specific ratio.", "U", "Divider", keywords: "ic divider")]
            [ParentPartType(DefaultPartTypes.IC)]
            DividerIc,
            [PartTypeInfo("A Power Management Integrated Circuit (PMIC) is a specialized electronic chip that manages the power requirements of a system or device.", "U", "Battery_Management", keywords: "ic pmic")]
            [ParentPartType(DefaultPartTypes.IC)]
            PMIC,
            [PartTypeInfo("A Field-Programmable Gate Array (FPGA) is a versatile type of integrated circuit (IC) that allows you to implement custom digital logic circuits.", "U", "FPGA", keywords: "ic fpga")]
            [ParentPartType(DefaultPartTypes.IC)]
            FPGA,
            [PartTypeInfo("A Data Acquisition (DAQ) IC is a small integrated circuit that measures analog electrical signals and converts them into digital form.", "U", "DAQ")]
            [ParentPartType(DefaultPartTypes.IC)]
            DataAcquisitionIc,
            [PartTypeInfo("Embedded systems are specialized ICs for performing dedicated functions via software within a larger system.", "U", "Embedded")]
            [ParentPartType(DefaultPartTypes.IC)]
            EmbeddedIc,
            [ParentPartType(DefaultPartTypes.IC)]
            SpecializedIc,
            [ParentPartType(DefaultPartTypes.Capacitor)]
            [PartTypeInfo("A ceramic capacitor is a fixed-value capacitor where the ceramic material acts as the dielectric, separating two conductive plates.", "C", "Device:C", keywords: "cap cer")]
            CeramicCapacitor,
            [PartTypeInfo("An electrolytic capacitor is a type of capacitor that uses an electrolyte to increase its capacitance and stores electrical energy.", "C", "Device:C", keywords: "cap alum")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            ElectrolyticCapacitor,
            [PartTypeInfo("Film capacitors are a type of capacitor that utilizes a thin plastic film as its dielectric material, separating two conductive plates.", "C", "Device:C", keywords: "cap film")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            FilmCapacitor,
            [PartTypeInfo("Mica capacitors are electronic components that utilize sheets of mica as the dielectric material between conductive plates.", "C", "Device:C", keywords: "cap mica")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            MicaCapacitor,
            [PartTypeInfo("Non-polarized capacitors are electronic components that do not have a designated positive or negative terminal and can be connected in either direction in a circuit without causing damage.", "C", "Device:C")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            NonPolarizedCapacitor,
            [PartTypeInfo("A supercapacitor, also known as an ultracapacitor, is an energy storage device that bridges the gap between traditional capacitors and batteries.", "C", "Device:C", keywords: "edlc")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            SupercapacitorCapacitor,
            [PartTypeInfo("A paper capacitor is a type of fixed capacitor that utilizes paper as its dielectric material, the insulator between conductive plates.", "C", "Device:C")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            PaperCapacitor,
            [PartTypeInfo("A variable capacitor is a capacitor whose capacitance can be changed, usually by physically adjusting the gap or overlap between its plates.", "C", "Device:C", keywords: "cap trimmer,cap glass trimmer,cap film trim,cap cer trim,cap variable,cap var,cap trim")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            VariableCapacitor,
            [PartTypeInfo("A carbon film resistor is a type of fixed resistor where a thin carbon film is deposited on a ceramic substrate to create the resistive element.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            CarbonFilmResistor,
            [PartTypeInfo("A metal film resistor is an electronic component that uses a thin film of metal deposited on a ceramic core to create resistance.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            MetalFilmResistor,
            [PartTypeInfo("A wirewound resistor is a passive electronic component that restricts current flow in a circuit by utilizing a length of resistive wire wound around a core.", "R", "Device:R", keywords: "res chas")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            WirewoundResistor,
            [PartTypeInfo("A metal oxide film resistor is a type of fixed resistor that utilizes a thin film of metal oxide, typically tin oxide, coated on a ceramic rod.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            MetalOxideResistor,
            [PartTypeInfo("Metal strip resistors are electronic components that provide resistance, often used for current sensing and power applications.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            MetalStripResistor,
            [PartTypeInfo("A power resistor is a type of resistor designed to dissipate a larger amount of electrical power as heat, typically exceeding 1 watt.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            PowerResistor,
            [PartTypeInfo("A resistor array, also known as a resistor network, is a single electronic component that contains two or more resistors in a specific configuration, typically arranged in a single package.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            ResistorArray,
            [PartTypeInfo("A variable resistor is an electronic component that allows for adjusting the amount of electrical resistance within a circuit.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            VariableResistor,
            [PartTypeInfo("An air core inductor is a type of inductor that doesn't use a magnetic core material like ferrite or iron, instead relying on only air or a non-magnetic material for its core.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            AirCoreInductor,
            [PartTypeInfo("An iron core inductor is a type of inductor that utilizes a magnetic core, typically made of iron or ferrite, to enhance the magnetic field created by a coil of wire.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            IronCoreInductor,
            [PartTypeInfo("A ferrite core inductor is an inductor (a component that stores energy in a magnetic field) that uses a core made of ferrite, a ceramic material, to enhance its magnetic properties.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            FerriteCoreInductor,
            [PartTypeInfo("An iron powder core inductor is a type of inductor that uses a core made from iron particles, typically bound together with a resin or binder, to increase its inductance.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            IronPowderInductor,
            [PartTypeInfo("A laminated core inductor is an inductor with a core made of thin, stacked steel sheets (laminations).", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            LaminatedCoreInductor,
            [PartTypeInfo("A bobbin inductor, also known as a drum core inductor or tubular inductor, is an electronic component that stores energy in a magnetic field when current flows through it.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            BobbinInductor,
            [PartTypeInfo("A toroidal inductor is an electronic component, specifically an inductor, that utilizes a donut-shaped (toroidal) magnetic core.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            ToroidalInductor,
            [PartTypeInfo("A multilayer ceramic inductor (MLCI) is a small, high-frequency inductor typically used in surface mount technology (SMT) applications.", "L", "Device:L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            MultiLayerCeramicInductor,
            [PartTypeInfo("A Zener diode is a specialized semiconductor diode designed to operate reliably in reverse-bias mode, allowing current to flow in the reverse direction when a specific voltage threshold (the Zener voltage) is reached.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            Zener,
            [PartTypeInfo("A Schottky diode is a type of semiconductor diode characterized by a metal-semiconductor junction, leading to a lower forward voltage drop and faster switching speeds compared to conventional PN junction diodes.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            Schottky,
            [PartTypeInfo("A small signal diode is a type of PN junction diode designed to handle low-voltage, low-current signals and operate efficiently at higher frequencies.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            SmallSignalDiode,
            [PartTypeInfo("A large signal diode refers to a diode's behavior when subjected to significant changes in voltage or current, unlike \"small signal\" diodes which are used for processing small signals or high frequencies.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            LargeSignalDiode,
            [PartTypeInfo("A Shockley diode, also known as a PNPN diode, is a four-layer semiconductor device that behaves like a pair of interconnected NPN and PNP transistors.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            Shockley,
            [PartTypeInfo("A Peltier diode, also known as a thermoelectric module (TEC), is a solid-state heat pump that transfers heat from one side to the other when a DC voltage is applied.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            PeltierDiode,
            [PartTypeInfo("A Gunn diode, also known as a Transferred Electron Device (TED), is a two-terminal semiconductor device that generates microwave frequencies.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            GunnDiode,
            [PartTypeInfo("A tunnel diode is a heavily doped, fast-switching semiconductor diode exhibiting negative resistance.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            TunnelDiode,
            [PartTypeInfo("A Step Recovery Diode (SRD) is a specialized semiconductor diode designed to generate very short, high-speed pulses.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            StepRecoveryDiode,
            [PartTypeInfo("A varactor diode, also known as a varicap or voltage-variable capacitance diode, is a type of diode designed to exploit the voltage-dependent capacitance of a reverse-biased p-n junction.", "D", "Device:D")]
            [ParentPartType(DefaultPartTypes.Diode)]
            VaractorDiode,
            [PartTypeInfo("A Transient Voltage Suppressor (TVS) diode is an electronic component designed to protect sensitive circuits from voltage spikes and surges.", "D", "Device:D", "tvs transient voltage suppressor")]
            [ParentPartType(DefaultPartTypes.Diode)]
            TransientVoltageSuppressionDiode,
            [PartTypeInfo("A crystal diode, also known as a semiconductor diode, is a two-terminal electronic device that allows current to flow in one direction and blocks it in the opposite direction.", "D", "Device:D", "semiconductor diode germanium silicon")]
            [ParentPartType(DefaultPartTypes.Diode)]
            CrystalDiode,
            [PartTypeInfo("A MOSFET (Metal-Oxide-Semiconductor Field-Effect Transistor) is a type of field-effect transistor that controls the flow of current using an electric field.", "Q", "Transistor_FET")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            MOSFET,
            [PartTypeInfo("An Insulated-Gate Bipolar Transistor (IGBT) is a three-terminal power semiconductor device, primarily used as an electronic switch, that combines the high efficiency and fast switching capabilities of a MOSFET with the high current handling and voltage blocking ability of a BJT.", "Q", "Transistor_IGBT")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            IGBT,
            [PartTypeInfo("A Junction Field-Effect Transistor (JFET) is a voltage-controlled semiconductor device that regulates the flow of current between its source and drain terminals.", "Q", "Transistor_FET")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            JFET,
            [PartTypeInfo("A Silicon Controlled Rectifier (SCR), also known as a thyristor, is a three-terminal semiconductor device used to control current flow in a specific direction.", "Q", "Transistor")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            SCR,
            [PartTypeInfo("A Diac (Diode for Alternating Current) is a bidirectional semiconductor switch that conducts current in both directions when the applied voltage exceeds its breakover voltage.", "Q", "Device:DIAC")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            DIAC,
            [PartTypeInfo("A TRIAC (Triode for Alternating Current) is a three-terminal semiconductor device used to control the flow of alternating current.", "Q", "Transistor")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            TRIAC,
            [PartTypeInfo("An electromagnetic relay is an electronically operated switch that uses an electromagnet to control the opening and closing of a circuit.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            ElectromagneticRelay,
            [PartTypeInfo("A latching relay is an electromechanical switch that remains in its last switched position (ON or OFF) even after the control power is removed.", "K", "Relay", keywords: "latch relay,latching relay,relay latch")]
            [ParentPartType(DefaultPartTypes.Relay)]
            LatchingRelay,
            [PartTypeInfo("A solid-state relay (SSR) is an electronic switching device that controls a load (like a light bulb or a motor) using a low-power control signal.", "K", "Relay", keywords: "ssr relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            SolidStateRelay,
            [PartTypeInfo("A reed relay is an electromagnetic switching device that uses one or more reed switches to open or close an electrical circuit.", "K", "Relay", keywords: "relay reed")]
            [ParentPartType(DefaultPartTypes.Relay)]
            ReedRelay,
            [PartTypeInfo("A time relay is an electronic component that introduces a time delay into an electrical circuit before switching it on or off.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            TimeRelay,
            [PartTypeInfo("A thermal relay is an electrical protection device that detects excessive heat buildup in a circuit, typically due to overload or short circuits, and triggers a response, such as disconnecting the power supply or sending an alarm.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            ThermalRelay,
            [PartTypeInfo("A sequencing relay is an electronic device that controls the order in which multiple electrical devices or systems are activated.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            SequenceRelay,
            [PartTypeInfo("A rotary relay is a type of relay where the contact arm can rotate through 360 degrees, though not in one continuous movement.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            RotaryRelay,
            [PartTypeInfo("A high voltage (HV) relay is an electromechanical device designed to switch high voltage and current signals, typically above 1kV and up to 10,000V.", "K", "Relay")]
            [ParentPartType(DefaultPartTypes.Relay)]
            HighVoltageRelay,
            [PartTypeInfo("A step-down transformer is an electronic device that reduces AC voltage.", "T", "Transformer")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            StepDownTransformer,
            [PartTypeInfo("A step-up transformer is an electrical device that increases the voltage of an alternating current (AC) electrical power supply.", "T", "Transformer")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            StepUpTransformer,
            [PartTypeInfo("An isolation transformer is a type of transformer that electrically separates the input and output circuits while still allowing AC power to be transferred.", "T", "Transformer")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            IsolationTransformer,
            [PartTypeInfo("A solid-state transformer (SST) is a power electronics device that uses semiconductor components and control circuitry, instead of traditional transformers, to convert and manage electrical power.", "T", "Transformer", "SST")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            SolidStateTransformer,
            [PartTypeInfo("An RF transformer is a passive electronic component that transfers energy between circuits using electromagnetic induction, typically for radio frequency (RF) signals.", "T", "Transformer", "high frequency")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            RfTransformer,
            [PartTypeInfo("An audio transformer is an electronic component that uses magnetic induction to transfer and modify audio signals.", "T", "Transformer")]
            [ParentPartType(DefaultPartTypes.Transformer)]
            AudioTransformer,
            [ParentPartType(DefaultPartTypes.Sensor)]
            SensorAssembly,
            [PartTypeInfo("A light sensor, also known as a photodetector or photocell, is an electronic device that detects the intensity of light and converts it into an electrical signal.", "U", "Sensor_Optical")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            LightSensor,
            [PartTypeInfo("An electronic imaging sensor, or imager, is a device that converts light into electrical signals, forming the basis of digital images.", "U", "Sensor_Optical")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            ImagingSensor,
            [PartTypeInfo("A current sensor is a device that measures the electric current flowing through a conductor and generates an output signal proportional to that current.", "U", "Sensor_Current")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            CurrentSensor,
            [PartTypeInfo("A voltage sensor is an electronic device that detects and measures the voltage (the difference in electrical potential) between two points in a circuit.", "U", "Sensor_Voltage")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            VoltageSensor,
            [PartTypeInfo(" load sensor, also known as a force sensor or load cell, is an electronic device that converts a mechanical force, like weight or pressure, into an electrical signal.", "U", "Sensor_Touch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            LoadSensor,
            [PartTypeInfo("A distance sensor is an electronic device that measures the distance between itself and an object or surface without physical contact.", "U", "Sensor_Distance", keywords: "ranging sensor,sensor dist,sensor distance,range sensor,tof")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            DistanceSensor,
            [PartTypeInfo("A force sensor is an electronic device that detects and measures the amount of force applied to it, typically converting mechanical force into an electrical signal.", "U", "Sensor_Touch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            ForceSensor,
            [PartTypeInfo("An RF (Radio Frequency) sensor is an electronic device that detects and measures radio frequency signals, often used to monitor the strength or properties of RF signals for various applications.", "U", "Sensor_Energy")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            RfSensor,
            [PartTypeInfo("A motion sensor is an electronic device that detects movement.", "U", "Sensor_Motion")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            MotionSensor,
            [PartTypeInfo("A capacitive sensor is an electronic device that detects the presence or absence of an object by sensing changes in capacitance.", "U", "Sensor_Touch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            CapacitiveSensor,
            [PartTypeInfo("A biometric sensor is a device that converts a unique biological characteristic (like a fingerprint or facial features) into an electrical signal.", "U", "Sensor_Touch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            BiometricSensor,
            [PartTypeInfo("Environmental sensors are electronic devices that detect and measure various physical and chemical properties of the surrounding environment.", "U", "Sensor_Temperature")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            EnvironmentSensor,
            [PartTypeInfo("An electronic radiation sensor is a device designed to detect and measure various types of radiation, converting the radiation's energy into an electrical signal that can be measured.", "U", "Sensor_Energy")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            RadiationSensor,
            [PartTypeInfo("No description.", "U", "Sensor_Energy")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            OtherSensor,
            [ParentPartType(DefaultPartTypes.Module)]
            WirelessModule,
            [ParentPartType(DefaultPartTypes.Module)]
            CurrentVoltageModule,
            [ParentPartType(DefaultPartTypes.Module)]
            ExperimentModule,
            [ParentPartType(DefaultPartTypes.Module)]
            RaspberryPiShield,
            [ParentPartType(DefaultPartTypes.Module)]
            ArduinoShield,
            [ParentPartType(DefaultPartTypes.Module)]
            EvaluationModule,
            [ParentPartType(DefaultPartTypes.Module)]
            OtherModule,
            [PartTypeInfo("A Raspberry Pi is a low-cost, credit-card sized computer that acts as a single-board computer (SBC). It's designed to be versatile, allowing users to connect it to a monitor, keyboard, and mouse, and then use it for a wide range of tasks, from web browsing and video playback to programming and creating various digital projects", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            RaspberryPi,
            [PartTypeInfo("Arduino is an open-source platform used for building electronic projects. It consists of a physical programmable circuit board (a microcontroller) and a software IDE that runs on a computer", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Arduino,
            [PartTypeInfo("The BeagleBoard is a low-cost, open-source single-board computer (SBC) designed for development and prototyping. It's based on Texas Instruments processors, offering a platform for building embedded systems and exploring open-source hardware and software", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            BeagleBoard,
            [PartTypeInfo("Nvidia Jetson is a series of embedded computing boards from Nvidia. The Jetson TK1, TX1 and TX2 models all carry a Tegra processor (or SoC) from Nvidia that integrates an ARM architecture central processing unit (CPU). Jetson is a low-power system and is designed for accelerating machine learning applications.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            NVidiaJetson,
            [PartTypeInfo("Teensy is a family of compact, USB-based microcontroller development boards known for their versatility and ease of use. They are often used for prototyping, audio projects, and other applications where a powerful, small, and versatile microcontroller is needed", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Teensy,
            [PartTypeInfo("A LaunchPad, in the context of electronics, refers to a low-cost development board from Texas Instruments, specifically designed for embedded programming", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Launchpad,
            [PartTypeInfo("Alchitry is a brand of Field-Programmable Gate Array (FPGA) development boards designed by SparkFun to make FPGA development more accessible for beginners and hobbyists", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Alchitry,
            [PartTypeInfo("The NodeMCU Amica board is a popular development board based on the ESP8266 Wi-Fi microcontroller, often used for Internet of Things (IoT) projects", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Amica,
            [PartTypeInfo("Particle MCUs are microcontroller units, essentially small, self-contained computer chips, used in Internet of Things (IoT) devices. Particle offers various MCUs like the Photon, Electron, and Boron, each with different features and capabilities for connecting devices to the internet.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Particle,
            [PartTypeInfo("Sparkfun development boards", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Sparkfun,
            [PartTypeInfo("Qwiic is a standardized, solder-free, plug-and-play connectivity system developed by SparkFun Electronics. It uses 4-pin JST SH connectors and cables to simplify interfacing sensors, displays, and other components with microcontrollers and other devices. Qwiic leverages the I2C communication protocol, making it suitable for a wide range of applications, especially in robotics, home automation, and scientific research.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Qwiic,
            [PartTypeInfo("PIC (Programmable Interface Controller) microcontrollers are small, programmable chips, essentially miniature computers on a single chip, designed to control specific tasks within an embedded system", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Pic,
            [PartTypeInfo("The STM32 is a popular family of 32-bit microcontrollers (MCUs) and microprocessors (MPUs) developed by STMicroelectronics. They are based on the ARM Cortex-M core, offering a wide range of performance and features for various applications.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            STM32,
            [PartTypeInfo("MikroElektronika (MIKROE), often abbreviated as MIKROE, is a Serbian company known for its development tools and compilers for various microcontroller families.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            MikroElektronika,
            [PartTypeInfo("The BASIC Stamp is a small, self-contained microcontroller board designed to be easily programmed, especially for beginners in electronics and programming. It's named \"Stamp\" due to its compact size, similar to a postage stamp.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            BasicEvaluation,
            [PartTypeInfo("Pinecone EVB is an open-source development board designed specially for IoT-based applications utilizing Wifi and BLE. It is can be used for prototyping and building IoT products.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Pine,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Odriod,
            [PartTypeInfo("LattePanda is a compact, pocket-sized single-board computer (SBC) that runs full Windows or Linux operating systems. It is designed to be a powerful and versatile alternative to the Raspberry Pi, particularly for projects requiring a more powerful Intel processor and compatibility with Windows.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            LattePanda,
            [PartTypeInfo("Seeeduino boards are Arduino-compatible microcontroller boards designed and produced by Seeed Studio.", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Seeeduino,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            SiliconLabs,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            RockPi,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            Udoo,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            PocketBeagle,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            AsusTinker,
            [PartTypeInfo("", "A", "MCU_Module")]
            [ParentPartType(DefaultPartTypes.Evaluation)]
            OtherEvaluation,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Adapter,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Screw,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Washer,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Nut,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Standoff,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Gear,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Coupler,
            [ParentPartType(DefaultPartTypes.Hardware)]
            BallBearing,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Bracket,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Shaft,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Spacer,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Tube,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Plate,
            [ParentPartType(DefaultPartTypes.Hardware)]
            RawMaterial,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Mount,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Belt,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Hub,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Fan,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Wheel,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Robotics,

            // added out of order (don't change order or keys will change)
            [PartTypeInfo("A flip-flop, also known as a bistable multivibrator, is a digital electronic circuit with two stable states, representing 0 or 1, that can be used to store a single bit of information.", "U", null, keywords: "ic ff,flipflop,flip flop,flip-flop")]
            [ParentPartType(DefaultPartTypes.IC)]
            FlipFlopIc,
            [PartTypeInfo("An adjustable inductor is a type of inductor with a movable core, allowing its inductance to be changed by adjusting the position of the core within the coil.", "L")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            AdjustableInductor,
            [PartTypeInfo("An electronic pressure sensor is a device that detects and measures pressure and converts it into an electrical signal.", "U", "Sensor_Pressure", keywords: "sensor pressure")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            PressureSensor,
            [PartTypeInfo("A photodiode is a semiconductor device that converts light energy into electrical current.", "U", "Sensor_Optical", keywords: "sensor reflective,photo sen,sensor reflctv,sensor transmissive,sensor retroreflective,sensor retrorefl,sensor through-beam,sen laser,sen photo,sensor photodiode")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            Photodiodes,
            [PartTypeInfo("A color sensor is an electronic device that measures the color of an object by detecting the amount of red, green, and blue (RGB) light reflected or emitted by that object.", "U", "Sensor_Optical", keywords: "color sensor,color sen")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            ColorSensor,
            [PartTypeInfo("A humidity sensor is an electronic device that detects and measures the amount of water vapor (humidity) in the air.", "U", "Sensor_Humidity", keywords: "sensor humidity,humidity,sensor humid/temp,sensor humi/temp")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            HumiditySensor,
            [PartTypeInfo("A gas sensor is a device that detects and measures the presence and concentration of various gases. It works by converting the gas concentration into an electrical signal, which can then be interpreted to determine the specific gas and its level.", "U", "Sensor_Gas", keywords: "sensor gas")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            GasSensor,
            [PartTypeInfo("An electronic flow sensor is a device that uses electronic components to measure the flow rate of liquids or gases.", "U", "Sensor_Gas", keywords: "sensor flow")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            FlowSensor,
            [PartTypeInfo("A tilt sensor, also known as an inclinometer or tilt switch, is a device that measures the angle or tilt of an object relative to a reference plane, usually gravity.", "U", "Sensor_Motion", keywords: "sensor tilt,tilt switch,sensor rolling,tilt sensor,inclination sensor")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            TiltSensor,
            [PartTypeInfo("A proximity sensor is an electronic device that can detect the presence or absence of an object without making physical contact.", "U", "Sensor_Proximity", keywords: "sensor prox")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            ProximitySensor,
            [PartTypeInfo("A temperature sensor is an electronic device that measures and detects changes in temperature, converting those changes into an electrical signal for further processing or monitoring.", "U", "Sensor_Temperature", keywords: "sensor temp,sensor temperature")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            TemperatureSensor,
            [PartTypeInfo("A touch sensor is an electronic device that detects physical touch or pressure, often used to provide input to electronic devices.", "U", "Sensor_Touch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            TouchSensor,
            [PartTypeInfo("An ultrasonic sensor is a device that uses sound waves higher than the human hearing range (above 20 kHz) to detect objects and measure distances.", "U", "Sensor_Distance")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            UltrasonicSensor,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Spring,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Grommet,
            [PartTypeInfo("A Bipolar Junction Transistor (BJT) is a three-terminal semiconductor device used to amplify or switch electronic signals.", "Q", "Transistor_BJT")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            BJT,
            [PartTypeInfo("A sound sensor, or audio sensor, is a device that detects sound waves and converts them into electrical signals, essentially acting as a digital ear.", "U", "Sensor_Audio")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            AudioSensor,
            [PartTypeInfo("A liquid level sensor is a device that detects and measures the level of liquid within a container.", "U", "Sensor_Gas")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            LiquidSensor,
            [PartTypeInfo("A magnetic sensor is a device that detects and measures a magnetic field, converting it into an electrical signal.", "U", "Sensor_Magnetic", keywords: "sensor mag,mag switch,magnetic switch")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            MagneticSensor,
            [PartTypeInfo("A Hall effect sensor is a device that measures the strength and direction of a magnetic field, using the Hall effect, which is the phenomenon of a voltage difference across a conductor carrying current when subjected to a magnetic field.", "U", "Sensor_Magnetic", keywords: "sensor hall,sen hall")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            HallEffectSensor,
            [PartTypeInfo("A smoke sensor, or smoke detector, is an electronic device designed to detect smoke, typically as an indicator of fire. It works by detecting changes in light or electrical current caused by smoke particles entering a sensing chamber. There are two primary types: ionization and photoelectric.", "U", "Sensor_Gas")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            SmokeSensor,
            [PartTypeInfo("An air quality sensor is an electronic device designed to detect and measure the concentration of pollutants and other contaminants in the air.", "U", "Sensor_Gas")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            AirQualitySensor,
            [PartTypeInfo("An acceleration sensor, or accelerometer, is an electronic device that measures acceleration, which is the rate of change of velocity.", "U", "Sensor_Motion", keywords: "sensor accel,three axis accelerometer,3-axis accelerometer,accelerometer,accel,2 axes digital inclinometer,ic gyroscope,sensor gyroscope,gyroscope")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            AccelerationSensor,
            [PartTypeInfo("A position sensor is an electronic device that measures the location or displacement of an object. It can provide information about the object's position, speed, direction, or distance.", "U", "Sensor_Motion", keywords: "encoder,encoder mech,encoder increm")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            PositionSensor,
            [PartTypeInfo("A gyroscope is a motion sensor that detects and measures angular velocity (rotation rate). It's a more advanced sensor than an accelerometer, as it measures the tilt and lateral orientation of an object, while accelerometers only measure linear motion.", "U", "Sensor_Motion")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            GyroscopeSensor,
            [PartTypeInfo("An electronics incline sensor, also known as an inclinometer or tilt sensor, is a device that measures the angle of an object relative to gravity.", "U", "Sensor_Tilt")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            InclineSensor,
            [PartTypeInfo("A speed sensor is a device that measures and reports the speed of a moving object or component, typically rotational speed.", "U", "Sensor_Speed")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            SpeedSensor,
            [PartTypeInfo("A vibration sensor is an electronic device that measures vibrations, typically by converting mechanical motion into an electrical signal.", "U", "Sensor_Motion", keywords: "sensor vibration,sensor vib,piezo sensor")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            VibrationSensor,
            [PartTypeInfo("An infrared (IR) sensor is an electronic device that detects infrared radiation, a type of electromagnetic radiation with wavelengths longer than visible light.", "U", "Sensor_Optical")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            InfraredSensor,
            [ParentPartType(DefaultPartTypes.Hardware)]
            Enclosure,
            [PartTypeInfo("Ceramic resistors are passive electronic components that offer resistance to electrical current, utilizing a core material made from a blend of ceramic and metal oxide elements.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            CeramicResistor,
            [PartTypeInfo("A current sense resistor is a low-value resistor specifically designed to measure the current flowing through it.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            CurrentSenseResistor,
            [PartTypeInfo("High-frequency resistors are specialized electronic components designed to function effectively at radio frequencies (RF) and microwave frequencies, typically above 1 GHz.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            HighFrequencyResistor,
            [PartTypeInfo("A metal foil resistor is an electronic component that uses a thin sheet of metal foil as its resistive element, attached to a ceramic substrate, to provide electrical resistance.", "R", "Device:R")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            MetalFoilResistor,
            [PartTypeInfo("A kit containing multiple resistor values.", "A", "Kit", keywords: "resistor kit,res kit")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            ResistorKit,
            [PartTypeInfo("A potentiometer (often called a pot or pot meter) is a three-terminal variable resistor used to adjust the voltage at a specific point in a circuit.", "R", "Device:R", keywords: "pot,pot meter")]
            [ParentPartType(DefaultPartTypes.Resistor)]
            Potentiometer,
            [PartTypeInfo("Safety capacitors are specialized electronic components designed to protect equipment and users from dangerous electrical surges, voltage transients, and electromagnetic interference (EMI).", "C", "Device:C", "")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            SafetyCapacitor,
            [PartTypeInfo("A tantalum capacitor is a type of electrolytic capacitor that uses tantalum as its anode material. It's known for its high capacitance per volume and low weight, making it suitable for applications where space and weight are critical.", "C", "Device:C", keywords: "cap tant")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            TantalumCapacitor,
            [PartTypeInfo("A general kit not otherwise classified.", "A", "Kit", keywords: "parts kit, sensor kit")]
            Kit,
            [PartTypeInfo("A kit containing multiple capactior values.", "A", "Kit", keywords: "capacitor kit,cap kit")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            CapacitorKit,
            [PartTypeInfo("A kit containing multiple diode values.", "A", "Kit", keywords: "diode kit")]
            [ParentPartType(DefaultPartTypes.Diode)]
            DiodeKit,
            [PartTypeInfo("A kit containing multiple inductor values.", "A", "Kit", keywords: "inductor kit")]
            [ParentPartType(DefaultPartTypes.Inductor)]
            InductorKit,
            [PartTypeInfo("A capacitor array, also known as a capacitor network, is a single electronic component that contains two or more capacitors in a specific configuration, typically arranged in a single package.", "C", "Device:C", keywords: "capacitor array,cap array")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            CapacitorArray,
            [PartTypeInfo("A diode array, also known as a diode network, is a single electronic component that contains two or more diodes in a specific configuration, typically arranged in a single package.", "D", "Device:D", keywords:"diode array")]
            [ParentPartType(DefaultPartTypes.Diode)]
            DiodeArray,
            [PartTypeInfo("An led array, also known as a led network, is a single electronic component that contains two or more LEDs in a specific configuration, typically arranged in a single package.", "D", "Device:D", keywords: "led array")]
            [ParentPartType(DefaultPartTypes.LED)]
            LEDArray,
            [PartTypeInfo("A transistor array, also known as a transistor network, is a single electronic component that contains two or more transistors in a specific configuration, typically arranged in a single package.", "Q", "Device:Q", keywords:"transistor array,tran array,trans array")]
            [ParentPartType(DefaultPartTypes.Transistor)]
            TransistorArray,
            [PartTypeInfo("Development tools aid with the design and development of electronic products.", "A", "Development_Tools", keywords: "development tool,development tools")]
            DevelopmentTool,
            [PartTypeInfo("A debugger/programmer is a hardware tool for connecting a PC to a microcontroller to control code execution, set breakpoints, inspect memory/registers or uploading code.", "A", "Development_Tools", keywords: "debugging,debug tool,debugger tool,jtag,programmer,emulator")]
            [ParentPartType(DefaultPartTypes.DevelopmentTool)]
            Debugger,
            [PartTypeInfo("A breakout board helps to isolate pins of an integrated circuit.", "A", "Development_Tools", keywords: "breakout board")]
            [ParentPartType(DefaultPartTypes.DevelopmentTool)]
            Breakout,
            [PartTypeInfo("Breadboard is a solderless method for quickly building and testing circuits using jumper wires.", "A", "Development_Tools")]
            [ParentPartType(DefaultPartTypes.DevelopmentTool)]
            Breadboard,
            [PartTypeInfo("Consumables consist of things that are expended during the production of a product.", "A", "Development_Tools")]
            Consumable,
            [PartTypeInfo("Solder is used for connecting metal electrical contacts using heat.", "A", "Consumable")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Solder,
            [PartTypeInfo("Solder paste is a mixture of tiny metal spheres suspended in flux for connecting surface-mount components using heat.", "A", "Consumable", keywords: "solder paste")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            SolderPaste,
            [PartTypeInfo("Solder mask prevents unintended solder from flowing onto traces during reflow and protects from oxidation.", "A", "Consumable", keywords: "solder mask")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            SolderMask,
            [PartTypeInfo("Flux is a chemical agent used to clean metal surfaces by removing oxidation and impurities.", "A", "Consumable", keywords: "liquid flux,flux pen")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Flux,
            [PartTypeInfo("Blank copper clad board used for creating printed circuit boards.", "A", "Consumable")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            CopperBoard,
            [PartTypeInfo("Blank copper clad board with a single copper pad per hole used for prototyping.", "A", "Consumable")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Perfboard,
            [PartTypeInfo("Blank copper clad board with connected copper strips in various line patterns used for prototyping.", "A", "Consumable", keywords: "veroboard")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Stripboard,
            [PartTypeInfo("Mimics the layout of a standard solderless breadboard featuring connected rows and power rails.", "A", "Consumable")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            SolderableBreadboard,
            [PartTypeInfo("Single wire such as stranded, solid or jumper wire.", "A", "Consumable", keywords: "jumper wire,stranded wire,solid wire,wires")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Wire,
            [PartTypeInfo("High performance polyimide film tape with silicone adhesive is a high heat and chemical resistant tape.", "A", "Consumable", keywords: "capton,kapton tape,amber tape,yellow tape")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Kapton,
            [PartTypeInfo("General adhesive tape.", "A", "Consumable")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Tape,
            [PartTypeInfo("General adhesive glues, sealants and epoxies.", "A", "Consumable", keywords: "epoxy,permabond,rtv silicone,polyurethane,glue,hot melt")]
            [ParentPartType(DefaultPartTypes.Consumable)]
            Adhesive,
            [PartTypeInfo("Equipment and tools used for development of electronics.", "A", "Equipment")]
            Equipment,
            [PartTypeInfo("Testing and measuring equipment such as oscilloscopes, multimeters or analyzers.", "A", "Equipment", keywords: "oscilloscope,dmm,function generator,signal analyzer,power supply,rf analyzer,spectrum analyzer,frequency counter,multimeter,can bus analyzer,logic analyzer,esr meter,capacitance meter,lcr meter,pwr meter,cable tester,electronic load,continuity tester,ohmmeter,signal generator,particle counter,diodes analyzer,pwr analyzer,data logger,tachometer,cable test,dc pwr source,strain gauge,resistance tester,dynamometer,megohmmeter,protocol analyzer")]
            [ParentPartType(DefaultPartTypes.Equipment)]
            TestAndMeasurement,
            [PartTypeInfo("Probes and test leads used for test and measurement equipment.", "A", "Equipment")]
            [ParentPartType(DefaultPartTypes.Equipment)]
            Probes,
            [PartTypeInfo("Hand tools for performing manual tasks.", "A", "Equipment")]
            [ParentPartType(DefaultPartTypes.Equipment)]
            HandTools,
            [PartTypeInfo("Used to display information visually, there are many types of displays from LCD to OLED.", "DS", "Display")]
            Display,
            [PartTypeInfo("Liquid Crystal display comprised of character segments.", "DS", "Display_Character")]
            [ParentPartType(DefaultPartTypes.Display)]
            LCDCharacter,
            [PartTypeInfo("Liquid Crystal display comprised of pixels.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            LCDGraphic,
            [PartTypeInfo("Light Emitting Diode display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            LEDDisplay,
            [PartTypeInfo("Organic Light Emitting Diode display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            OLED,
            [PartTypeInfo("Quantom Dot Light Emitting Diode display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            QLED,
            [PartTypeInfo("Polymer Light Emitting Diode display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            PLED,
            [PartTypeInfo("Thin-Film Transistor LCD display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            TFT,
            [PartTypeInfo("Active Matrix Organic Light Emitting Diode.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            AMOLED,
            [PartTypeInfo("In-Plane Switching LCD display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            IPS,
            [PartTypeInfo("Low-Temperature Polycrystalline Silicon LCD/OLED display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            LTPS,
            [PartTypeInfo("Low-Temperature Polycrystalline Oxide OLED display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            LTPO,
            [PartTypeInfo("Electroluminescent ELD uses electroluminescent material such as Gallium arsenide between two conductors.", "DS", "Display_Graphic", keywords: "eld")]
            [ParentPartType(DefaultPartTypes.Display)]
            Electroluminescent,
            [PartTypeInfo("A vacuum fluorescent tube works by heating a cathode to emit electrons in a vacuum, then accelerating them toward phosphor-coated anodes or grids, where the impact makes the phosphor glow.", "DS", "Display_Character", keywords: "vfd")]
            [ParentPartType(DefaultPartTypes.Display)]
            VacuumFluorescent,
            [PartTypeInfo("Cathode ray tube is a vacuum tube using a hot filament to generate thermo-electrons.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            CRT,
            [PartTypeInfo("Flip-dot displays uses mechanical hinged disks as each pixel of the display.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            FlipDot,
            [PartTypeInfo("Incandescent displays use a coated tungsten filament to run white hot in a vacuum and radiates visible and infrared light.", "DS", "Display_Graphic")]
            [ParentPartType(DefaultPartTypes.Display)]
            Incandescent,
            [PartTypeInfo("Nixie tubes are glow discharge (plasma) displays that utilize high voltage and an inert gas such as neon.", "DS", "Display_Character")]
            [ParentPartType(DefaultPartTypes.Display)]
            Nixie,
            [PartTypeInfo("Plasma, or PDP displays use controlled gas discharge paths combined with phosphor.", "DS", "Display_Character")]
            [ParentPartType(DefaultPartTypes.Display)]
            Plasma,
            [PartTypeInfo("Headers are used for board to board or board to module connections and come in male and female varieties with different pin pitches.", "J", "Connector", keywords: "conn header")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Header,
            [PartTypeInfo("Spring loaded connectors are used to establish electrical contact between two seperate objects.", "J", "Connector", keywords: "spring loaded,conn spring,pogo pins,pogo")]
            [ParentPartType(DefaultPartTypes.Connector)]
            SpringLoaded,
            [PartTypeInfo("Contacts are used as part of a crimp connection to produce multi-conductor connectors.", "J", "Connector", keywords: "conn pin,conn socket")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Contact,
            [PartTypeInfo("Power connectors typically carry an AC or DC connection.", "J", "Connector", keywords: "conn dc,conn pwr,conn power")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Power,
            [PartTypeInfo("Barrel connectors are often used in audio or low current DC power applications.", "J", "Connector", keywords: "conn jack,conn plug")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Barrel,
            [PartTypeInfo("Banana connectors allow conversion from one connector type to another and are found in audio or DC power applications.", "J", "Connector", keywords: "adapt ban,ban2,bana,conn banana")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Banana,
            [PartTypeInfo("Modular connectors are a series of standardized interconnect products commonly used in telecommunications.", "J", "Connector", keywords: "conn mod,conn wrg,shielded jack")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Modular,
            [PartTypeInfo("Universal Serial Bus connectors.", "J", "Connector", keywords: "conn rcpt usb,usb type,conn rcp usb")]
            [ParentPartType(DefaultPartTypes.Connector)]
            USB,
            [PartTypeInfo("High Definition Multimedia Interface (HDMI) is a standard used for transmitting digital uncompressed video and audio signals.", "J", "Connector", keywords: "hdmi rec,conn rcpt hdmi")]
            [ParentPartType(DefaultPartTypes.Connector)]
            HDMI,
            [PartTypeInfo("D-Sub are named for the D-shaped shround at their mating end and typically feature a high density of conductors used for board-mount applications.", "J", "Connector", keywords: "serial,rs232,rs-232,rs485,rs-485,conn serial,dsub,d-sub,conn scsi,conn rcpt,conn plug,conn stck")]
            [ParentPartType(DefaultPartTypes.Connector)]
            DSub,
            [PartTypeInfo("RF connectors are designed to maintain a consistent characteristic impedance over the length of the material for high frequency signals.", "J", "Connector", keywords: "conn rf,conn umc,conn bnc,conn coax,conn umcc,conn swd,umcc,micro-coax,conn sma,conn mcx,conn u.fl,conn rp-sma,conn mmcx,conn tnc,conn fakra,conn mini bnc,conn smb,conn smt")]
            [ParentPartType(DefaultPartTypes.Connector)]
            RFConnector,
            [PartTypeInfo("Jumpers or shunts are designed to connect two pin contacts for an electrical connection of a specified pitch.", "J", "Connector", keywords: "conn jumper,conn shunt,shunt jumper")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Jumper,
            [PartTypeInfo("Terminal blocks are used for wire to board connections.", "J", "Connector", keywords: "term block,terminal block,term blk")]
            [ParentPartType(DefaultPartTypes.Connector)]
            TerminalBlock,
            [PartTypeInfo("Terminal connectors are single conductor connectors that can provide a quick solderless connection.", "J", "Connector", keywords: "conn spade,conn term rcpt,nullet,conn knife,conn wire pin,conn ring")]
            [ParentPartType(DefaultPartTypes.Connector)]
            Terminal,
            [PartTypeInfo("Terminal strips provide multiple conductor connectors that are solderable or terminal posts mounted to an insulated material.", "J", "Connector", keywords: "term lug strip,turret,bindpost strip")]
            [ParentPartType(DefaultPartTypes.Connector)]
            TerminalStrip,
            [PartTypeInfo("An optical sensor convert light and/or heat input into an analog or digital output.", "U", "Sensor_Optical", keywords: "sensor opt")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            OpticalSensor,
            [PartTypeInfo("An image sensor consists of an array of photosensitive materials used for still image or video capture.", "U", "Sensor_Optical", keywords: "image sensor,sensor dgtl,sensor image,cis so")]
            [ParentPartType(DefaultPartTypes.Sensor)]
            ImageSensor,
            [PartTypeInfo("A level shifter or logic translator IC are devices used to pass information between differennt logic devices that are operating from different supply voltages.", "U", "Interface", keywords: "ic xltr,ic translation")]
            [ParentPartType(DefaultPartTypes.IC)]
            LevelShifterIc,
            [PartTypeInfo("An inverter IC reverses a digital input signal turning a high to low or low to high.", "U", "Interface", keywords: "ic buffer/inverter,ic buf/inv,ic buffer inverting")]
            [ParentPartType(DefaultPartTypes.IC)]
            InverterIc,
            [PartTypeInfo("Logic buffers allow isolated access to logic signals from different circuits.", "U", "Interface", keywords: "ic buff non-invert,ic buffer non-invert")]
            [ParentPartType(DefaultPartTypes.IC)]
            BufferIc,
            [PartTypeInfo("Shift registers are logic devices used to convert digital signals between serial and parallel formats.", "U", "Interface", keywords: "ic sr,ic shift regtr,ic shft reg")]
            [ParentPartType(DefaultPartTypes.IC)]
            ShiftRegisterIc,
            [PartTypeInfo("An Digital-to-Analog Converter (DAC) IC is a type of integrated circuit (IC) that transforms a digital signal into a continuous analog signal, such as a voltage or current.", "U", "Analog_DAC", keywords: "ic dac")]
            [ParentPartType(DefaultPartTypes.IC)]
            DAC,
            [PartTypeInfo("An Analog Front End is a semiconductor device that is used for signal conditioning using analog amplifiers, op amps and filters.", "U", "Analog_AFE", keywords: "ic afe")]
            [ParentPartType(DefaultPartTypes.IC)]
            AFE,
            [PartTypeInfo("Complex Programmable Logic Device (CPLD) is a non-volatile, reconfigurable digital integrated circuit used to implement custom logic functions.", "U", "FPGA", keywords: "ic clpd")]
            [ParentPartType(DefaultPartTypes.IC)]
            CPLD,
            [PartTypeInfo("A niobium oxide capacitor are a type of electrolytic capacitor that uses oxides of niobium as the anode and dielectric material.", "C", "Device:C", keywords: "cap niob,cap niob oxid")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            NiobiumCapacitor,
            [PartTypeInfo("A silicon capacitor are near ideal capacitors with great stability and produced using the same methods as semiconductor devices.", "C", "Device:C", keywords: "cap silicon")]
            [ParentPartType(DefaultPartTypes.Capacitor)]
            SiliconCapacitor,
            [PartTypeInfo("A high frequency (RF) relay is an electromechanical device designed to switch high speed signals.", "K", "Relay", keywords: "relay rf")]
            [ParentPartType(DefaultPartTypes.Relay)]
            HighFrequencyRelay,
            [PartTypeInfo("Automotive relays are designed to operate in high temperature high vibrartion environments typically at 12 to 48VDC.", "K", "Relay", keywords: "relay auto,relay automotive")]
            [ParentPartType(DefaultPartTypes.Relay)]
            AutomotiveRelay,
            [PartTypeInfo("Safety relays are designed with human safety in mind by using force guided contacts, contact verification and often used in flammable environments.", "K", "Relay", keywords: "relay safety,relay guided,sfty rly guide")]
            [ParentPartType(DefaultPartTypes.Relay)]
            SafetyRelay,
        }
    }
}
