import React from "react";
import { Table } from "semantic-ui-react";
import Visibility from "@semantic-ui-react/component-visibility";

export function InifiniteScrollTable(props) {
	const {headerRow, children, nextPage, ...rest} = props;
	return (
		<Table {...rest}>
			<Table.Header>
				{headerRow}
			</Table.Header>
			<Visibility as="tbody" continuous={false} once={false} onBottomVisible={() => nextPage()}>
				{children}
			</Visibility>
		</Table>
	);
};