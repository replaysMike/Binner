import { Trans, useTranslation } from "react-i18next";
import { Outlet } from "react-router-dom";
// components

export function RootLayout(props) {
  const { t } = useTranslation('en');
  const noop = t('noop', "-do-not-translate-");

  return (
      <div>
          <Outlet />
          {props.children}
      </div>
    );
}