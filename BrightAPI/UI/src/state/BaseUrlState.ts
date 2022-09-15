import { atom, selector } from "recoil";

export const baseUrlState = atom({
    key: 'baseUrl',
    default: 'https://localhost:7181'
});