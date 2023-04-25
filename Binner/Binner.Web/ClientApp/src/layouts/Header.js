import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { NavMenu } from "./NavMenu";
import { Dropdown, Icon, Flag } from "semantic-ui-react";

// import i18n
import '../i18n';

const lngs = {
    en: { nativeName: 'English', flag: 'gb' },      // english
    it: { nativeName: 'Italiano', flag: 'it' },      // italian
    de: { nativeName: 'Deutsch', flag: 'de' },      // german
    // temporary: enable languages as the translations are finished.
    //fr: { nativeName: 'Français ', flag: 'fr' },    // french
    //es: { nativeName: 'Español', flag: 'mx' },      // spanish
    zh: { nativeName: '中文', flag: 'cn' },         // chinese
};

export function Header() {
  const { i18n } = useTranslation();
  // console.log('resolved langauge', i18n.resolvedLanguage);
  const [language, setLanguage] = useState(i18n.resolvedLanguage || 'en');

  useEffect(() => {
    setLanguage(i18n.resolvedLanguage || 'en');
  }, []);

  const langOptions = Object.keys(lngs).map((lng, key) => ({
    key,
    text: lngs[lng].nativeName,
    value: lng,
    content: <span><Flag name={lngs[lng].flag} />{lngs[lng].nativeName}</span>
  }));

  const handleChange = (e, control) => {
    e.preventDefault();
    setLanguage(control.value);
    // localStorage.setItem('i18nextLng', control.value);
    i18n.changeLanguage(control.value);
  };

  return (
    <div className='header'>
      <div className='language-selection'>
        <Icon name="world" />
        <Dropdown
          selection
          inline
          value={language || 'en'}
          options={langOptions}
          onChange={handleChange}
        />
      </div>
      <NavMenu />
    </div>
  );
}
