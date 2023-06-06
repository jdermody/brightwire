import React from "react";
import { atom, DefaultValue, selector } from "recoil";
import { Project } from '../project/Project';

export interface PanelInfo
{
    id: string;
    name: string;
    contents: JSX.Element;
    selectionSequence: number;
    canClose: boolean;
}

let currentSelectionCadence = 0;
let getNextSelectionSequence = () => ++currentSelectionCadence;
export var getCurrentSelectedPanel = (panels: PanelInfo[]) => {
    let ret = panels[0];
    for(let i = 1, len = panels.length; i < len; i++) {
        const panel = panels[i];
        if(panel.selectionSequence > ret.selectionSequence) {
            ret = panel;
        }
    }
    return ret;
};
export var addNewPanel = (id: string, name: string, contents: JSX.Element, panels: PanelInfo[]) => {
    return [...panels, {
        canClose: true,
        contents,
        id,
        name,
        selectionSequence: getNextSelectionSequence()
    }]
};
export var setSelectedPanel = (panel: PanelInfo, panels: PanelInfo[]) => panels.map(x => (x === panel) ? {...x, selectionSequence: getNextSelectionSequence()} : x);

const projectPanel: PanelInfo = {
    id: 'bright-data',
    name: 'Bright Data',
    selectionSequence: getNextSelectionSequence(),
    canClose: false,
    contents: <Project/>
};

export const panelState = atom<PanelInfo[]>({
    key: 'panelState',
    default: [projectPanel]
});