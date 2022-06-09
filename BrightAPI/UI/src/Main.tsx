import { Button } from '@blueprintjs/core';
import React from 'react';
import { createRoot } from 'react-dom/client';
import { Project } from './project/Project';

export const Main = () => {
    return <React.StrictMode>
        <Project/>
    </React.StrictMode>;
};

const root = createRoot(document.getElementById('main')!);
root.render(<Main/>, );