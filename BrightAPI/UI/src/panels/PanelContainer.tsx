import { AnchorButton } from '@blueprintjs/core';
import React, { useCallback, useEffect } from 'react';
import { useRecoilState, useRecoilValue, useSetRecoilState } from 'recoil';
import { dataTablesChangeState, dataTablesState } from '../state/dataTablesState';
import { activePanelState, currentPanelsState, PanelInfo, previousActivePanelState } from '../state/panelState';
import { webClientState } from '../state/webClientState';
import { useLocation, useNavigate } from 'react-router-dom';
import './PanelContainer.scss';

export const PanelContainer = () => {
    const currentPanels = useRecoilValue(currentPanelsState);
    const [activePanel, setActivePanel] = useRecoilState(activePanelState);
    const [previousActivePanel, setPreviousActivePanel] = useRecoilState(previousActivePanelState);
    const webClient = useRecoilValue(webClientState);
    const setDataTables = useSetRecoilState(dataTablesState);
    const dataTablesChange = useRecoilValue(dataTablesChangeState);
    const location = useLocation();
    const navigate = useNavigate();

    // initialise state
    useEffect(() => {
        webClient.getDataTables().then(setDataTables);
    }, [dataTablesChange]);

    // sync state with browser location
    useEffect(() => {
        const locationId = location.hash.substring(1);
        if(activePanel.id != locationId) {
            const panel = currentPanels.find(x => x.id === locationId);
            if(panel) {
                setActivePanel(panel);
            }
        }
    }, [location, activePanel, currentPanels, setActivePanel]);

    // sync tab state with browser location
    const onTabChange = useCallback((newPanel: PanelInfo) => {
        setPreviousActivePanel(x => [...x, newPanel]);
        navigate({
            hash: newPanel.id
        }, {
            state: newPanel
        });
        //setActivePanel(newPanel);
    }, [setActivePanel, navigate, setPreviousActivePanel]);

    return <div className="panel-container">
        <header>
            {currentPanels.map(p => 
                <AnchorButton 
                    key={p.id} 
                    className={activePanel === p ? 'selected' : undefined} 
                    onClick={e => onTabChange(p)}
                >{p.name}</AnchorButton>
            )}
        </header>
        <div className="contents">
            {currentPanels.map(p => 
                <main key={p.id} style={{visibility: p === activePanel ? 'visible' : 'hidden'}}>
                    {p.contents}
                </main>
            )}
        </div>
    </div>;
};