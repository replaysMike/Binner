import React, { useState, useEffect, useCallback, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Dropdown, Icon } from "semantic-ui-react";
import PropTypes from "prop-types";
import _ from "underscore";
import { getIcon } from "../common/partTypes";
import { styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import { TreeView } from "@mui/x-tree-view";
import { TreeItem, treeItemClasses } from "@mui/x-tree-view";
import Typography from "@mui/material/Typography";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";
import "./PartTypeSelector.css";

/**
 * Part type selector dropdown (treeview with icons)
 * [memoized]
 */
export default function PartTypeSelectorMemoized({ partTypes, loadingPartTypes, label, name, value, onSelect, onBlur, onFocus }) {
  const { t } = useTranslation();
  PartTypeSelectorMemoized.abortController = new AbortController();
  const [internalPartTypes, setInternalPartTypes] = useState(partTypes);
  const [internalPartTypesFiltered, setInternalPartTypesFiltered] = useState([]);
  const [partTypeId, setPartTypeId] = useState(0);
  const [partType, setPartType] = useState();
  const [filter, setFilter] = useState('');
  const [expandedNodeIds, setExpandedNodeIds] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const getPartTypeFromId = useCallback((partTypeId) => {
    let partTypeIdInt = partTypeId;
    if (typeof partTypeId === "string")
      partTypeIdInt = parseInt(partTypeId);
    else if (typeof partTypeId === "object")
      return partTypeId;
    return _.find(internalPartTypes, (i) => i.partTypeId === partTypeIdInt);
  }, [internalPartTypes]);

  useEffect(() => {
    setInternalPartTypes(partTypes);
    setInternalPartTypesFiltered(partTypes);
  }, [partTypes]);

  useEffect(() => {
    setLoading(loadingPartTypes);
  }, [loadingPartTypes]);

  useEffect(() => {
    const type = typeof value;
    let newPartTypeId = 0;
    if (type === "string") {
      newPartTypeId = parseInt(value);
    } else if (type === "number") {
      newPartTypeId = value;
    } else {
      console.error(`Unknown value type specified: ${value} = ${type}`);
      return;
    }
    const newPartType = getPartTypeFromId(newPartTypeId);
    if (newPartTypeId !== 0) {
      setPartTypeId(newPartTypeId);
      setPartType(newPartType);
    }
  }, [value, partTypes, getPartTypeFromId]);

  const StyledTreeItemRoot = styled(TreeItem)(({ theme }) => ({
    color: theme.palette.text.secondary,
    [`& .${treeItemClasses?.content}`]: {
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
      [`& .${treeItemClasses?.label}`]: {
        fontWeight: "inherit",
        color: "inherit"
      }
    },
    [`& .${treeItemClasses?.group}`]: {
      marginLeft: 0,
      [`& .${treeItemClasses?.content}`]: {
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

  const getSelectedText = (partType) => {
    if (partType) {
      return partType?.name || "";
    }
    return "";
  };

  const handleClear = (e) => {
    setPartTypeId(0);
    setPartType();
    setSearchQuery('');
    setFilter('');
    setExpandedNodeIds([]);
    setInternalPartTypesFiltered([...internalPartTypes]);
  };

  const render = useMemo(() => {
    const getPartTypeFromName = (name) => {
      const lcName = name.toLowerCase();
      return _.find(internalPartTypes, (i) => i.name.toLowerCase() === lcName)
    };

    const recursivePreFilter = (allPartTypes, parentPartTypeId, filterBy) => {
      // go through every child, mark filtered matches

      const filterByLowerCase = filterBy.toLowerCase();
      const childrenComponents = [];
      let partTypesInCategory = _.filter(allPartTypes, (i) => i.parentPartTypeId === parentPartTypeId);
      for (let i = 0; i < partTypesInCategory.length; i++) {
        partTypesInCategory[i].exactMatch = partTypesInCategory[i].name.toLowerCase() === filterByLowerCase;
        if (partTypesInCategory[i].name.toLowerCase().includes(filterByLowerCase)) {
          partTypesInCategory[i].filterMatch = true;
        } else {
          partTypesInCategory[i].filterMatch = false;
        }
        childrenComponents.push(partTypesInCategory[i]);

        // now filter the children of this category
        const childs = recursivePreFilter(allPartTypes, partTypesInCategory[i].partTypeId, filterBy);
        if (_.find(childs, i => i.filterMatch)) {
          // make sure the parent matches the filter because it has children that does
          partTypesInCategory[i].filterMatch = true;
        }
        for (var c = 0; c < childs.length; c++) {
          childrenComponents.push(childs[c]);
        }
      }
      return childrenComponents;
    };

    const recursiveTreeItem = (allPartTypes, parentPartTypeId = null) => {
      // build a tree graph

      let children = _.filter(allPartTypes, (i) => i.parentPartTypeId === parentPartTypeId);

      const childrenComponents = [];
      if (children && children.length > 0) {
        for (let i = 0; i < children.length; i++) {
          const key = `${children[i].name}-${i}`;
          const nodeId = `${children[i].name}`;
          const childs = recursiveTreeItem(allPartTypes, children[i].partTypeId);
          const basePartTypeName = _.find(allPartTypes, x => x.partTypeId === children[i].parentPartTypeId)?.name;
          const partTypeName = children[i].name;
          const partTypeIcon = children[i].icon;
          childrenComponents.push(
            <StyledTreeItem
              nodeId={nodeId}
              key={key}
              data={children[i]}
              labelText={partTypeName}
              labelIcon={() => getIcon(partTypeName, basePartTypeName, partTypeIcon)({ className: `parttype parttype-${basePartTypeName || partTypeName}` })}
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
      setSearchQuery(control.searchQuery);
      // process keyboard input
      setFilter(control.searchQuery);
      let newPartTypesFiltered = recursivePreFilter(internalPartTypes, null, control.searchQuery.toLowerCase());
      // now remove all part types that don't match the filter
      newPartTypesFiltered = _.filter(internalPartTypes, i => i.filterMatch === true);
      const newPartTypesFilteredOrdered = _.sortBy(newPartTypesFiltered, x => x.exactMatch ? 0 : 1);
      setInternalPartTypesFiltered(newPartTypesFilteredOrdered);
      if (control.searchQuery.length > 1) {
        setExpandedNodeIds(_.map(newPartTypesFiltered, (i) => (i.name)));
      } else {
        setExpandedNodeIds([]);
      }
    };

    const handleOnNodeSelect = (e, selectedPartTypeName) => {
      const selectedPartType = getPartTypeFromName(selectedPartTypeName);
      if (selectedPartType) {
        setPartType(selectedPartType);
        setSearchQuery(selectedPartTypeName);
        // fire event
        if (onSelect) onSelect(e, selectedPartType);
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
      if (onBlur) onBlur(e, control);
      // reset the search filtering
      setFilter(null);
      setExpandedNodeIds([]);
      setInternalPartTypesFiltered([...internalPartTypes]);
    };

    const handleOnFocus = (e, control) => {
      setFilter('');
      if (onFocus) onFocus(e, control);
    };

    const handleInternalOnBlur = (e, control) => {
      if (onBlur) onBlur(e, control);
    };

    const handleInternalOnFocus = (e, control) => {
      document.getElementById("partTypeDropdown").firstChild.focus();
      if (onFocus) onFocus(e, control);
    };

    const getSelectedIcon = (partType) => {
      if (partType) {
        const basePartTypeName = partType?.parentPartTypeId && _.find(internalPartTypes, x => x.partTypeId === partType?.parentPartTypeId)?.name;
        const partTypeName = partType?.name;
        const partTypeIcon = partType?.icon;
        return (partType && getIcon(partType?.name, basePartTypeName, partTypeIcon)({ className: `parttype parttype-${basePartTypeName || partTypeName}` }));
      }
      return "";
    };

    return (
      <div className="partTypeSelector-container">
        <div className="icon">{getSelectedIcon(partType)}</div>
        <Dropdown
          id="partTypeDropdown"
          name={name || ""}
          text={getSelectedText(partType)}
          // searchQuery allows us to force set the search text when node changes, due to weird behavior in semantic's dropdown
          searchQuery={searchQuery}
          search
          floating
          fluid
          placeholder={t('comp.partTypeSelector.choosePartType', "Choose part type")}
          className="selection partTypeSelector"
          onSearchChange={handleOnSearchChange}
          onBlur={handleOnBlur}
          onFocus={handleOnFocus}
          disabled={loading}
          loading={loading}
        >
          <Dropdown.Menu>
            <Dropdown.Item>
              {/** https://mui.com/material-ui/react-tree-view/ */}
              <TreeView
                className="partTypeSelectorTreeView"
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
                {recursiveTreeItem(internalPartTypesFiltered).map((x) => x)}
              </TreeView>
            </Dropdown.Item>
          </Dropdown.Menu>
        </Dropdown>
        {partType && partType?.partTypeId > 0 && <div style={{ position: 'absolute', right: '10px', top: '6px', zIndex: '2', padding: '2px', backgroundColor: '#fff', cursor: 'pointer' }} onClick={handleClear}>
          <Icon name="times" circular link size="small" className="clearIcon" style={{ opacity: '0.5', padding: '0', margin: '0', lineHeight: '1em', fontSize: '0.6em' }} />
        </div>}
      </div>);
  }, [partType, internalPartTypes, internalPartTypesFiltered, expandedNodeIds, loading]);

  return (
    <>
      <label>{label}</label>
      {render}
    </>
  );
}

PartTypeSelectorMemoized.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.any.isRequired,
  /** Event handler when selecting a part type */
  onSelect: PropTypes.func.isRequired,
  /** The array of partTypes */
  partTypes: PropTypes.array.isRequired,
  loadingPartTypes: PropTypes.bool,
  label: PropTypes.string,
  onBlur: PropTypes.func,
  onFocus: PropTypes.func
};
