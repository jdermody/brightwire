import React from 'react';

export interface TextFilePreviewProps {
    file  : File;
    lines : string[];
}

export const TextFilePreview = ({file, lines}: TextFilePreviewProps) => {
    return <div className="text-file-preview">
        {file.name}
    </div>;
};