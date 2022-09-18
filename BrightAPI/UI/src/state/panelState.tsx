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

export let previousActivePanel: PanelInfo[] = [];
export const currentPanelsState = atom<PanelInfo[]>({
    key: 'currentPanelsState',
    default: [projectPanel],
    effects: [
        ({onSet}) => {
            onSet(newValue => {
                var activePanels = new Set(newValue);
                previousActivePanel = previousActivePanel.filter(x => activePanels.has(x));
            });
        }
    ]
});

export const activePanelState = atom<PanelInfo>({
    key: 'activePanelState',
    default: projectPanel,
    effects: [
        ({onSet}) => {
            onSet((newValue, oldValue) => {
                if(oldValue instanceof DefaultValue)
                    previousActivePanel.push(newValue);
                else
                    previousActivePanel.push(oldValue);
            });
        }
    ]
});