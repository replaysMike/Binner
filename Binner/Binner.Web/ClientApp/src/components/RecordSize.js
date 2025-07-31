import { useTranslation } from "react-i18next";
import { Dropdown } from "semantic-ui-react";
import PropTypes from "prop-types";

const defaultRppOptions = [
  { key: 1, text: "10", value: 10 },
  { key: 2, text: "25", value: 25 },
  { key: 3, text: "50", value: 50 },
  { key: 4, text: "100", value: 100 },
  { key: 5, text: "200", value: 200 },
  { key: 6, text: "500", value: 500 },
];

/**
 * Record size selection control
 */
export function RecordSize({ options = defaultRppOptions, value, float, onChange, ...rest }) {
  const { t } = useTranslation();
  return (<div className={`recordsize ${(float?.length > 0 ? 'float-' + float : '')}`}>
    <Dropdown selection options={options} value={value} className="labeled" onChange={onChange} {...rest} />
    <div>
      <span>{t("comp.partsGrid.recordsPerPage", "records per page")}</span>
    </div>
  </div>);
};

RecordSize.propTypes = {
  float: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  options: PropTypes.array
};