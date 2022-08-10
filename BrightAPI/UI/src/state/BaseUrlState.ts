import { atom, selector } from "recoil";

export const baseUrlState = atom({
    key: 'baseUrl',
    default: 'https://localhost:7181'
});

export const urlsState = selector({
    key: 'urls',
    get: ({get}) => {
        const baseUrl = get(baseUrlState);
        return {
            previewFromCsv: `${baseUrl}/datatable/preview/csv`
        };
    }
});