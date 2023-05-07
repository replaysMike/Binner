import React, { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation, Trans } from "react-i18next";
import _ from "underscore";
import { DEFAULT_FONT } from '../common/Types';
import { FormHeader } from "../components/FormHeader";
import { HandleBinaryResponse } from "../common/handleResponse.js";
import { Button, Icon, Form, Input, Checkbox, Table, Image, Dropdown, Breadcrumb } from "semantic-ui-react";
import { fetchApi } from "../common/fetchApi";
import { getImagesToken } from "../common/authentication";
import { LabelEditor } from "./labelEditor/index.js";

export function PrintLabels2(props) {
  const { t } = useTranslation();
  const navigate = useNavigate();

	return (
		<LabelEditor />
	);
};