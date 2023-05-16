import { selector } from "recoil";
import { DataTableCsvRequest, DataTablePreviewModel, NamedItemModel, DataTableCsvPreviewRequest, DataTableInfoModel, ConvertDataTableColumnsRequest, DataTableListItemModel, NormalizeDataTableColumnsRequest, ReinterpretDataTableColumnsRequest, VectoriseDataTableColumnsRequest, DataTableColumnsRequest, DataTableRowsRequest, BagTableRequest, SplitTableRequest, RenameTableColumnsRequest, SetColumnTargetRequest } from "../models";
import { baseUrlState } from "./BaseUrlState";
import { authenticatedHeaderState } from "./webClientHeaders";

class WebClient
{   
    constructor(public baseUrl: string, private headers: any)
    {
    }

    getCSVPreview(request: DataTableCsvPreviewRequest) {
        return this.postModel<DataTablePreviewModel>(`${this.baseUrl}/datatable/csv/preview`, request);
    }

    createCSV(request: DataTableCsvRequest) {
        return this.postModel<DataTableListItemModel>(`${this.baseUrl}/datatable/csv`, request);
    }

    getDataTables() {
        return this.getResult<DataTableListItemModel[]>(`${this.baseUrl}/datatable`);
    }

    getDataTableInfo(id: string) {
        return this.getResult<DataTableInfoModel>(`${this.baseUrl}/datatable/${id}`);
    }

    getDataTableData(id: string, start: number, count: number) {
        return this.getResult<any[][]>(`${this.baseUrl}/datatable/${id}/${start}/${count}`);
    }

    convertDataTable(id: string, request: ConvertDataTableColumnsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/convert`, request);
    }

    deleteDataTable(id: string) {
        return this.delete(`${this.baseUrl}/datatable/${id}`);
    }

    normalizeDataTable(id: string, request: NormalizeDataTableColumnsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/normalize`, request);
    }

    reinterpretDataTable(id: string, request: ReinterpretDataTableColumnsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/reinterpret`, request);
    }

    vectoriseDataTable(id: string, request: VectoriseDataTableColumnsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/vectorise`, request);
    }

    copyDataTableColumns(id: string, request: DataTableColumnsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/copy-columns`, request);
    }

    copyDataTableRows(id: string, request: DataTableRowsRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/copy-rows`, request);
    }
    
    shuffleDataTable(id: string) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/shuffle`, {});
    }

    bagDataTable(id: string, request: BagTableRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/${id}/bag`, request);
    }

    splitDataTable(id: string, request: SplitTableRequest) {
        return this.postModel<NamedItemModel[]>(`${this.baseUrl}/datatable/${id}/split`, request);
    }

    renameDataTableColumns(id: string, request: RenameTableColumnsRequest) {
        return this.postModel<DataTableInfoModel>(`${this.baseUrl}/datatable/${id}/rename-columns`, request);
    }

    setDataTableTargetColumn(id: string, request: SetColumnTargetRequest) {
        return this.postModel<DataTableInfoModel>(`${this.baseUrl}/datatable/${id}/set-target`, request);
    }

    async getResult<RT>(url: string): Promise<RT>
    {
        const r = await fetch(url, {headers: this.headers});
        return await r.json();
    }

    async postModel<RT>(url: string, model: any): Promise<RT>
    {
        const r = await fetch(url, {
            method: 'POST',
            headers: this.headers,
            body: JSON.stringify(model)
        });
        return await r.json();
    }

    async postText<RT>(url: string, text: string): Promise<RT>
    {
        const r = await fetch(url, {
            method: 'POST',
            headers: {
                ...this.headers,
                'Content-Type': 'text/plain',
            },
            body: text
        });
        return await r.json();
    }

    async delete(url: string): Promise<string> {
        const r = await fetch(url, {
            method: 'DELETE',
            headers: this.headers
        });
        return await r.text();
    }
}

export const webClientState = selector({
    key: 'webClient',
    get: ({get}) => {
        const baseUrl = get(baseUrlState);
        const headers = get(authenticatedHeaderState);
        return new WebClient(baseUrl, headers);
    }
})