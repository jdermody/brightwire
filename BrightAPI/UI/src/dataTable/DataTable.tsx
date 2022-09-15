import { HTMLTable, Spinner } from '@blueprintjs/core';
import React, { useEffect, useMemo, useState } from 'react';
import { useRecoilValue } from 'recoil';
import { DataTableGrid } from '../common/DataTableGrid';
import { DataTableInfoModel } from '../models';
import { webClientState } from '../state/webClientState';
import './DataTable.scss';

export const DataTable = ({id}: {id: string}) => {
    const webClient = useRecoilValue(webClientState);
    const [dataTableInfo, setDataTableInfo] = useState<DataTableInfoModel>();

    useEffect(() => {
        webClient.getDataTableInfo(id).then(setDataTableInfo);
    }, [webClient]);

    if(!dataTableInfo)
        return <Spinner/>;

    return <div className="data-table">
        <div className="lhs">
            <HTMLTable>
                <thead>
                    <tr>{dataTableInfo.columns.map((c, i) => 
                        <th key={i}>
                            <div>
                                <span className="type">{c.columnType}</span>
                                <span className="name">{c.name}</span>
                            </div>
                        </th>
                    )}</tr>
                </thead>
                <tbody>
                    <tr>{dataTableInfo.columns.map((c, i) => 
                        <td key={i}>
                            <HTMLTable bordered={false}>
                                <tbody>{c.metadata?.filter(x => x.name != 'Name' && x.name != 'IsTarget' && x.name != 'Type' && x.name != 'HasBeenAnalysed').map(x => 
                                    <tr key={x.name}>
                                        <th>{x.name}</th>
                                        <td>{x.value}</td>
                                    </tr>
                                )}</tbody>
                            </HTMLTable>
                        </td>
                    )}</tr>
                </tbody>
            </HTMLTable>
        </div>
        <div className="rhs">
            <DataTableGrid tableId={id} columns={dataTableInfo.columns} rowCount={dataTableInfo.rowCount}/>
        </div>
    </div>;
}