import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Segment, Breadcrumb, Image } from "semantic-ui-react";
import "./help.css";

export const ApiIntegrations = () => {
	const navigate = useNavigate();
	return (
		<div>
			<Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate('/')}>Home</Breadcrumb.Section>
				<Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate('/help')}>Help</Breadcrumb.Section>
				<Breadcrumb.Divider />
				<Breadcrumb.Section active>Api Integrations</Breadcrumb.Section>
			</Breadcrumb>
			<h1>How to configure API Integrations</h1>

			<Segment raised>
				<h3>Overview</h3>
				<p>
					Binner currently supports Binner Swarm, Digikey, Mouser, Octopart and AliExpress. For standalone installations, you will need to obtain your own API keys to enable these features. It's easy to obtain them but be aware Octopart is not free so you may want to avoid using it. Alternatively you can use Binner.io if you do not wish to worry about configuring external API's.
				</p>
				<p>
					Integrations enable features such as automatic part metadata lookup, datasheet retrieval and automatic importing parts from your orders. To get the best out of Binner, it is a good idea to sign up for Digikey and Mouser API keys at a minimum however they are not required.
				</p>
				<p>
					Configuration values can be found in appsettings.json alongside the Binner executable.
				</p>


				<h3>Configuring Binner Swarm API</h3>

				<p>
					Binner comes with free Swarm API support built-in. Swarm is an aggregate of part information that includes parametrics, datasheets, product images, pinouts and schematics. It is a new service, so content is still being expanded and indexed so a lot more data will be coming in the near future. There is a limit on how many Swarm requests can be made per hour/day - if you feel you need larger limits you can signup for a free or paid account at <a href="https://binner.io/swarm">https://binner.io/swarm</a> and your limits are increased accordingly.
				</p>

				<h4>Settings example</h4>
				<code><pre>// appsettings.json<br/>
{JSON.stringify({ Integrations: {
    Swarm: {
			Enabled: true,
			ApiKey: "",
			ApiUrl: "https://swarm.binner.io",
    },
},}, null, 2)}</pre></code>

				<h3>Configuring DigiKey API</h3>
				<p>
				Visit <a href="https://developer.digikey.com">https://developer.digikey.com</a> and sign up for a free developer account. You will be asked to create an App which will come with a ClientId and ClientSecret and needs to be set in the appsettings.json under the DigiKey configuration section.
				</p>
				
				<h5>Creating an App</h5>

				<div className="bullet">
					The DigiKey Api uses oAuth with postbacks so this must be configured in your DigiKey developer account. DigiKey calls this a <b>Callback URL</b>, while in Binner this is the <b>oAuthPostbackUrl</b>. This can be safely set to <b>https://localhost:8090/Authorization/Authorize</b> in both systems. It is not called by their servers, but rather by the web UI so it does not need to resolve to an external IP and requires no firewall settings. It must be set to exactly the same value on both systems.
					<div className="helpimage">
						<img src="/image/help/digikey-callbackurl.png" alt="DigiKey Callback URL" />
						Figure 1. DigiKey's api settings, located in the DigiKey api <a href="https://developer.digikey.com">developers portal</a>
					</div>
				
				</div>

				<div className="bullet">You will want to enable API access for the Product Information, Order Support, and Barcode Apis.</div>
				
				<h5>Sandbox</h5>

				<p>If you wish to use the DigiKey sandbox rather than their production API, you can specify the ApiUrl to use <b>https://sandbox-api.digikey.com</b>. Otherwise, you can leave it set to <b>https://api.digikey.com</b></p>

				<h4>Settings example</h4>
				<code><pre>// appsettings.json<br/>
{JSON.stringify({ Integrations: {
    Digikey: {
			Enabled: true,
			ClientId: "KsGAFuZGErn4zgvFDI9ux4nW3vZ63H3r",
			ClientSecret: "IAbQsT4GCnagahrH",
			oAuthPostbackUrl: "https://localhost:8090/Authorization/Authorize",
			ApiUrl: "https://api.digikey.com",
    },
},}, null, 2)}</pre></code>

				<h3>Configuring Mouser API</h3>
				<p>Visit <a href="https://www.mouser.com/api-hub/">https://www.mouser.com/api-hub</a> and sign up for a free developer account. Mouser requires you to sign up for each API product you wish to use. Currently, Binner supports both the Search API and Order API so sign up for those two APIs separately. Once you have an API key for each, set those in the <i>appsettings.json</i> under the Mouser configuration section.</p>

				<h4>Settings example</h4>
				<code><pre>// appsettings.json<br/>
{JSON.stringify({ Integrations: {
    Mouser: {
			Enabled: true,
			ApiKeys: {
				SearchApiKey: "84e40c37-99de-4990-86c2-290749dc7f52",
				OrderApiKey: "2d1a00d7-16fd-4979-b7e3-5ca384711ab2",
				CartApiKey: ""
			},
			ApiUrl: "https://api.mouser.com"
    },
},}, null, 2)}</pre></code>

				<h3>Configuring Octopart API</h3>
				<p>Visit <a href="https://octopart.com/api/home">https://octopart.com/api/home</a> and sign up for a developer account. Please note that Octopart API is not free to use so you may opt to skip this one. They don't advertise pricing until you start using the API (sneaky), but if you already have a key it can be used for additional datasheet support. If you do not wish to use it Digikey and Mouser will be used to access datasheets for parts, as well as the free Binner datasheet API.</p>

				<h4>Settings example</h4>
				<code><pre>// appsettings.json<br/>
{JSON.stringify({ Integrations: {
    Octopart: {
			Enabled: true,
			ApiKey: "b3h5632j245jh5521426",
			ApiUrl: "https://octopart.com"
    },
},}, null, 2)}</pre></code>

			</Segment>

		</div>
	);
}
