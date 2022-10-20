import { atom } from "recoil";
import { DataTableListItemModel, NamedItemModel } from "../models";

export const dataTablesState = atom<DataTableListItemModel[]>({
    key: 'dataTablesState',
    default: []
});

export const dataTablesChangeState = atom<number>({
    key: 'dataTablesChangeState',
    default: 0
});