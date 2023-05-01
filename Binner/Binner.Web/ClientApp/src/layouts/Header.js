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

/**
 * Global Header
 */
export function Header() {
  const { i18n } = useTranslation();
  // console.log('resolved langauge', i18n.resolvedLanguage);
  const [language, setLanguage] = useState(i18n.resolvedLanguage || 'en');

  useEffect(() => {
    setLanguage(i18n.resolvedLanguage || 'en');
    const documentMinBoundsPerc = 0.1;
    const documentMaxBoundsPerc = 0.8;

    // randomize the bottom promo avatars
    var time = 1400 * 4 * Math.random() * -1;
    var footer = document.querySelector('footer');
    footer.style.animationDelay = (time + 's');

    // .sticky-target support handling
    // allows for popping static controls to the bottom of the scroll window, such as save buttons.
    document.addEventListener("scroll", (event) => {
      const stickies = document.getElementsByClassName('sticky-target');
      if (stickies.length > 0) {
        const sticky = stickies[0];
        const bounds = sticky.getAttribute("data-bounds");
        let minBounds = documentMinBoundsPerc;
        let maxBounds = documentMaxBoundsPerc;
        if (bounds) {
          const parts = bounds.split(',');
          minBounds = parseFloat(parts[0]);
          maxBounds = parseFloat(parts[1]);
        }
        const documentMin = document.body.scrollHeight * minBounds;
        const documentMax = document.body.scrollHeight * maxBounds;
        if (window.scrollY > documentMin && window.scrollY < documentMax) {
          sticky.style.bottom = 0;
        } else if (window.scrollY < documentMin || window.scrollY > documentMax){
          sticky.style.bottom = '-100px';
        }
      }
      
    });
    return () => {
      document.removeEventListener("scroll");
    };
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
    i18n.changeLanguage(control.value);
  };

  return (
    <header className='header'>
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
    </header>
  );
}
