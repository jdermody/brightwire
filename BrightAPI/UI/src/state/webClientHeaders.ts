import { atom, selector } from "recoil";

const baseHeadersState = atom({
    key: 'baseHeadersState',
    default: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    }
});

export const authenticatedHeaderState = selector({
    key: 'authenticatedHeaderState',
    get: ({get}) => {
        //const userAuth = get(currentAuthState);
        const baseHeaders = get(baseHeadersState);
        return {
            ...baseHeaders,
            //'Authorization': `Bearer ${userAuth}`
        };
    }
});

