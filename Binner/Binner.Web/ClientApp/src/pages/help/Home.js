import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Segment, Breadcrumb } from "semantic-ui-react";
import "./help.css";

export const Help = () => {
	const navigate = useNavigate();
	return (
		<div>
			<Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate('/')}>Home</Breadcrumb.Section>
				<Breadcrumb.Divider />
				<Breadcrumb.Section active>Help</Breadcrumb.Section>
			</Breadcrumb>
			<h1>Help</h1>

            <p>
							Need assistance? Try using our help topics below to solve your issue.
            </p>

			<Segment raised>
				<h3>Choose a Help topic</h3>

                <div className="helpcontainer">
                    <ul>
                        <li><Link to="/help/scanning">Barcode Scanning</Link><p>Learn more about what types of features are available using a handheld barcode scanner.</p></li>
												<li><Link to="/help/api-integrations">Api Integrations</Link><p>Configuring Api integrations are an important part of using Binner effectively.</p></li>
                        <li><a href="https://github.com/replaysMike/Binner/wiki">Wiki</a><p>Get more help from the wiki on GitHub</p></li>
                    </ul>
                </div>
			</Segment>

		</div>
	);
}
