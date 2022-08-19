import { Button } from '@blueprintjs/core';
import React from 'react';
import { createRoot } from 'react-dom/client';
import { RecoilRoot } from 'recoil';
import { Project } from './project/Project';
import 'normalize.css/normalize.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import '@blueprintjs/core/lib/css/blueprint.css';
import './Main.scss';

export const Main = () => {
    return <Project/>;
};

const root = createRoot(document.getElementById('main')!);
root.render(<RecoilRoot>
    <Main/>
</RecoilRoot>);