import React, { useState, useEffect } from 'react';
import { createMedia } from "@artsy/fresnel";
import { Table, Visibility, Input, Label, Button, Confirm, Modal, Header, Dropdown, Pagination, Popup } from 'semantic-ui-react';
import _ from 'underscore';
import { Link } from 'react-router-dom';
import PropTypes from 'prop-types';
import { fetchApi } from '../common/fetchApi';
import { useDropzone } from "react-dropzone";
import { humanFileSize } from "../common/files";

export default function Dropzone(props) {
  const [files, setFiles] = useState([]);
	const [dragOverClass, setDragOverClass] = useState("");

	const { getRootProps, getInputProps, open } = useDropzone({
    maxFiles: 3,
    noClick: true,
    onDrop: (acceptedFiles, rejectedFiles, e) => {
			setDragOverClass("");
      const acceptedMimeTypes = ["application/pdf","image/jpeg","image/png","image/svg+xml","image/webp","image/gif","application/msword","application/vnd.openxmlformats-officedocument.wordprocessingml.document"];
      const filesToUpload = [];
			const errors = [];
      for (let i = 0; i < acceptedFiles.length; i++) {
        if (!acceptedMimeTypes.includes(acceptedFiles[i].type)) {
          errors.push(`File '${acceptedFiles[i].name}' with mime type '${acceptedFiles[i].type}' is not an accepted file type!`);
        }else{
					filesToUpload.push(acceptedFiles[i]);
				}
      }
      if (errors.length > 0) {
				console.error(errors);
				if (props.onError) props.onError(errors);

        setFiles([]);
				setDragOverClass("droptarget-error");
				if (props.stopOnAnyError)
					return;
      } else {
        setFiles(filesToUpload);
      }

      if (filesToUpload.length > 0){
        if (props.onUpload) props.onUpload(filesToUpload, props.type);
      }
			return true;
    },
    onDragEnter: (e) => {
      setDragOverClass("droptarget");
    },
    onDragLeave: (e) => {
      setDragOverClass("");
    },

  });

  const acceptedFileItems = files.map((file) => (
    <li key={file.path}>
      {file.path} - {humanFileSize(file.size, true)} bytes
    </li>
  ));

	return (
		<div key={props.key} {...getRootProps({ className: `dropzone ${dragOverClass}` })}>
			<input {...getInputProps()} />
				{props.children}
		</div>
	);
};

Dropzone.propTypes = {
  /** Callback to load next page */
  onUpload: PropTypes.func,
	/** Called on error */
	onError: PropTypes.func,
	/** Called on error */
	stopOnAnyError: PropTypes.bool,
  /** Root key name */
  key: PropTypes.string,
	/** Type of file being uploaded */
	type: PropTypes.string
};

Dropzone.defaultProps = {
	key: 'dropzone-container',
	type: 'Other',
	stopOnAnyError: true
};