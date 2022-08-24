import { ControlGroup, InputGroup } from '@blueprintjs/core';
import React, { useMemo, useState } from 'react';
import DataGrid from 'react-data-grid';

export interface TextFilePreviewProps {
    file  : File;
    lines : string[];
}

export const TextFilePreview = ({file, lines}: TextFilePreviewProps) => {
    const [separator, setSeparator] = useState(',');
    const [hasHeader, setHasHeader] = useState(false);

    const columns = useMemo(() => [
        { key: 'id', name: 'ID'},
        { key: 'title', name: 'Title'},
    ], []);
    const rows = [
        { id: 0, title: 'Example' },
        { id: 1, title: 'Demo' }
    ];

    return <div className="text-file-preview">
        <ControlGroup >
            <InputGroup value={separator} onChange={e => setSeparator(e.currentTarget.value)}/>
        </ControlGroup>
        <DataGrid columns={columns} rows={rows} />
    </div>;
};