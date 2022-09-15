import { atom } from "recoil";
import { NamedItemModel } from "../models";

export const dataTablesState = atom<NamedItemModel[]>({
    key: 'dataTablesState',
    default: []
});