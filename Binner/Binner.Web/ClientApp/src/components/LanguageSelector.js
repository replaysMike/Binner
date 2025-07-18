import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Dropdown, Icon, Flag } from "semantic-ui-react";
// import i18n
import '../i18n';

const lngs = {
    en: { nativeName: 'English', flag: 'gb' },      // english
    it: { nativeName: 'Italiano', flag: 'it' },     // italian
    de: { nativeName: 'Deutsch', flag: 'de' },      // german
    fr: { nativeName: 'Français ', flag: 'fr' },    // french
    tr: { nativeName: 'Türkçe ', flag: 'tr' },      // turkish
    zh: { nativeName: '中文', flag: 'cn' },         // chinese
  // temporary: enable languages as the translations are finished.
  //es: { nativeName: 'Español', flag: 'mx' },      // spanish
};

export function LanguageSelector() {
  const { i18n } = useTranslation();
  // console.debug('resolved langauge', i18n.resolvedLanguage);
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

  const handleLanguageChange = (e, control) => {
    e.preventDefault();
    setLanguage(control.value);
    i18n.changeLanguage(control.value);
  };

  return (
    <div className='language-selection'>
      <Icon name="world" />
      <Dropdown
        selection
        inline
        value={language || 'en'}
        options={langOptions}
        onChange={handleLanguageChange}
      />
    </div>);
};