import { useNavigate } from "react-router-dom";
import PropTypes from "prop-types";
import { Popup } from "semantic-ui-react";
import { useTranslation, Trans } from "react-i18next";
import { getSubscriptionLevel, handleSubscriptionLink } from "../common/authentication";
import { SubscriptionLevels } from "../common/Types";

export const SubscriptionFeatureLink = ({ requiredSubscriptionLevel, title, description, url }) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const subscriptionLevel = getSubscriptionLevel();

  switch(requiredSubscriptionLevel){
    case SubscriptionLevels.Maker:
      return (<Popup
        hoverable
        content={<p>
          <Trans i18nKey="popup.makerSubscriptionRequired">
            Requires a Maker level subscription. <a href="https://binner.io/signup" target="_blank" rel="noreferrer">Click here</a> for details.
          </Trans></p>}
        trigger={<li onClick={(e) => handleSubscriptionLink(e, SubscriptionLevels.Maker, () => navigate(url))} className={subscriptionLevel >= SubscriptionLevels.Maker ? '' : 'disabled'}>
          <h1 className="maker"><span>{title}</span></h1>
          <p>{description}</p>
        </li>}
      />);
    case SubscriptionLevels.Professional:
      return (<Popup
        hoverable
        content={<p>
          <Trans i18nKey="popup.proSubscriptionRequired">
            Requires a Pro level subscription. <a href="https://binner.io/signup" target="_blank" rel="noreferrer">Click here</a> for details.
          </Trans></p>}
        trigger={<li onClick={(e) => handleSubscriptionLink(e, SubscriptionLevels.Professional, () => navigate(url))} className={subscriptionLevel >= SubscriptionLevels.Professional ? '' : 'disabled'}>
          <h1 className="pro"><span>{title}</span></h1>
          <p>{description}</p>
        </li>}
      />);
  }
};

SubscriptionFeatureLink.propTypes = {
  requiredSubscriptionLevel: PropTypes.any,
  title: PropTypes.string,
  description: PropTypes.string,
  url: PropTypes.string,
};