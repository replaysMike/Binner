import { Icon, Label } from "semantic-ui-react";

// form error component
export const FormError = (errorMessage) => {
	if (errorMessage && errorMessage.length > 0)
		return (<div>
		<Label color="red"><Icon name="warning sign" /> Error: {errorMessage}</Label>
	</div>);
	
	return ("");
};