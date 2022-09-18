import { selector } from "recoil";
import { DataTableCsvRequest, DataTablePreviewModel, NamedItemModel, DataTableCsvPreviewRequest, DataTableInfoModel, ConvertDataTableColumnsRequest } from "../models";
import { baseUrlState } from "./baseUrlState";
import { authenticatedHeaderState } from "./webClientHeaders";

class WebClient
{
    constructor(public baseUrl: string, private headers: any)
    {
        console.log(headers);
    }

    getCSVPreview(request: DataTableCsvPreviewRequest) {
        return this.postModel<DataTablePreviewModel>(`${this.baseUrl}/datatable/csv/preview`, request);
    }

    createCSV(request: DataTableCsvRequest) {
        return this.postModel<NamedItemModel>(`${this.baseUrl}/datatable/csv`, request);
    }

    getDataTables() {
        return this.getResult<NamedItemModel[]>(`${this.baseUrl}/datatable`);
    }

    getDataTableInfo(id: string) {
        return this.getResult<DataTableInfoModel>(`${this.baseUrl}/datatable/${id}`);
    }

    getDataTableData(id: string, start: number, count: number) {
        return this.getResult<any[][]>(`${this.baseUrl}/datatable/${id}/${start}/${count}`);
    }

    convertDataTable(id: string, request: ConvertDataTableColumnsRequest) {
        return this.postModel<string>(`${this.baseUrl}/datatable/${id}/convert`, request);
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
}

export const webClientState = selector({
    key: 'webClient',
    get: ({get}) => {
        const baseUrl = get(baseUrlState);
        const headers = get(authenticatedHeaderState);
        return new WebClient(baseUrl, headers);
    }
})