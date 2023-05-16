import React from "react";
import { atom, DefaultValue, selector } from "recoil";
import { Project } from '../project/Project';

export interface PanelInfo
{
    id: string;
    name: string;
    contents: JSX.Element;
}

const projectPanel: PanelInfo = {
    id: 'bright-data',
    name: 'Bright Data',
    contents: <Project/>
};

export const previousActivePanelState = atom<PanelInfo[]>({
    key: 'previousActivePanelState',
    default: [projectPanel]
});

export const currentPanelsState = atom<PanelInfo[]>({
    key: 'currentPanelsState',
    default: [projectPanel]
});

export const activePanelState = atom<PanelInfo>({
    key: 'activePanelState',
    default: projectPanel
});