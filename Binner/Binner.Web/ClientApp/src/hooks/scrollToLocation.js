import React from "react";
import { useLocation } from "react-router-dom";

/**
 * Scroll to a tag with a matching id, specified on the URL as a # anchor.
 */
export const useScrollToLocation = () => {
  const scrolledRef = React.useRef(false);
  const { hash } = useLocation();
  const hashRef = React.useRef(hash);

  React.useEffect(() => {
    if (hash) {
      // We want to reset if the hash has changed
      if (hashRef.current !== hash) {
        hashRef.current = hash;
        scrolledRef.current = false;
      }

      // only attempt to scroll if we haven't yet (this could have just reset above if hash changed)
      if (!scrolledRef.current) {
        const id = hash.replace('#', '');
        const element = document.getElementById(id);
        if (element) {
          element.scrollIntoView({ behavior: 'smooth' });
          scrolledRef.current = true;
        }
      }
    }
  });
};