import React  from "react";
import { useTranslation } from "react-i18next";
import { Image, Button, Table } from "semantic-ui-react";
import { MountingTypes, GetTypeName } from "../common/Types";
import ReactDOM from "react-dom";
import PropTypes from "prop-types";

export function DuplicateParts({ duplicateParts }) {
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

  return (
    <Table compact celled selectable size="small" className="partstable">
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>{t('label.partNumber', "Part Number")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.manufacturerPart', "Manufacturer Part")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.manufacturer', "Manufacturer")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.description', "Description")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.partType', "Part Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.location', "Location")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber', "Bin Number")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.binNumber2', "Bin Number 2")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.mountingType', "Mounting Type")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.image', "Image")}</Table.HeaderCell>
            <Table.HeaderCell>{t('label.datasheet', "Datasheet")}</Table.HeaderCell>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {duplicateParts.map((p, index) => (
            <Table.Row key={index}>
              <Table.Cell>{p.partNumber}</Table.Cell>
              <Table.Cell>{p.manufacturerPartNumber}</Table.Cell>
              <Table.Cell>{p.manufacturer}</Table.Cell>
              <Table.Cell>{p.description}</Table.Cell>
              <Table.Cell>{p.partType}</Table.Cell>
              <Table.Cell>{p.location}</Table.Cell>
              <Table.Cell>{p.binNumber}</Table.Cell>
              <Table.Cell>{p.binNumber2}</Table.Cell>
              <Table.Cell>{GetTypeName(MountingTypes, p.mountingTypeId)}</Table.Cell>
              <Table.Cell>
                <Image src={p.imageUrl} size="mini"></Image>
              </Table.Cell>
              <Table.Cell>
                <Button onClick={(e) => handleHighlightAndVisit(e, p.datasheetUrl)}>{t('button.viewDatasheet', "View Datasheet")}</Button>
              </Table.Cell>
            </Table.Row>
          ))}
        </Table.Body>
      </Table>
  );
}

DuplicateParts.propTypes = {
  duplicateParts: PropTypes.array,
};
