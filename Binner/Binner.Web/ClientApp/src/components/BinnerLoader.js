import PropTypes from "prop-types";
import "./BinnerLoader.css";

/**
 * Binner animated loader
 * @param {props} props 
 * @returns 
 */
export function BinnerLoader({ text = 'Loading...', color = 'blue', loading = false, opacityLevel = 'heavy', inverted = false, size = 'medium', ...rest }) {

  const getOpacityLevel = (level) => {
    switch (level) {
      case 'light':
      case 'medium':
      case 'heavy':
      case 'block':
        return level;
    }
    return 'medium';
  };

  const getSize = (size) => {
    switch (size) {
      case 'small':
      case 'medium':
      case 'large':
        return size;
    }
    return 'medium';
  };

  const getColor = (color) => {
    switch (color) {
      case 'red':
      case 'green':
      case 'blue':
      case 'none':
        return color;
    }
    return 'none';
  };

  if (loading) {
    return (
      <div className={`loading-container size-${getSize(size)} opacity-${getOpacityLevel(opacityLevel)} color-${getColor(color)} ${inverted ? 'inverted' : ''}`}>
        <div className="loader">
          <div className={`loading-spinner`}>
            <div className="binner">
              <div className="top left" />
              <div className="top middle" />
              <div className="top right" />
              <div className="mid left"></div>
              <div className="mid middle"></div>
              <div className="mid right"></div>
              <div className="bottom left"></div>
              <div className="bottom middle"></div>
              <div className="bottom right"></div>
            </div>
          </div>
          {text && <div className="loading-text">{text}</div>}
        </div>
        <div className="content-container">
          {rest.children}
        </div>
      </div>);
  }

  return rest.children;
};

BinnerLoader.propTypes = {
  /** Text to copy */
  text: PropTypes.string,
  /** Color of loader highlight */
  color: PropTypes.oneOf(['red', 'green', 'blue', 'none']),
  /** If true loading animation will be shown */
  loading: PropTypes.bool,
  /** Size of loader */
  size: PropTypes.oneOf(['small', 'medium', 'large']),
  /** Opacity level of loader */
  opacityLevel: PropTypes.oneOf(['light', 'medium', 'heavy', 'block']),
  /** True to invert colors */
  inverted: PropTypes.bool,
};
