import { HTMLTable } from '@blueprintjs/core';
import React from 'react';

export interface PropertyListProps {
    properties: {name: string, value: string}[];
}

export const PropertyList = ({properties}: PropertyListProps) => {
    return <HTMLTable className="property-list">
        <tbody>{properties.map((p, i) => <tr key={i}>
            <th className="name">{p.name}</th>
            <td className="value">{p.value}</td>
        </tr>)}
        </tbody>
    </HTMLTable>;
};