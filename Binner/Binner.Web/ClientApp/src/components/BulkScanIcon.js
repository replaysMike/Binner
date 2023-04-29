import PropTypes from "prop-types";

export function BulkScanIcon({onClick, width, height}) {
	return (
	<div style={{ width: width, height: height, display: "inline-block", cursor: "pointer" }} className="barcodescan" onClick={onClick}>
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
</div>);
}

BulkScanIcon.propTypes = {
  /** Event handler for onClick */
  onClick: PropTypes.func.isRequired,
	width: PropTypes.string,
	height: PropTypes.string
};

BulkScanIcon.defaultProps = {
  width: "132px",
  height: "30px",
};