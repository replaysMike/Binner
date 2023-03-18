import { Link, useNavigate } from "react-router-dom";
import { Icon } from "semantic-ui-react";

export const FormHeader = function (props) {
	const name = props.name;
	const to = props.to;
	const navigate = useNavigate();

	const goBack = (e) => {
		e.preventDefault();
		e.stopPropagation();
		// this is better than Link to=".." as it actually does what we expect
		navigate(-1);
	};

	if (to)
		return (
		<section>
			<h1>{to !== ".." ? <Link to={to}><Icon name="arrow left" color="blue" /></Link> : <a href="/" onClick={goBack}><Icon name="arrow left" color="blue" /></a>} {name}</h1>
			{props.children && <p>{props.children}</p>}
		</section>
		);
	return (<section><h1>{name}</h1>{props.children && <p>{props.children}</p>}</section>);
};