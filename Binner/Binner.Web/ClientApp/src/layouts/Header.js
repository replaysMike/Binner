import { useEffect } from 'react';
import { NavMenu } from "./NavMenu";
import { ThemeChangeToggle } from '../components/ThemeChangeToggle';
import { LanguageSelector } from '../components/LanguageSelector';

/**
 * Global Header
 */
export function Header() {
  useEffect(() => {
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

  

  return (
    <header className='header'>
      <ThemeChangeToggle />
      <LanguageSelector />
      <NavMenu />
    </header>
  );
}
