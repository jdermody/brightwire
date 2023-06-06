import { ColDef, IDatasource } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import React, { useMemo, useRef } from 'react';
import { useRecoilValue } from 'recoil';
import { BrightDataType, DataTableColumnModel } from '../models';
import { webClientState } from '../state/webClientState';
import './DataTableGrid.scss';

export interface DataGridProps {
    tableId: string;
    rowCount: number;
    columns: DataTableColumnModel[];
    onDataPreview: (preview: any[][]) => void;
}

export const DataTableGrid = ({tableId, columns, rowCount, onDataPreview}: DataGridProps) => {
    const gridRef = useRef<AgGridReact>(null);
    const webClient = useRecoilValue(webClientState);

    const dataSource = useMemo(() => {
        return {
            getRows:({startRow, endRow, successCallback, failCallback, sortModel, filterModel}) => {
                webClient.getDataTableData(tableId, startRow, endRow - startRow).then(r => {
                    if(startRow === 0) {
                        onDataPreview(r.slice(0, 5));
                    }

                    const rows = r.map(x => {
                        let ret:any = {};
                        let index = 0;
                        for(const y of x)
                            ret[`c${index++}`] = y;
                        return ret;
                    });
                    successCallback(rows, rowCount);
                });
            },
            rowCount
        }as IDatasource;
    }, []);
    const gridColumns = useMemo(() => columns.map((x,i) => {
        let ret: ColDef<any> = {
            headerName: x.name,
            field: `c${i}`
        };
        // if(x.columnType === BrightDataType.Vector) {
        //     ret.valueFormatter = e => e.value ? e.value.segment.values : '-'
        // }
        return ret;
    }), [columns]);

    return (
        <div className="ag-theme-alpine" style={{height:'100%'}}>
            <AgGridReact
                ref={gridRef}
                className='data-table-grid'
                rowModelType ='infinite'
                datasource={dataSource}
                columnDefs={gridColumns}
                infiniteInitialRowCount={rowCount}
            />
        </div>
    );
};