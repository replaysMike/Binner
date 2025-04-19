import React, { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Table, Dimmer, Loader, Header, Segment, Label } from "semantic-ui-react";
import PropTypes from "prop-types";

/**
 * Lists the recently added parts
 * [memoized]
 */
export function RecentParts({ loadingRecent, recentParts, handleRecentPartClick }) {
  const { t } = useTranslation();

	const renderRecentParts = useMemo(() => {
		return (
			<div style={{ marginTop: "20px" }}>
				<Segment style={{ minHeight: "50px" }} color="teal">
					<Header dividing as="h3">
						{t("page.inventory.recentlyAdded", "Recently Added")}
					</Header>
					<Dimmer active={loadingRecent} inverted>
						<Loader inverted />
					</Dimmer>
					{!loadingRecent && recentParts && (
						<Table compact celled selectable striped>
							<Table.Header>
								<Table.Row>
									<Table.HeaderCell>{t("label.part", "Part")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.quantity", "Quantity")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.partType", "Part Type")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.manufacturerPart", "Manufacturer Part")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.location", "Location")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.binNumber", "Bin Number")}</Table.HeaderCell>
									<Table.HeaderCell>{t("label.binNumber2", "Bin Number 2")}</Table.HeaderCell>
								</Table.Row>
							</Table.Header>
							<Table.Body>
								{recentParts.map((p, index) => (
									<Table.Row key={index} onClick={(e) => handleRecentPartClick(e, p)}>
										<Table.Cell>{index === 0 ? <Label ribbon>{p.partNumber}</Label> : p.partNumber}</Table.Cell>
										<Table.Cell>{p.quantity}</Table.Cell>
										<Table.Cell>{p.partType}</Table.Cell>
										<Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
										<Table.Cell>{p.location}</Table.Cell>
										<Table.Cell>{p.binNumber}</Table.Cell>
										<Table.Cell>{p.binNumber2}</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table>
					)}
				</Segment>
			</div>
		);
	}, [loadingRecent, recentParts]);
  
	return renderRecentParts;
}

RecentParts.propTypes = {
  recentParts: PropTypes.array.isRequired,
  handleRecentPartClick: PropTypes.func.isRequired,
  loadingRecent: PropTypes.bool
};
