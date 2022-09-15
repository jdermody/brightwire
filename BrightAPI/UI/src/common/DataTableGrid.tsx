import { IDatasource } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import React, { useMemo, useRef } from 'react';
import { useRecoilValue } from 'recoil';
import { DataTableColumnModel } from '../models';
import { webClientState } from '../state/webClientState';
import './DataTableGrid.scss';

export interface DataGridProps {
    tableId: string;
    rowCount: number;
    columns: DataTableColumnModel[];
}

export const DataTableGrid = ({tableId, columns, rowCount}: DataGridProps) => {
    const gridRef = useRef<AgGridReact>(null);
    const webClient = useRecoilValue(webClientState);

    const dataSource = useMemo(() => {
        return {
            getRows:({startRow, endRow, successCallback, failCallback, sortModel, filterModel}) => {
                webClient.getDataTableData(tableId, startRow, endRow - startRow).then(r => {
                    const rows = r.map(x => {
                        let ret:any = {};
                        let index = 0;
                        for(const y of x)
                            ret[`c${index++}`] = y;
                        return ret;
                    });
                    successCallback(rows);
                });
            },
            rowCount
        }as IDatasource;
    }, []);
    const gridColumns = useMemo(() => columns.map((x,i) => ({
        headerName: x.name,
        field: `c${i}`,

    })), []);

    return (
        <div className="ag-theme-alpine" style={{height:'100%'}}>
            <AgGridReact
                ref={gridRef}
                className='data-table-grid'
                rowModelType ='infinite'
                datasource={dataSource}
                columnDefs={gridColumns}
            />
        </div>
    );
};