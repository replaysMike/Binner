import React, { useState } from "react";
import { useTranslation, Trans } from 'react-i18next';
import { Input, Checkbox, Form, Segment, Header, Dropdown, Popup } from "semantic-ui-react";

const TimeoutLength = 1500;

export function LineTemplate(props) {
  const { t } = useTranslation();
  const [ line, fonts, title, color, font, name, onChange ] = props;
  const [isOpen, setIsOpen] = useState(false);
  const [timeoutValue, setTimeoutValue] = useState(0);
  const [positionOptions] = useState([
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

  const handleOpen = () => {
    setIsOpen(true);
  };

  const handleClose = () => {
    // delay the close by a set timeout, so links can be clicked
    const value = setTimeout(() => {
      setIsOpen(false);
      clearTimeout(timeoutValue);
    }, TimeoutLength);
    setTimeoutValue(value);
  };

  return (
    <Segment color={color} {...props}>
      <Header dividing as="h3">
        {title}
      </Header>
      <Form.Group>
        <Popup
          disabled={false}
          open={isOpen}
          onOpen={handleOpen}
          onClose={handleClose}
          content={
            <p>
              <Trans i18nKey="comp.lineTemplate.popup.content">
              Template can reference any part field, example: &#123;partNumber&#125;, &#123;description&#125;, &#123;manufacturer&#125;, &#123;location&#125;,
              &#123;binNumber&#125;, &#123;cost&#125; etc. See 
              <a href="https://github.com/replaysMike/Binner/wiki/Printing-Configuration#partlabeltemplate-line-options" target="_blank" rel="noreferrer">
                Wiki
              </a> for all available tags.
              </Trans>
            </p>
          }
          trigger={
            <Form.Field width={4}>
              <label>{t('comp.lineTemplate.label.content', "Content")}</label>
              <Input
                name={name + "Content"}
                className="labeled"
                placeholder="{partNumber}"
                value={line.content || ""}
                onChange={onChange}
              />
            </Form.Field>
          }
        />

        <Form.Field width={4}>
          <label>{t('comp.lineTemplate.label.font', "Font")}</label>
          <Dropdown name={name + "FontName"} placeholder={font} selection value={line.fontName} options={fonts} onChange={onChange} />
        </Form.Field>
        <Form.Field width={2}>
          <label>{t('comp.lineTemplate.label.fontSize', "Font Size")}</label>
          <Input name={name + "FontSize"} className="labeled" placeholder="16" value={line.fontSize || ""} onChange={onChange}></Input>
        </Form.Field>
        <Form.Field width={2}>
          <label>{t('comp.lineTemplate.label.fontColor', "Font Color")}</label>
          <Input name={name + "Color"} className="labeled" placeholder="#000000" value={line.color || ""} onChange={onChange}></Input>
        </Form.Field>
        <Form.Field width={3}>
          <label>{t('comp.lineTemplate.label.textPosition', "Text Position")}</label>
          <Dropdown
            name={name + "Position"}
            placeholder="Center"
            selection
            value={line.position}
            options={positionOptions}
            onChange={onChange}
          />
        </Form.Field>
      </Form.Group>
      <Form.Group>
        <Popup
          content={<p>{t('comp.lineTemplate.popup.autoSize', "Text size will be automatically determined.")}</p>}
          trigger={
            <Form.Field width={3}>
              <Checkbox
                name={name + "AutoSize"}
                label="Auto size text"
                className="labeled"
                checked={line.autoSize}
                onChange={onChange}
              />
            </Form.Field>
          }
        />

        <Popup
          content={<p>{t('comp.lineTemplate.popup.upperCaseText', "Render the text as all upper-case characters.")}</p>}
          trigger={
            <Form.Field width={3}>
              <Checkbox
                name={name + "UpperCase"}
                label={t('comp.lineTemplate.label.upperCaseText', "UpperCase Text")}
                className="labeled"
                checked={line.upperCase}
                onChange={onChange}
              />
            </Form.Field>
          }
        />
        <Popup
          content={<p>{t('comp.lineTemplate.popup.lowerCaseText', "Render the text as all lower-case characters.")}</p>}
          trigger={
            <Form.Field width={3}>
              <Checkbox
                name={name + "LowerCase"}
                label={t('comp.lineTemplate.label.lowerCaseText', "LowerCase Text")}
                className="labeled"
                checked={line.lowerCase}
                onChange={onChange}
              />
            </Form.Field>
          }
        />
        <Popup
          content={<p>{t('comp.lineTemplate.popup.barcode', "Render the Content value encoded as a barcode.")}</p>}
          trigger={
            <Form.Field width={3}>
              <Checkbox name={name + "Barcode"} label={t('comp.lineTemplate.label.barcode', "Barcode")} className="labeled" checked={line.barcode} onChange={onChange} />
            </Form.Field>
          }
        />
      </Form.Group>
      <Form.Group>
        <Form.Field width={3}>
          <label>{t('comp.lineTemplate.label.marginLeft', "Margin Left")}</label>
          <Input
            name={name + "MarginLeft"}
            className="labeled"
            placeholder="0"
            value={line.marginLeft || ""}
            onChange={onChange}
          ></Input>
        </Form.Field>
        <Form.Field width={3}>
          <label>{t('comp.lineTemplate.label.marginTop', "Margin Top")}</label>
          <Input name={name + "MarginTop"} className="labeled" placeholder="0" value={line.marginTop || ""} onChange={onChange}></Input>
        </Form.Field>
        <Popup
          content={<p>{t('comp.lineTemplate.popup.rotateDegrees', "Rotate the text in degrees. Example: 90")}</p>}
          trigger={
            <Form.Field width={3}>
              <label>{t('comp.lineTemplate.label.rotateDegrees', "Rotate Degrees")}</label>
              <Input name={name + "Rotate"} className="labeled" placeholder="0" value={line.rotate || ""} onChange={onChange}></Input>
            </Form.Field>
          }
        />
      </Form.Group>
    </Segment>
  );
}
