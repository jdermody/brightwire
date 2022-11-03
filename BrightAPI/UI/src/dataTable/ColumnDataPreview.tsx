import { HTMLTable } from '@blueprintjs/core';
import React from 'react';

export interface ColumnDataPreviewProps {
    preview: string[];
}

export const ColumnDataPreview = ({preview}: ColumnDataPreviewProps) => {
    return <HTMLTable>
        <tbody>{preview.map((v, i) => 
            <tr key={i}><td>{v}</td></tr>
        )}</tbody>
    </HTMLTable>
};