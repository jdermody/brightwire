import { useCallback } from "react";
import { useRecoilCallback, useRecoilState, useRecoilValue, useSetRecoilState } from "recoil";
import { activePanelState, currentPanelsState, previousActivePanelState } from "../state/panelState";

export function useCloseCurrentPanel() {
    const setCurrentPanels = useSetRecoilState(currentPanelsState);
    const [activePanel, setActivePanel] = useRecoilState(activePanelState);
    const [previousActivePanel, setPreviousActivePanel] = useRecoilState(previousActivePanelState);

    const setActivePanelToLastUsedPanel = useRecoilCallback(({snapshot}) => async () => {
        const previousActivePanel = await snapshot.getPromise(previousActivePanelState);
        setActivePanel(previousActivePanel[previousActivePanel.length-1]);
    }, []);

    return useCallback(() => {
        console.log(activePanel);
        setCurrentPanels(x => x.filter(y => y !== activePanel));
        setPreviousActivePanel(x => x.filter(y => y !== activePanel));
        setActivePanelToLastUsedPanel();
    }, [setCurrentPanels, setActivePanel, previousActivePanel]);
}