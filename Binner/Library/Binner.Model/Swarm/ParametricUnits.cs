namespace Binner.Model.Swarm
{
    public enum ParametricUnits
    {
        [Abbreviation("A")]
        Amp,        // A
        [Abbreviation("mA")]
        Milliamp,   // mA
        [Abbreviation("µA", "uA")]
        Microamp,   // µA
        [Abbreviation("nA")]
        Nanoamp,    // nA
        [Abbreviation("pA")]
        Picoamp,    // pA
        [Abbreviation("kA")]
        Kiloamp,   // kA
        [Abbreviation("MA")]
        Megaamp,   // MA
        [Abbreviation("GA")]
        Gigaamp,   // GA

        [Abbreviation("V")]
        Volt,       // V
        [Abbreviation("mV")]
        Millivolt,  // mV
        [Abbreviation("µV", "uV")]
        Microvolt,  // µV
        [Abbreviation("nV")]
        Nanovolt,   // nV
        [Abbreviation("pV")]
        Picovolt,   // pV
        [Abbreviation("kV")]
        Kilovolt,   // kV
        [Abbreviation("MV")]
        Megavolt,   // MV
        [Abbreviation("GV")]
        Gigavolt,   // GV

        [Abbreviation("Ω","ohm")]
        Ohms,       // Ω
        [Abbreviation("mΩ", "milliohm")]
        Milliohm,   // mΩ
        [Abbreviation("nΩ", "nanoohm")]
        Nanoohm,    // nΩ
        [Abbreviation("kΩ", "kiloohm")]
        Kiloohm,    // kΩ
        [Abbreviation("MΩ", "megaohm")]
        Megaohm,    // MΩ
        [Abbreviation("GΩ", "gigaohm")]
        Gigaohm,    // GΩ
        [Abbreviation("statΩ", "statohm")]
        Statohm,    // statΩ
        [Abbreviation("abΩ", "abohm")]
        Abaohm,     // abΩ

        [Abbreviation("F")]
        Farad,      // F
        [Abbreviation("µF", "uF", "mf","mfd")]
        Microfarad, // µF, mf, mfd
        [Abbreviation("nF")]
        Nanofarad,  // nF
        [Abbreviation("pF", "mmf", "mmfd", "pfd", "μμF")]
        Picofarad,  // pF, mmF, mmfd, pfd, μμF
        [Abbreviation("MF")]
        Megafarad,  // MF
        [Abbreviation("GF")]
        Gigafarad,  // GF
        [Abbreviation("abF")]
        Abfarad,    // abF
        [Abbreviation("statF")]
        Statfarad,  // statF

        [Abbreviation("H")]
        Henry,      // H
        [Abbreviation("µH", "uH")]
        Millihenry, // µH
        [Abbreviation("mH")]
        Microhenry, // mH
        [Abbreviation("nH")]
        Nanohenry,  // nH
        [Abbreviation("kH")]
        Kilohenry,  // kH
        [Abbreviation("MH")]
        Megahenry,  // MH
        [Abbreviation("GH")]
        Gigahenry,  // GH

        [Abbreviation("Hz")]
        Hertz,      // Hz
        [Abbreviation("daHz")]
        Decahertz,  // daHz
        [Abbreviation("hHz")]
        Hectohertz, // hHz
        [Abbreviation("kHz")]
        Kilohertz,  // kHz
        [Abbreviation("MHz")]
        Megahertz,  // MHz
        [Abbreviation("GHz")]
        Gigahertz,  // GHz
        [Abbreviation("THz")]
        Terahertz,  // THz
        [Abbreviation("PHz")]
        Petahertz,  // PHz
    }

    public class AbbreviationAttribute : Attribute
    {
        public IEnumerable<string> Abbreviations { get; set; }
        public AbbreviationAttribute(params string[] abbreviations)
        {
            Abbreviations = abbreviations;
        }
    }
}
