import React, { useEffect, useState, useMemo, useCallback } from "react";
import { Form, Popup, Header } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";

/**
 * Custom Field Values control
 * @param {CustomFieldTypes} type 
 * @param {array} customFieldDefinitions List of all custom field definitions
 * @param {array} customFieldValues List of custom values for the current type
 * @param {string} header The header to display, "Custom Fields"
 * @param {string} headerElement The header element type, "h3"
 * @returns 
 */
export function CustomFieldValues({ type, customFieldDefinitions, customFieldValues: fieldValue, header = "Custom Fields", headerElement = "h3", onChange }) {

  return (
    <div className="customFieldValues">
      {header?.length > 0 && _.filter(customFieldDefinitions, i => i.customFieldTypeId === type.value)?.length > 0 &&
        <Header dividing as={headerElement}>{header}</Header>
      }
      <Form.Group>
        {_.filter(customFieldDefinitions, i => i.customFieldTypeId === type.value).map((fieldDefinition, fieldKey) => (
          <Popup
            key={fieldKey}
            content={fieldDefinition.description}
            trigger={<Form.Input
              key={fieldDefinition.name}
              label={fieldDefinition.name}
              value={_.find(fieldValue, x => x.field === fieldDefinition.name)?.value || ''}
              name={fieldDefinition.name}
              onChange={(e, control) => onChange(e, control, _.find(fieldValue, x => x.field === fieldDefinition.name), fieldDefinition)}
            />}
          />
        ))}
      </Form.Group>
    </div>
  ); 
};

CustomFieldValues.propTypes = {
  type: PropTypes.object,
  /** array of custom fields */
  customFieldDefinitions: PropTypes.array,
  /** array of custom field values */
  customFieldValues: PropTypes.array,
  header: PropTypes.string,
  headerElement: PropTypes.string,
  /** callback to handle value change */
  onChange: PropTypes.func
};
