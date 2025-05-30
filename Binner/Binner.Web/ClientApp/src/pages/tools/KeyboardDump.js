import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { Table, Button, Icon, Checkbox, Breadcrumb, Segment } from "semantic-ui-react";
import { Clipboard } from "../../components/Clipboard";
import _ from "underscore";
import "./KeyboardDump.css";

export function KeyboardDump(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [events, setEvents] = useState([]);
  const [preventDefault, setPreventDefault] = useState(true);
  const [inputs, setInputs] = useState({ keydown: true, keyup: false, console: false });
  const [historyElements, setHistoryElements] = useState([]);
  const [historyStr, setHistoryStr] = useState([]);
  const [activeModifierValues, setActiveModifierValues] = useState([]);

  const onKeydown = (e) => {
    if (preventDefault) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (inputs.keydown && inputs.console) console.debug('keydown', e);
    if (inputs.keydown) addEvent('keydown', e);
  };

  const onKeyup = (e) => {
    if (preventDefault) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (inputs.keyup && inputs.console) console.debug('keyup', e);
    if (inputs.keyup) addEvent('keyup', e);
  };

  useEffect(() => {
    const enableListening = () => {
      // start listening for all key presses on page
      addKeyboardHandler();
    };

    enableListening();

    return () => {
      // stop listening for key presses
      removeKeyboardHandler();
    };

  }, [onKeydown, onKeyup]);

  const addKeyboardHandler = () => {
    if (document) {
      document.addEventListener("keydown", onKeydown);
      document.addEventListener("keyup", onKeyup);
    }
  };

  const removeKeyboardHandler = () => {
    if (document) {
      document.removeEventListener("keydown", onKeydown);
      document.removeEventListener("keyup", onKeyup);
    }
  };

  const addEvent = (eventName, e) => {
    //events.unshift({ name: eventName, event: e });
    const obj = { name: eventName, event: e };
    //events.push(obj);
    setEvents(prev => [...prev, obj]);
    if ((inputs.keydown && eventName === 'keydown') || (!inputs.keydown && eventName === 'keyup')) {
      const ignoreCodes = [16, 17, 18]; // ignore modifier keys (shift, alt, ctrl)
      if (ignoreCodes.includes(e.keyCode)) {
        // not valid to print out as a string
      } else {
        let key = e.key;
        let hasModifier = false;
        if (e.altKey || e.ctrlKey) hasModifier = true;
        if (e.altKey) {
          setActiveModifierValues([...activeModifierValues, e]);
        } else if (activeModifierValues.length > 0) {
          // decode the ascii/unicode modifier values
          const hexStr = activeModifierValues.map(i => i.key).join('');
          let val = Number('0x' + hexStr);
          let dec = parseInt(hexStr);
          let entity = '';
          // transform control characters to visible unicode
          switch(dec) {
            case 1:
              entity = '\u2401'; // start of heading
              break;
            case 2:
              entity = '\u2402'; // start of text
              break;
            case 3:
              entity = '\u2403'; // end of text
              break;
            case 4:
              entity = '\u2404'; // eot
              break;
            case 11:
              entity = '\u2405'; // vert tab
              break;
            case 12:
              entity = '\u2406'; // form feed
              break;
            case 13:
              entity = '\u2407'; // carriage return
              break;
            case 14:
              entity = '\u2408'; // shift out
              break;
            case 15:
              entity = '\u2409'; // shift in
              break;
            case 16:
              entity = '\u2410'; // data link escape
              break;
            case 17:
              entity = '\u2411'; // device control 1
              break;
            case 18:
              entity = '\u2412'; // device control 2
              break;
            case 19:
              entity = '\u2413'; // device control 3
              break;
            case 20:
              entity = '\u2414'; // device control 4
              break;
            case 21:
              entity = '\u2415'; // nak
              break;
            case 22:
              entity = '\u2416'; // sync idle
              break;
            case 23:
              entity = '\u2417'; // etb
              break;
            case 24:
              entity = '\u2418'; // cancel
              break;
            case 25:
              entity = '\u2419'; // end of medium
              break;
            case 26:
              entity = '\u241a'; // substitute
              break;
            case 27:
              entity = '\u241b'; // esc
              break;
            case 28:
              entity = '\u241c'; // file separator
              break;
            case 29:
              entity = '\u241d'; // group separator
              break;
            case 30:
              entity = '\u241e'; // record separator
              break;
            case 31:
              entity = '\u241f'; // unit separator
              break;
            default:
              entity = htmlDecode(`&#x${val};`); // convert to unicode
          }
          const el = (<span className="unicode red">{entity}</span>);
          setHistoryElements(prev => [...prev, el]);
          setHistoryStr(prev => prev + entity);
          setActiveModifierValues([]);
        }
        
        if (!hasModifier) {
          setHistoryElements(prev => [...prev, key]);
          let str = e.key;
          switch(e.keyCode) {
            case 13:
              key = (<span className="unicode">&#x240D;</span>); // lf
              str = "\n";
              break;
            case 12:
              key = (<span className="unicode">&#x240A;</span>); // cr
              str = "\r";
              break;
            case 9:
              key = (<span className="unicode red">&#x2409;</span>); // tab
              str = "\t";
              break;
          }
          setHistoryStr(prev => prev + str);
        }
      }
    }
    //console.log('event', eventName, e);
  };

  function htmlDecode(input) {
    var doc = new DOMParser().parseFromString(input, "text/html");
    return doc.documentElement.textContent;
  }

  const showModifiers = (e) => {
    const modifiers = [];
    if (e.shiftKey) {
      if (e.location === KeyboardEvent.DOM_KEY_LOCATION_LEFT)
        modifiers.push('LeftShift');
      else if (e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT)
        modifiers.push('RightShift');
      else
        modifiers.push('Shift');
    }
    if (e.altKey) {
      if (e.location === KeyboardEvent.DOM_KEY_LOCATION_LEFT)
        modifiers.push('LeftAlt');
      else if (e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT)
        modifiers.push('RightAlt');
      else
        modifiers.push('Alt');
    }
    if (e.ctrlKey) {
      if (e.location === KeyboardEvent.DOM_KEY_LOCATION_LEFT)
        modifiers.push('LeftCtrl');
      else if (e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT)
        modifiers.push('RightCtrl');
      else
        modifiers.push('Ctrl');
    }
    if (e.metaKey) modifiers.push('Meta'); // mac: Command key, windows: Windows key
    if (e.getModifierState("ScrollLock")) modifiers.push('ScrollLock');
    if (e.getModifierState("NumLock")) modifiers.push('NumLock');
    if (e.getModifierState("CapsLock")) modifiers.push('CapsLock');

    return modifiers.join(', ');
  };

  const showSACM = (e) => {
    const modifiers = ['.', '.', '.', '.'];
    if (e.shiftKey) modifiers[0] = 's';
    if (e.altKey) modifiers[1] = 'a';
    if (e.ctrlKey) modifiers[2] = 'c';
    if (e.metaKey) modifiers[3] = 'm'; // mac: Command key, windows: Windows key
    return modifiers.join('');
  };

  const handleClear = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setEvents([]);
    setHistoryElements([]);
    setHistoryStr([]);
  };

  const handleChange = (e, control) => {
    inputs[control.name] = control.checked;
    setInputs({ ...inputs });
  };

  const renderHistory = useMemo(() => {
    return _.filter(events, i => (i.name === 'keydown' && inputs.keydown) || (i.name === 'keyup' && inputs.keyup)).map((row, key) => (
      <Table.Row key={key} className={row.name}>
        <Table.Cell>{row.name}</Table.Cell>
        <Table.Cell className="sacm">{showSACM(row.event)}</Table.Cell>
        <Table.Cell>{row.event.keyCode}</Table.Cell>
        <Table.Cell>{row.event.key}</Table.Cell>
        <Table.Cell>{row.event.code}</Table.Cell>
        <Table.Cell className="modifiers">{showModifiers(row.event)}</Table.Cell>
      </Table.Row>
    ));
  }, [events, inputs]);

  //console.log('inputs', inputs);
  return (
    <div>
      <Breadcrumb>
        <Breadcrumb.Section link onClick={() => navigate("/")}>{t('bc.home', "Home")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section link onClick={() => navigate("/tools")}>{t('bc.tools', "Tools")}</Breadcrumb.Section>
        <Breadcrumb.Divider />
        <Breadcrumb.Section active>{t('bc.keyboardDump', "Keyboard Dump")}</Breadcrumb.Section>
      </Breadcrumb>
      
      <Segment color="blue">
        <Button primary type="button" size='tiny' onClick={handleClear} disabled={events.length === 0}><Icon name="eraser" /> {t('buttons.clear', "Clear")}</Button>
        <Checkbox label="keydown" name="keydown" checked={inputs.keydown} onChange={handleChange} style={{ paddingLeft: '10px' }} />
        <Checkbox label="keyup" name="keyup" checked={inputs.keyup} onChange={handleChange} style={{ paddingLeft: '10px' }} />
        <Checkbox label="console" name="console" checked={inputs.console} onChange={handleChange} style={{ paddingLeft: '10px' }} />
        <div className="history">
          {historyStr?.length > 0 && <Clipboard text={historyStr} />}
          {historyElements.map((element, ckey) => (<span key={ckey}>{element}</span>))}
        </div>
      </Segment>

      <Segment color="green">
        <h5>{t('page.keyboardDump.history', "History")}</h5>
        <Table>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell width={2}>{t('page.keyboardDump.event', "Event")}</Table.HeaderCell>
              <Table.HeaderCell width={2}>{t('page.keyboardDump.sacm', "SACM")}</Table.HeaderCell>
              <Table.HeaderCell width={2}>{t('page.keyboardDump.value', "Value")}</Table.HeaderCell>
              <Table.HeaderCell width={2}>{t('page.keyboardDump.key', "Key")}</Table.HeaderCell>
              <Table.HeaderCell width={2}>{t('page.keyboardDump.code', "Code")}</Table.HeaderCell>
              <Table.HeaderCell width={6}>{t('page.keyboardDump.modifiers', "Modifiers")}</Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {renderHistory}
          </Table.Body>
        </Table>
      </Segment>
    </div>
  );
};