import { useMemo } from 'react';
import PropTypes from "prop-types";
import "./BulkScanIcon.css";

export function BulkScanIconMemoized ({onClick, width = "132px", height = "30px"}) {
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
		</div>)}, []);
};

BulkScanIconMemoized.propTypes = {
  /** Event handler for onClick */
  onClick: PropTypes.func.isRequired,
	width: PropTypes.string,
	height: PropTypes.string
};
