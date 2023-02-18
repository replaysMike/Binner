import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Segment, Breadcrumb, Image } from "semantic-ui-react";
import "./help.css";

export const Scanning = () => {
	const navigate = useNavigate();
	return (
		<div>
			<Breadcrumb>
				<Breadcrumb.Section link onClick={() => navigate('/')}>Home</Breadcrumb.Section>
				<Breadcrumb.Divider />
				<Breadcrumb.Section link onClick={() => navigate('/help')}>Help</Breadcrumb.Section>
				<Breadcrumb.Divider />
				<Breadcrumb.Section active>Barcode Scanning</Breadcrumb.Section>
			</Breadcrumb>
			<h1>Barcode scanning support with Binner</h1>

			<Segment raised>
				<h3>Overview</h3>

				<p>
					If you own a handheld barcode scanner you can utilize it for importing orders, parts or scanning barcodes on your custom part bin labels.<br/>
					Pages that feature this symbol <Image src="/image/barcode.png" width={35} height={35} className="barcode-support" style={{display: 'inline-block', margin: '0 5px'}} /> indicates there are barcode scanning features supported on that page.
					<br/><br/>
					DigiKey and Mouser both label their parts using a combination of 2D (DotMatrix) and 1D (Code-128) barcodes. Each supplier offers different features and integrates directly with Binner.
				</p>

				<div className="imagegroup">
					<div>
						<img src="/image/help/datamatrix.png" alt="2D DotMatrix Barcode" style={{width: '64px', height: 'auto'}} />
						2D DotMatrix Barcode
					</div>
					<div>
						<img src="/image/help/code128.png" alt="1D Code128 Barcode" style={{width: 'auto', height: '64px'}} />
						1D Code-128 Barcode
					</div>
				</div>				

				<Segment>
					<h4>DigiKey</h4>
					
					<p>
						DigiKey uses different types of barcodes for it's various paperwork and labeling. Invoices use a standard code-128 barcode which can be scanned on the Order Import page.<br/><br/>
						Parts use a 2D DotMatrix barcode so you must have a scanner that supports 2D barcodes.
					</p>

					<h5>Scanning Parts</h5>
					By scanning the 2D Dotmatrix barcode on a part label you can:<br/><br/>
					<ul>
						<li>Search for parts in your inventory on the Search page</li>
						<li>Add new parts to your inventory on the Add Inventory page</li>
						<li>Bulk import new parts on the Add Inventory page</li>
					</ul>

					<div className="helpvideo">
						<img src="/image/help/digikey-label.jpg" alt="DigiKey DotMatrix 2D Barcode" style={{width: '400px', height: 'auto'}} />
						Example 1. This is a 2D DotMatrix barcode on a standard DigiKey part label.
					</div>

					<h5>Importing Orders</h5>

					<p>
						On the <Link to="/import">Order Import</Link> page you can scan one of your past orders to import all parts in the order at once.<br/>
						To do so, you will need the DigiKey packing list for your order. The invoice doesn't contain the barcodes you want, however you can type in the Sales Order Number manually easily as well.

					</p>

					<div className="helpvideo">
						<img src="/image/help/digikey-packinglist.jpg" alt="DigiKey DotMatrix 2D Barcode" style={{width: '600px', height: 'auto'}} />
						Example 2. There are multiple 1D code-128 barcodes, and several 2D DotMatrix barcodes on a standard DigiKey packing list.
					</div>

					<p>The Sales Order number can be scanned (top right) to import the entire - <i>or parts of</i> an order. You can also scan the 2D DotMatrix barcodes which provide the same information.</p>
					<p>Additionally, each itemized part can be scanned in the packing list but this is only supported on the Add Inventory page. Note there is a barcode for both the part as well as a smaller barcode for its quantity.</p>

				</Segment>
					
				<Segment>
						<h4>Mouser</h4>
						
						<p style={{textAlign: 'center', margin: '100px'}}>
							Mouser barcode scanning support is <i>coming soon</i>! Look for it in the next feature release.
						</p>

					</Segment>

			</Segment>

		</div>
	);
}
