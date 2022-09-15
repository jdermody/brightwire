import { Button, EditableText } from '@blueprintjs/core';
import React, { useState } from 'react';

export interface PreviewHeaderProps {
    index: number;
    defaultName: string;
    onChangeName: (newName: string) => void;
}

export const PreviewHeader = ({defaultName, onChangeName}: PreviewHeaderProps) => {
    const [name, setName] = useState(defaultName);
    
    return <EditableText 
        value={name} 
        onChange={newValue => setName(newValue)}
        onCancel={() => setName(defaultName)}
        alwaysRenderInput={true}
        onConfirm={onChangeName}
    />;
};