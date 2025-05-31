import React, { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Button, Modal } from "semantic-ui-react";
import { DuplicateParts } from "../DuplicateParts";
import PropTypes from "prop-types";

/**
 * Display modal that the user is adding a part already in inventory
 */
export function DuplicatePartModal({ part, duplicateParts, isOpen, onSetPart, onSubmit }) {
  const { t } = useTranslation();
	const [duplicatePartModalOpen, setDuplicatePartModalOpen] = useState(isOpen);

	useEffect(() => {
		setDuplicatePartModalOpen(isOpen);
	}, [isOpen]);

  const handleDuplicatePartModalClose = () => {
    setDuplicatePartModalOpen(false);
  };

  /**
   * Force a save of a possible duplicate part
   * @param {any} e
   */
  const handleForceSubmit = (e) => {
    setDuplicatePartModalOpen(false);
    const updatedPart = { ...part, allowPotentialDuplicate: true };
		if (onSetPart) onSetPart(updatedPart);
		if (onSubmit) onSubmit(e, updatedPart);
  };

  return (
    <Modal centered open={duplicatePartModalOpen} onClose={handleDuplicatePartModalClose}>
			<Modal.Header>{t('comp.duplicatePartModal.title', "Duplicate Part")}</Modal.Header>
			<Modal.Content scrolling>
				<Modal.Description>
					<h3>{t('comp.duplicatePartModal.description', "There is a possible duplicate part already in your inventory.")}</h3>
					<DuplicateParts duplicateParts={duplicateParts} />
				</Modal.Description>
			</Modal.Content>
			<Modal.Actions>
				<Button onClick={handleDuplicatePartModalClose}>{t('button.cancel', "Cancel")}</Button>
				<Button primary onClick={handleForceSubmit}>
				{t('button.addAnyway', "Add Anyway")}
				</Button>
			</Modal.Actions>
		</Modal>
  );
}

DuplicatePartModal.propTypes = {
	part: PropTypes.object.isRequired,
	duplicateParts: PropTypes.array.isRequired,
  isOpen: PropTypes.bool,
	onSetPart: PropTypes.func,
	onSubmit: PropTypes.func
};
