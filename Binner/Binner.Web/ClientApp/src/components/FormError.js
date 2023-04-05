import { Icon, Label } from "semantic-ui-react";
import { useTranslation } from 'react-i18next';

// form error component
export const FormError = (errorMessage) => {
	const { t } = useTranslation();
	if (errorMessage && errorMessage.length > 0)
		return (<div>
		<Label color="red"><Icon name="warning sign" /> {t('label.error', "Error")}: {errorMessage}</Label>
	</div>);
	
	return ("");
};