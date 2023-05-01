import React, { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Image, Button, Table, Popup, Label } from "semantic-ui-react";
import ReactDOM from "react-dom";
import PropTypes from "prop-types";
import { MountingTypes, GetTypeName } from "../common/Types";

/**
 * Show all matching parts. todo: is this used for anything anymore? doesn't look like it!
 * [memoized]
 */
export function MatchingPartsMemoized({part, metadataParts, setPartFromMetadata, partTypes}) {
  const { t } = useTranslation();

	const handleVisitLink = (e, url) => {
    e.preventDefault();
    e.stopPropagation();
    window.open(url, "_blank");
  };

	const handleHighlightAndVisit = (e, url) => {
    handleVisitLink(e, url);
    // this handles highlighting of parent row
    const parentTable = ReactDOM.findDOMNode(e.target).parentNode.parentNode.parentNode;
    const targetNode = ReactDOM.findDOMNode(e.target).parentNode.parentNode;
    for (let i = 0; i < parentTable.rows.length; i++) {
      const row = parentTable.rows[i];
      if (row.classList.contains("positive")) row.classList.remove("positive");
    }
    targetNode.classList.toggle("positive");
  };

	const handleChooseAlternatePart = (e, part) => {
		if (setPartFromMetadata)
    	setPartFromMetadata(metadataParts, part);
  };

	const renderAllMatchingParts = useMemo(() => {
		return (
			<Table compact celled selectable size="small" className="partstable">
				<Table.Header>
					<Table.Row>
						<Table.HeaderCell>{t('label.part', "Part")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.supplier', "Supplier")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.supplierType', "Supplier Type")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.packageType', "Package Type")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.mountingType', "Mounting Type")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.cost', "Cost")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
						<Table.HeaderCell>{t('label.datasheet', "Datasheet")}</Table.HeaderCell>
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{metadataParts.map((p, index) => (
						<Popup
							key={index}
							content="This is a test"
							trigger={
								<Table.Row onClick={(e) => handleChooseAlternatePart(e, p, partTypes)}>
									<Table.Cell>
										{part.supplier === p.supplier && part.supplierPartNumber === p.supplierPartNumber ? (
											<Label ribbon>{p.manufacturerPartNumber}</Label>
										) : (
											p.manufacturerPartNumber
										)}
									</Table.Cell>
									<Table.Cell>{p.manufacturer}</Table.Cell>
									<Table.Cell>{p.partType}</Table.Cell>
									<Table.Cell>{p.supplier}</Table.Cell>
									<Table.Cell>{p.supplierPartNumber}</Table.Cell>
									<Table.Cell>{p.packageType}</Table.Cell>
									<Table.Cell>{GetTypeName(MountingTypes, p.mountingTypeId)}</Table.Cell>
									<Table.Cell>{p.cost}</Table.Cell>
									<Table.Cell>
										<Image src={p.imageUrl} size="mini"></Image>
									</Table.Cell>
									<Table.Cell>
										{p.datasheetUrls.map(
											(d, dindex) =>
												d &&
												d.length > 0 && (
													<Button key={dindex} onClick={(e) => handleHighlightAndVisit(e, d)}>
														{t('button.viewDatasheet', "View Datasheet")}
													</Button>
												)
										)}
									</Table.Cell>
								</Table.Row>
							}
						/>
					))}
				</Table.Body>
			</Table>
		);
	}, [part, metadataParts]);

  return renderAllMatchingParts;
}

MatchingPartsMemoized.propTypes = {
	part: PropTypes.object.isRequired,
  metadataParts: PropTypes.array.isRequired,
	partTypes: PropTypes.array.isRequired,
	setPartFromMetadata: PropTypes.func,
};

MatchingPartsMemoized.defaultProps = {};
