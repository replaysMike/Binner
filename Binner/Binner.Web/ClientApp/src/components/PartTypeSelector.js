import React, { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { Dropdown } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";
import { getIcon } from "../common/partTypes";

import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import TreeView from "@mui/lab/TreeView";
import TreeItem, { treeItemClasses } from "@mui/lab/TreeItem";
import Typography from "@mui/material/Typography";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";
import "./PartTypeSelector.css";

export default function PartTypeSelector(props) {
  const { t } = useTranslation();
  PartTypeSelector.abortController = new AbortController();
  const [partTypes, setPartTypes] = useState(props.partTypes);
	const [partTypesFiltered, setPartTypesFiltered] = useState([]);
	const [partTypeId, setPartTypeId] = useState(0);
  const [partType, setPartType] = useState({ partTypeId: 0, name: ""});
	const [filter, setFilter] = useState('');
	const [expandedNodeIds, setExpandedNodeIds] = useState([]);

	const getPartTypeFromId = useCallback((partTypeId) => {
		let partTypeIdInt = partTypeId;
		if (typeof partTypeId === "string")
			partTypeIdInt = parseInt(partTypeId);
		else if(typeof partTypeId === "object")
			return partTypeId;
		return _.find(partTypes, (i) => i.partTypeId === partTypeIdInt);
	}, [partTypes]);

	const getPartTypeFromName = (name) => {
		const lcName = name.toLowerCase();
		return _.find(partTypes, (i) => i.name.toLowerCase() === lcName)
	};

  useEffect(() => {
    setPartTypes(props.partTypes);
		setPartTypesFiltered(props.partTypes);
  }, [props.partTypes]);

  useEffect(() => {
		const type = typeof props.value;
		let newPartTypeId = 0;
		if (type === "string") {
			newPartTypeId = parseInt(props.value);
		} else if (type === "number") {
			newPartTypeId = props.value;
		} else {
			console.error(`Unknown value type specified: ${props.value} = ${type}`);
			return;
		}
		const newPartType = getPartTypeFromId(newPartTypeId);
		if (newPartTypeId !== 0) {
			setPartTypeId(newPartTypeId);
			setPartType(newPartType);
		}
  }, [props.value, props.partTypes, getPartTypeFromId]);

  const StyledTreeItemRoot = styled(TreeItem)(({ theme }) => ({
    color: theme.palette.text.secondary,
    [`& .${treeItemClasses.content}`]: {
      color: theme.palette.text.secondary,
      borderTopRightRadius: theme.spacing(2),
      borderBottomRightRadius: theme.spacing(2),
      paddingRight: theme.spacing(1),
      fontWeight: theme.typography.fontWeightMedium,
      "&.Mui-expanded": {
        fontWeight: theme.typography.fontWeightRegular
      },
      "&:hover": {
        backgroundColor: theme.palette.action.hover
      },
      "&.Mui-focused, &.Mui-selected, &.Mui-selected.Mui-focused": {
        backgroundColor: `var(--tree-view-bg-color, ${theme.palette.action.selected})`,
        color: "var(--tree-view-color)"
      },
      [`& .${treeItemClasses.label}`]: {
        fontWeight: "inherit",
        color: "inherit"
      }
    },
    [`& .${treeItemClasses.group}`]: {
      marginLeft: 0,
      [`& .${treeItemClasses.content}`]: {
        paddingLeft: theme.spacing(2)
      }
    }
  }));

  const StyledTreeItem = (props) => {
    const { bgColor, color, labelIcon: LabelIcon, labelColor, labelFontWeight, labelInfo, labelText, data, ...other } = props;

    return (
      <StyledTreeItemRoot
        label={
          <Box sx={{ display: "flex", alignItems: "center", p: 0.5, pr: 0 }}>
            <Box component={LabelIcon} color="inherit" sx={{ mr: 1 }} />
            <Typography variant="body2" sx={{ fontWeight: "inherit", flexGrow: 1 }}>
              {labelText}
            </Typography>
            <Typography variant="caption" color={labelColor} sx={{ fontWeight: labelFontWeight }}>
              {labelInfo}
            </Typography>
          </Box>
        }
        style={{
          "--tree-view-color": color,
          "--tree-view-bg-color": bgColor
        }}
        {...other}
      />
    );
  };

	const recursivePreFilter = (partTypes, parentPartTypeId, filterBy) => {
		// go through every child, mark filtered matches

		const filterByLowerCase = filterBy.toLowerCase();
		const childrenComponents = [];
		let partTypesInCategory = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
		for(let i = 0; i < partTypesInCategory.length; i++){
			partTypesInCategory[i].exactMatch = partTypesInCategory[i].name.toLowerCase() === filterByLowerCase;
			if (partTypesInCategory[i].name.toLowerCase().includes(filterByLowerCase)){
				partTypesInCategory[i].filterMatch = true;
			} else {
				partTypesInCategory[i].filterMatch = false;
			}
			childrenComponents.push(partTypesInCategory[i]);

			// now filter the children of this category
			const childs = recursivePreFilter(partTypes, partTypesInCategory[i].partTypeId, filterBy);
			if (_.find(childs, i => i.filterMatch)) {
				// make sure the parent matches the filter because it has children that does
				partTypesInCategory[i].filterMatch = true;
			}
			for(var c = 0; c < childs.length; c++) {
				childrenComponents.push(childs[c]);
			}
		}
		return childrenComponents;
	};

  const recursiveTreeItem = (partTypes, parentPartTypeId = null) => {
    // build a tree graph

    let children = _.filter(partTypes, (i) => i.parentPartTypeId === parentPartTypeId);
		
    const childrenComponents = [];
    if (children && children.length > 0) {
      for (let i = 0; i < children.length; i++) {
        const key = `${children[i].name}-${i}`;
        const nodeId = `${children[i].name}`;
        const childs = recursiveTreeItem(partTypes, children[i].partTypeId);
        childrenComponents.push(
          <StyledTreeItem
            nodeId={nodeId}
            key={key}
            data={children[i]}
            labelText={children[i].name}
            labelIcon={getIcon(children[i].name, children[i].parentPartTypeId && _.find(partTypes, x => x.partTypeId === children[i].parentPartTypeId)?.name)}
            labelInfo={`${children[i].parts}`}
            labelColor={children[i].parts > 0 ? "#1a73e8" : "inherit"}
            labelFontWeight={children[i].parts > 0 ? "700" : "inherit"}
            color="#1a73e8"
            bgColor="#e8f0fe"
          >
            {childs}
          </StyledTreeItem>
        );
      }
    }

    return childrenComponents;
  };

	const handleOnSearchChange = (e, control) => {
		// process keyboard input
		setFilter(control.searchQuery);
		let newPartTypesFiltered = recursivePreFilter(partTypes, null, control.searchQuery.toLowerCase());
    // now remove all part types that don't match the filter
    newPartTypesFiltered = _.filter(partTypes, i => i.filterMatch === true);
    const newPartTypesFilteredOrdered = _.sortBy(newPartTypesFiltered, x => x.exactMatch ? 0 : 1);
    setPartTypesFiltered(newPartTypesFilteredOrdered);
    if (control.searchQuery.length > 1) {
      setExpandedNodeIds(_.map(newPartTypesFiltered, (i) => (i.name)));
    }else{
      setExpandedNodeIds([]);
    }
	};

  const handleOnNodeSelect = (e, selectedPartTypeName) => {
    const selectedPartType = getPartTypeFromName(selectedPartTypeName);
    if (selectedPartType) {
      setPartType(selectedPartType);
      // fire event
			if (props.onSelect) props.onSelect(e, selectedPartType);
    }
  };

  const handleOnNodeToggle = (e, node) => {
    //e.preventDefault();
    //e.stopPropagation();
		// preventing event propagation leads to ui weirdness unfortunately
		if (expandedNodeIds.includes(node))
			setExpandedNodeIds(_.filter(expandedNodeIds, i => i !== node));
		else
			setExpandedNodeIds(node);
  };

  const handleOnBlur = (e, control) => {
		e.stopPropagation();
		if (props.onBlur) props.onBlur(e, control);
  };

  const handleOnFocus = (e, control) => {
		setFilter('');
    if (props.onFocus) props.onFocus(e, control);
  };

	const handleInternalOnBlur = (e, control) => {
		if (props.onBlur) props.onBlur(e, control);
	};

	const handleInternalOnFocus = (e, control) => {
		document.getElementById("partTypeDropdown").firstChild.focus();
    if (props.onFocus) props.onFocus(e, control);
  };

  return (
    <>
			<label>{props.label}</label>
			<div>
				<Dropdown
					id="partTypeDropdown"
					name={props.name || ""} 
					text={partType?.name || ""}
					search 
					floating
					fluid
					placeholder={t('comp.partTypeSelector.choosePartType', "Choose part type")}
					className="selection partTypeSelector"
					onSearchChange={handleOnSearchChange}
					onBlur={handleOnBlur}
					onFocus={handleOnFocus}
				>
					<Dropdown.Menu>
						<Dropdown.Item>
							{/** https://mui.com/material-ui/react-tree-view/ */}
							<TreeView
								defaultCollapseIcon={<ArrowDropDownIcon />}
								defaultExpandIcon={<ArrowRightIcon />}
								defaultEndIcon={<div style={{ width: 24 }} />}
								onNodeSelect={handleOnNodeSelect}
								onNodeToggle={handleOnNodeToggle}
								onBlur={handleInternalOnBlur}
								onFocus={handleInternalOnFocus}
								expanded={expandedNodeIds}
								selected={partType?.name || ""}
								sx={{ flexGrow: 1, maxWidth: "100%" }}
							>
								{recursiveTreeItem(partTypesFiltered).map((x) => x)}
							</TreeView>
						</Dropdown.Item>
					</Dropdown.Menu>
				</Dropdown>
			</div>
    </>
  );
}

PartTypeSelector.propTypes = {
  /** Event handler when selecting a part type */
  onSelect: PropTypes.func.isRequired,
  /** The array of partTypes */
  partTypes: PropTypes.array.isRequired
};
