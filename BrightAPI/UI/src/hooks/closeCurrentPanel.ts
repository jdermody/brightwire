import { useCallback } from "react";
import { useRecoilState } from "recoil";
import { panelState, getCurrentSelectedPanel } from "../state/panelState";

export function useCloseCurrentPanel() {
    const [panels, setPanels] = useRecoilState(panelState);

    return useCallback(() => {
        const selectedPanel = getCurrentSelectedPanel(panels);
        setPanels(x => x.filter(y => y !== selectedPanel));
    }, [panels]);
}