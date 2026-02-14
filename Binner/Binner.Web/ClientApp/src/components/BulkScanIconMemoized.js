import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import PropTypes from "prop-types";
import "./BulkScanIcon.css";

export function BulkScanIconMemoized ({onClick, width = "132px", height = "30px"}) {
  const { t } = useTranslation();
	return useMemo(() => {
		return (
		<div style={{ width: width, height: height }} className="barcodescan" onClick={onClick}>
			<div className="anim-box">
				<div className="scanner" />
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-md"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-md"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-lg"></div>
				<div className="anim-item anim-item-sm"></div>
				<div className="anim-item anim-item-md"></div>
			</div>
        <div className="description">{t('page.inventory.popup.bulkUpdateViaBarcode', "Add/Update via Barcode")}</div>
		</div>)}, []);
};

BulkScanIconMemoized.propTypes = {
  /** Event handler for onClick */
  onClick: PropTypes.func.isRequired,
	width: PropTypes.string,
	height: PropTypes.string
};
