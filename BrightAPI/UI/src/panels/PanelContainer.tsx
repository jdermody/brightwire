import { AnchorButton } from '@blueprintjs/core';
import React, { useCallback, useEffect } from 'react';
import { useRecoilState, useRecoilValue, useSetRecoilState } from 'recoil';
import { dataTablesState } from '../state/dataTablesState';
import { activePanelState, currentPanelsState, PanelInfo, previousActivePanel } from '../state/panelState';
import { webClientState } from '../state/webClientState';
import { useLocation, useNavigate } from 'react-router-dom';
import './PanelContainer.scss';

export const PanelContainer = () => {
    const currentPanels = useRecoilValue(currentPanelsState);
    const [activePanel, setActivePanel] = useRecoilState(activePanelState);
    const webClient = useRecoilValue(webClientState);
    const setDataTables = useSetRecoilState(dataTablesState);
    const location = useLocation();
    const navigate = useNavigate();

    // initialise state
    useEffect(() => {
        webClient.getDataTables().then(setDataTables);
    }, []);

    useEffect(() => {
        const locationId = location.hash.substring(1);
        if(activePanel.id != locationId) {
            const panel = currentPanels.find(x => x.id === locationId);
            if(panel) {
                setActivePanel(panel);
            }
        }
    }, [location, activePanel, currentPanels, setActivePanel]);

    const onTabChange = useCallback((newPanel: PanelInfo) => {
        navigate({
            hash: newPanel.id
        }, {
            state: newPanel
        });
        //setActivePanel(newPanel);
    }, [setActivePanel, navigate]);

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