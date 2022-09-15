import { useCallback } from "react";
import { useRecoilState, useSetRecoilState } from "recoil";
import { activePanelState, currentPanelsState, previousActivePanel } from "../state/panelState";

export function useCloseCurrentPanel() {
    const setCurrentPanels = useSetRecoilState(currentPanelsState);
    const [activePanel, setActivePanel] = useRecoilState(activePanelState);

    return useCallback(() => {
        setCurrentPanels(x => x.filter(y => y !== activePanel));
        setActivePanel(previousActivePanel[previousActivePanel.length-1]);
    }, [setCurrentPanels, setActivePanel, previousActivePanel]);
}