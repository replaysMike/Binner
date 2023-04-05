import React, { useEffect, useState } from 'react';
import { NavMenu } from "./NavMenu";
import { Dropdown, Icon } from "semantic-ui-react";
import { useTranslation } from 'react-i18next';

// import i18n
import '../i18n';

const lngs = {
    en: { nativeName: 'English' },      // english
    de: { nativeName: 'Deutsch' },      // german
    fr: { nativeName: 'Français ' },    // french
    es: { nativeName: 'Español' },      // spanish
    zh: { nativeName: '中文' },         // chinese
};

export function Header() {
  const { t, i18n } = useTranslation();
  const [language, setLanguage] = useState(localStorage.getItem('language') || i18n.resolvedLanguage || 'en');

  useEffect(() => {
    // console.log('init', localStorage.getItem('language'), i18n.resolvedLanguage);
    setLanguage(localStorage.getItem('language') || i18n.resolvedLanguage || 'en');
  }, []);

  const langOptions = Object.keys(lngs).map((lng, key) => ({
    key,
    text: lngs[lng].nativeName,
    value: lng,
  }));

  const handleChange = (e, control) => {
    e.preventDefault();
    setLanguage(control.value);
    localStorage.setItem('language', control.value);
    i18n.changeLanguage(control.value);
  };

  return (
    <div className='header'>
      <div className='language-selection'>
        <Icon name="world" />
        <Dropdown
          selection
          value={language || 'en'}
          options={langOptions}
          onChange={handleChange}
        />
      </div>
      <NavMenu />
    </div>
  );
}
