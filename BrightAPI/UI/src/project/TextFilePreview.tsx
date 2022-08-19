import React, { useMemo } from 'react';
import DataGrid from 'react-data-grid';

export interface TextFilePreviewProps {
    file  : File;
    lines : string[];
}

export const TextFilePreview = ({file, lines}: TextFilePreviewProps) => {
    const columns = useMemo(() => [
        { key: 'id', name: 'ID'},
        { key: 'title', name: 'Title'},
    ], []);
    const rows = [
        { id: 0, title: 'Example' },
        { id: 1, title: 'Demo' }
    ];

    return <div className="text-file-preview">
        <DataGrid columns={columns} rows={rows} />
    </div>;
};