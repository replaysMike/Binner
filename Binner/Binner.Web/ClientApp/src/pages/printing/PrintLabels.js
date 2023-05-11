import React, { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { DEFAULT_FONT } from '../../common/Types';
import { FormHeader } from "../../components/FormHeader";
import { HandleBinaryResponse } from "../../common/handleResponse.js";
import { Button, Icon, Form, Input, Checkbox, Table, Image, Dropdown, Breadcrumb } from "semantic-ui-react";
import { fetchApi } from "../../common/fetchApi";
import { getImagesToken } from "../../common/authentication";


export function PrintLabels(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const barcodeFonts = ["IDAutomationHC39M Free Version"];
  const defaultPrintPreferences = {
    lastLabelName: "30277",
    lastLabelSource: 0,
    lastFont: DEFAULT_FONT
  };
  var quan = [];
  for (var i = 1; i <= 10; i++) {
    quan.push({ key: i, value: i, text: i.toString() });
  }

  const [loading, setLoading] = useState(false);
  const [printPreferences, setPrintPreferences] = useState(JSON.parse(localStorage.getItem("printPreferences")) || defaultPrintPreferences);
  const [printLabelHistory, setPrintLabelHistory] = useState(JSON.parse(localStorage.getItem("printLabelHistory")) || []);
  const [lines, setLines] = useState([]);
  const [quantity, setQuantity] = useState(1);
  const [imgBase64, setImgBase64] = useState("");
  const [labelName, setLabelName] = useState(printPreferences.lastLabelName || "30277");
  const [labelSource, setLabelSource] = useState(printPreferences.lastLabelSource || 0);
  const [quantities, setQuantities] = useState(quan);
  const [banner, setBanner] = useState(null);
  const [labelPositions, setLabelPositions] = useState([
    {
      key: 1,
      value: 0,
      text: "Left"
    },
    {
      key: 2,
      value: 1,
      text: "Right"
    },
    {
      key: 3,
      value: 2,
      text: "Center"
    }
  ]);
  const [labelNames, setLabelNames] = useState([
    {
      key: 1,
      value: "30277",
      text: '30277 (Dual 9/16" x 3 7/16")',
      defaults: {
        label: 1, // use label 1,
        fontSizes: [16, 7, 16],
        topMargins: [0, 10, 0],
        isBarcodes: [false, false, true],
        fonts: [DEFAULT_FONT]
      }
    },
    {
      key: 2,
      value: "30346",
      text: '30346 (1/2" x 1 7/8")',
      defaults: {
        label: 2, // use label 2, there is no label 1,
        fontSizes: [16, 6, 16],
        topMargins: [0, 10, 0],
        isBarcodes: [false, false, true],
        fonts: [DEFAULT_FONT]
      }
    }
  ]);
  const [labelSources, setLabelSources] = useState([
    {
      key: 1,
      value: 0,
      text: "Auto"
    },
    {
      key: 2,
      value: 1,
      text: "Left"
    },
    {
      key: 3,
      value: 2,
      text: "Right"
    }
  ]);
  const [fonts, setFonts] = useState([
    {
      key: 1,
      value: 0,
      text: "Loading"
    }
  ]);

  useEffect(() => {
    const loadFonts = async () => {
      setLoading(true);
      await fetchApi("api/print/fonts", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((response) => {
        const { data } = response;
        const newFonts = data.map((l, k) => {
          return {
            key: k,
            value: l,
            text: l
          };
        });
        const selectedFont = _.find(newFonts, (x) => x && x.text === DEFAULT_FONT);
        setLoading(false);
        setFonts(newFonts);
    });
    };
    
    loadFonts();
  }, []);  

  const onSubmit = async (e) => {
    setLoading(true);
    const maxLineHistory = 20;

    const entry = {
      date: new Date(),
      // dereference and copy
      lines: JSON.parse(JSON.stringify(lines))
    };
    var newPrintLabelHistory = printLabelHistory;
    if (
      _.filter(printLabelHistory, (f) => _.isEqual(f.lines, entry.lines))
        .length === 0
    ) {
      printLabelHistory.push(entry);
      newPrintLabelHistory = printLabelHistory.slice(
        Math.max(printLabelHistory.length - maxLineHistory, 0)
      );
      localStorage.setItem(
        "printLabelHistory",
        JSON.stringify(newPrintLabelHistory)
      );
    }

    const request = {
      token: getImagesToken(),
      showDiagnostic: false,
      labelName,
      labelSource,
      lines: lines.map((l) => {
        return {
          label: Number.parseInt(l.label),
          content: l.content,
          fontSize: Number.parseInt(l.fontSize),
          position: Number.parseInt(l.position),
          fontName: l.font,
          margin: {
            top: Number.parseInt(l.topMargin),
            left: Number.parseInt(l.leftMargin)
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: false
    };

    for (var i = 0; i < quantity; i++) {
      await fetchApi("api/print/custom", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
      });
    }

    setLoading(false);
    setPrintLabelHistory(newPrintLabelHistory);
    setQuantity(1);
  };

  const handlePreview = async (e) => {
    e.preventDefault();
    e.stopPropagation();

    const request = {
      token: getImagesToken(),
      showDiagnostic: true,
      labelName,
      labelSource,
      lines: lines.map((l) => {
        return {
          label: Number.parseInt(l.label),
          content: l.content,
          fontSize: Number.parseInt(l.fontSize),
          position: Number.parseInt(l.position),
          fontName: l.font,
          margin: {
            top: Number.parseInt(l.topMargin),
            left: Number.parseInt(l.leftMargin)
          },
          barcode: l.barcode
        };
      }),
      generateImageOnly: true
    };

    await fetchApi("api/print/custom", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }).then((response) => {
      HandleBinaryResponse(response.responseObject).then((b) => {
        const base64 = arrayBufferToBase64(b);
        setImgBase64(base64); 
      });
    });
  };

  const arrayBufferToBase64 = (buffer) => {
    let binary = "";
    let bytes = [].slice.call(new Uint8Array(buffer));

    bytes.forEach((b) => (binary += String.fromCharCode(b)));

    return btoa(binary);
  };

  const handleChange = (e, control) => {
    let newLabelName = labelName;
    let newLabelSource = labelSource;
    let newQuantity = quantity;
    switch (control.name) {
      case "labelName":
        newLabelName = control.value;
        localStorage.setItem(
          "printPreferences",
          JSON.stringify({ ...printPreferences, lastLabelName: control.value })
        );
        break;
      case "labelSource":
        newLabelSource = control.value;
        localStorage.setItem(
          "printPreferences",
          JSON.stringify({
            ...printPreferences,
            lastLabelSource: control.value
          })
        );
        break;
      case "quantity":
        newQuantity = control.value;
        break;
      default:
        break;
    }
    setLabelName(newLabelName);
    setLabelSource(newLabelSource);
    setPrintPreferences({...printPreferences});
    setQuantity(newQuantity);
  };

  const handleLineChange = (e, control, line) => {
    switch (control.name) {
      case "barcode":
        line[control.name] = control.checked || false;
        break;
      case "font":
        if (barcodeFonts.includes(control.value)){
          setBanner(t('page.printLabels.barcodeFontsNotice', "Barcode fonts may be problematic. Values must be surrounded by asterix, ex: *value* or parenthesis, ex: (value) to encode correctly. For best accuracy use a font size of [6, 12, 18, 24, 30, 36]. Alternatively, toggle IsBarcode and a barcode will be drawn for you and no special characters needed."));
        } else {
          setBanner(null);
        }
        line[control.name] = control.value;
        break;
      default:
        line[control.name] = control.value;
        break;
    }
    setLines([...lines])
  };

  const handleLoad = (e, entry) => {
    e.preventDefault();
    e.stopPropagation();
    // dereference and copy
    const newLines = JSON.parse(JSON.stringify(entry.lines));
    // ensure all fields are populated, even for older history that is missing fields
    const formatLines = newLines.map((x) => {
      return {
        id: x.id,
        label: x.label,
        content: x.content || "",
        fontSize: x.fontSize || 16,
        position: x.position || 2,
        topMargin: x.topMargin || 0,
        leftMargin: x.leftMargin || 0,
        font: x.font || DEFAULT_FONT,
        barcode: x.barcode || false
      };
    });
    setLines(formatLines);
  };

  const handleAdd = (e) => {
    e.preventDefault();
    e.stopPropagation();
    const label = _.find(labelNames, (x) => x && x.value === labelName);
    const lastLine = _.last(lines);
    const newLine = {
      id: lines.length + 1,
      label: (lastLine && lastLine.label) || (label && label.defaults.label),
      content: "",
      fontSize:
        label &&
        label.defaults.fontSizes.length > lines.length &&
        (label.defaults.fontSizes[lines.length] || 16),
      position: 2,
      topMargin:
        label &&
        label.defaults.topMargins.length >= lines.length &&
        (label.defaults.topMargins[lines.length] || 0),
      leftMargin: 0,
      font:
        label &&
        label.defaults.fonts.length > lines.length &&
        (label.defaults.fonts[lines.length] || DEFAULT_FONT),
      barcode:
        label &&
        label.defaults.isBarcodes.length >= lines.length &&
        (label.defaults.isBarcodes[lines.length] || false)
    };
    lines.push(newLine);
    setLines([...lines]);
  };

  const handleReset = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setLines([]);
    setBanner(null);
  };

  const removeLine = (e, line, index) => {
    e.preventDefault();
    e.stopPropagation();
    lines.splice(lines.indexOf(line), 1);
    setLines([...lines]);
  };

  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/printing")}>{t('bc.printing', "Printing")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('page.printLabels.title', "Print Label")}</Breadcrumb.Section>
      </Breadcrumb>
      <FormHeader name={t('page.printLabels.title', "Print Label")} to="/printing">
        <Trans i18nKey="page.printLabels.description">
          Print custom multi-line labels for your storage bins.
          <br />
          Print history is kept so you can reuse templates for your labels.
        </Trans>
			</FormHeader>
      <Form onSubmit={onSubmit} loading={loading}>
        <Form.Group>
          <Form.Dropdown
            label={t('page.printLabels.label.labelType', "Label Type")}
            placeholder="30277"
            selection
            value={labelName}
            options={labelNames}
            onChange={handleChange}
            name="labelName"
          />
          <Form.Dropdown
            label={t('page.printLabels.label.paperSource', "Paper Source")}
            placeholder="Auto"
            selection
            value={labelSource}
            options={labelSources}
            onChange={handleChange}
            name="labelSource"
          />
        </Form.Group>
        <Button onClick={handleAdd}>{t('button.addLine', "Add line")}</Button>
        <Button onClick={handleReset}>{t('button.reset', "Reset")}</Button>

        {banner && <div className="page-notice resizable">{banner}</div>}

        <Table compact celled size="small">
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>{t('page.printLabels.label.labelNum', "Label #")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.text', "Text")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.fontSize', "FontSize")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.alignment', "Alignment")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.topMargin', "Top Margin")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.leftMargin', "Left Margin")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.font', "Font")}</Table.HeaderCell>
              <Table.HeaderCell>{t('page.printLabels.label.isBarcode', "Is Barcode")}</Table.HeaderCell>
              <Table.HeaderCell></Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {lines.length > 0 
            ? lines.map((l, k) => (
              <Table.Row key={k}>
                <Table.Cell>
                  <Input
                    name="label"
                    value={l.label}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Input
                    name="content"
                    value={l.content}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Input
                    name="fontSize"
                    value={l.fontSize}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Dropdown
                    name="position"
                    placeholder={t('page.printLabels.label.center', "Center")}
                    selection
                    value={l.position}
                    options={labelPositions}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Input
                    name="topMargin"
                    value={l.topMargin}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Input
                    name="leftMargin"
                    value={l.leftMargin}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Dropdown
                    name="font"
                    selection
                    value={l.font}
                    options={fonts}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Checkbox
                    toggle
                    name="barcode"
                    checked={l.barcode}
                    onChange={(e, c) => handleLineChange(e, c, l)}
                  />
                </Table.Cell>
                <Table.Cell>
                  <Button
                    circular
                    size="mini"
                    icon="delete"
                    title={t('label.delete', "Delete")}
                    onClick={(e) => removeLine(e, l, k)}
                  />
                </Table.Cell>
              </Table.Row>
            ))
          : (<Table.Row><Table.Cell colSpan={9} textAlign="center">{t('page.printLabels.addLineToStart', "Add a line to get started.")}</Table.Cell></Table.Row>)}
          </Table.Body>
        </Table>
        <Form.Group>
          <Form.Dropdown
            label={t('label.quantity', "Quantity")}
            placeholder="1"
            selection
            value={quantity}
            options={quantities}
            onChange={handleChange}
            name="quantity"
          />
        </Form.Group>
        <Button onClick={handlePreview}>
          <Icon name="eye" /> {t('button.preview', "Preview")}
        </Button>
        <Button primary>
          <Icon name="print" /> {t('button.print', "Print")}
        </Button>
        <div>
          {imgBase64 && (
            <Image
              label={{
                color: "grey",
                className: "transparent",
                content: t('label.preview', "Preview"),
                icon: "eye",
                ribbon: true,
                size: "tiny"
              }}
              name="previewImage"
              src={`data:image/png;base64,${imgBase64}`}
              size="medium"
              style={{ marginTop: "20px" }}
            />
          )}
        </div>
        <div style={{ marginTop: "20px" }}>
          <h2>{t('page.printLabels.printHistory', "Print History")}</h2>
          <Table compact celled size="small">
            <Table.Body>
              {_.sortBy(
                printLabelHistory.map((l, k) => (
                  <Table.Row key={k}>
                    <Table.Cell>
                      {new Date(l.date).toLocaleString("en-US")}
                    </Table.Cell>
                    <Table.Cell>
                      {l.lines.length > 0 ? l.lines[0].content : ""}
                    </Table.Cell>
                    <Table.Cell>
                      {l.lines.length > 1 ? l.lines[1].content : ""}
                    </Table.Cell>
                    <Table.Cell>
                      {l.lines.length > 2 ? l.lines[2].content : ""}
                    </Table.Cell>
                    <Table.Cell>
                      <Button onClick={(e) => handleLoad(e, l)}>
                        <Icon name="folder open" /> {t('button.load', "Load")}
                      </Button>
                    </Table.Cell>
                  </Table.Row>
                )),
                "date"
              ).reverse()}
            </Table.Body>
          </Table>
        </div>
      </Form>
    </div>
  );
}
