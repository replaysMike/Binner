import React, { useMemo } from "react";
import { Form, Popup, Header } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";

/**
 * Custom Field Values control
 * @param {props} props 
 * @returns 
 */
export function CustomFieldValues({ customFieldDefinitions, customFieldValues, header = "Custom Fields", headerElement = "h3", onChange }) {
  
  const renderCustomFields = useMemo(() => {
    return (
      <div className="customFieldValues">
        {header.length > 0 &&
          <Header dividing as={headerElement}>{header}</Header>
        }
        <Form.Group>
          {customFieldDefinitions.map((customFieldDefinition, fieldKey) => (
            <Popup
              key={fieldKey}
              content={customFieldDefinition.description}
              trigger={<Form.Input
                label={customFieldDefinition.name}
                value={_.find(customFieldValues, x => x.field === customFieldDefinition.name)?.value || ''}
                name={customFieldDefinition.name}
                onChange={(e, control) => onChange(e, control, _.find(customFieldValues, x => x.field === customFieldDefinition.name), customFieldDefinition)}
              />}
            />
          ))}
        </Form.Group>
      </div>
    );
  }, [customFieldDefinitions, customFieldValues]);
  
  // render memoized
  if (customFieldDefinitions?.length > 0)
    return renderCustomFields;

  return (<></>);
};

CustomFieldValues.propTypes = {
  /** array of custom fields */
  customFieldDefinitions: PropTypes.array,
  /** array of custom field values */
  customFieldValues: PropTypes.array,
  header: PropTypes.string,
  headerElement: PropTypes.string,
  /** callback to handle value change */
  onChange: PropTypes.func
};
