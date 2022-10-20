import { Button, Classes, Dialog, HTMLTable, Icon } from '@blueprintjs/core';
import React, { useCallback, useRef, useState } from 'react';
import { importFromFile } from './Commands';
import { TextFilePreview, TextFilePreviewProps } from './TextFilePreview';
import './Project.scss';
import { useRecoilState, useRecoilValue, useResetRecoilState, useSetRecoilState } from 'recoil';
import { activePanelState, currentPanelsState } from '../state/panelState';
import { dataTablesState } from '../state/dataTablesState';
import { DataTable } from '../dataTable/DataTable';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns'
import { webClientState } from '../state/webClientState';

export const Project = () => {
    const form = useRef<HTMLFormElement>(null);
    const fileInput = useRef<HTMLInputElement>(null);
    const mainContainer = useRef<HTMLDivElement>(null);
    const [currentPanels, setCurrentPanels] = useRecoilState(currentPanelsState);
    const setActivePanel = useSetRecoilState(activePanelState);
    const [dataTables, setDataTables] = useRecoilState(dataTablesState);
    const webClient = useRecoilValue(webClientState);
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

    function openDataTable(id: string, name: string) {
        openPanel(id, name, <DataTable id={id} openDataTable={openDataTable} />);
    }

    const deleteTable = useCallback((id: string) => {
        webClient.deleteDataTable(id).then(() => {
            setDataTables(x => x.filter(y => y.id !== id));
        });
    }, []);

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
            <HTMLTable className="items">
                <thead>
                    <tr>
                        <th>Data Table</th>
                        <th>Rows</th>
                        <th>Date Created</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>{dataTables.map(x => 
                    <tr key={x.id}>
                        <td className="primary">
                            <a onClick={() => openDataTable(x.id, x.name)}>{x.name}</a>
                        </td>
                        <td>
                            {x.rowCount}
                        </td>
                        <td>
                            {format(new Date(x.dateCreated), 'Pp')}
                        </td>
                        <td className="ctrls">
                            <a onClick={() => deleteTable(x.id)}><Icon icon='delete' intent='danger'/></a>
                        </td>
                    </tr>
                )}</tbody>
            </HTMLTable>
        </div>
    </div>;
};