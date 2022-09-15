import { Button, Classes, Dialog, Icon } from '@blueprintjs/core';
import React, { useCallback, useRef, useState } from 'react';
import { importFromFile } from './Commands';
import { TextFilePreview, TextFilePreviewProps } from './TextFilePreview';
import './Project.scss';
import { useRecoilState, useResetRecoilState, useSetRecoilState } from 'recoil';
import { activePanelState, currentPanelsState } from '../state/panelState';
import { dataTablesState } from '../state/dataTablesState';
import { DataTable } from '../dataTable/DataTable';
import { useNavigate } from 'react-router-dom';

export const Project = () => {
    const form = useRef<HTMLFormElement>(null);
    const fileInput = useRef<HTMLInputElement>(null);
    const mainContainer = useRef<HTMLDivElement>(null);
    const [currentPanels, setCurrentPanels] = useRecoilState(currentPanelsState);
    const setActivePanel = useSetRecoilState(activePanelState);
    const [dataTables, setDataTables] = useRecoilState(dataTablesState);
    const navigate = useNavigate();

    const openPanel = useCallback((id: string, name: string, content: JSX.Element) => {
        if(!currentPanels.find(x => x.id === id)) {
            setCurrentPanels(x => [...x, {
                canClose: true,
                contents: content,
                id,
                name
            }]);
        }
        navigate({
            hash: id
        });
    }, [currentPanels, setCurrentPanels, setActivePanel]);

    return <div className="project" ref={mainContainer}>
        <form ref={form}>
            <input 
                type="file" 
                ref={fileInput} 
                onChange={e => importFromFile(e.currentTarget, form.current!, (props) => openPanel(`preview:${props.file.name}`, `Import ${props.file.name}`, <TextFilePreview {...props}/>))} 
                multiple 
            />
            <Button icon='document-open' onClick={() => fileInput.current!.click()}>Import...</Button>
        </form>
        <div className="tables">
            <h2>Tables</h2>
            <ul>{dataTables.map(x => 
                <li key={x.id}><a onClick={() => openPanel(x.id, x.name, <DataTable id={x.id}/>)}>{x.name}</a></li>
            )}</ul>
        </div>
    </div>;
};