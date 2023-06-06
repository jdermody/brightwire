import { AnchorButton } from '@blueprintjs/core';
import React, { useCallback, useEffect, useMemo } from 'react';
import { useRecoilState, useRecoilValue, useSetRecoilState } from 'recoil';
import { dataTablesChangeState, dataTablesState } from '../state/dataTablesState';
import { panelState, PanelInfo, getCurrentSelectedPanel, setSelectedPanel } from '../state/panelState';
import { webClientState } from '../state/webClientState';
import { useLocation, useNavigate } from 'react-router-dom';
import './PanelContainer.scss';

export const PanelContainer = () => {
    const [panels, setPanels] = useRecoilState(panelState);
    const webClient = useRecoilValue(webClientState);
    const setDataTables = useSetRecoilState(dataTablesState);
    const dataTablesChange = useRecoilValue(dataTablesChangeState);
    const location = useLocation();
    const navigate = useNavigate();

    // initialise state
    useEffect(() => {
        webClient.getDataTables().then(setDataTables);
    }, [dataTablesChange]);

    // find the current active panel
    const activePanel = useMemo(() => getCurrentSelectedPanel(panels), [panels]);

    // sync state with browser location
    useEffect(() => {
        const locationId = location.hash.substring(1);
        if(activePanel.id != locationId) {
            const panel = panels.find(x => x.id === locationId);
            if(panel) {
                setPanels(setSelectedPanel(panel, panels));
            }
        }
    }, [location, activePanel, panels]);

    // sync tab state with browser location
    const onTabChange = useCallback((newPanel: PanelInfo) => {
        //setActivePanel(newPanel);
        navigate({
            hash: newPanel.id
        }, {
            state: newPanel
        });
    }, [navigate]);

    return <div className="panel-container">
        <header>
            {panels.map(p => 
                <AnchorButton 
                    key={p.id} 
                    className={activePanel === p ? 'selected' : undefined} 
                    onClick={e => onTabChange(p)}
                >{p.name}</AnchorButton>
            )}
        </header>
        <div className="contents">
            {panels.map(p => 
                <main key={p.id} style={{visibility: p === activePanel ? 'visible' : 'hidden'}}>
                    {p.contents}
                </main>
            )}
        </div>
    </div>;
};