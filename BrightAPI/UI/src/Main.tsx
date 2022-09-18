import React from 'react';
import { createRoot } from 'react-dom/client';
import { RecoilRoot } from 'recoil';
import 'normalize.css/normalize.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import '@blueprintjs/core/lib/css/blueprint.css';
import "@blueprintjs/popover2/lib/css/blueprint-popover2.css";
import './Main.scss';
import { PanelContainer } from './panels/PanelContainer';
import { BrowserRouter } from 'react-router-dom';

const root = createRoot(document.getElementById('main')!);
root.render(<RecoilRoot>
    <BrowserRouter >
        <PanelContainer/>
    </BrowserRouter>
</RecoilRoot>);