import { Alignment, Button, ButtonGroup, HTMLTable, Menu, MenuDivider, MenuItem, Navbar, Popover, Position, Spinner } from '@blueprintjs/core';
import { Popover2 } from '@blueprintjs/popover2';
import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useRecoilValue, useSetRecoilState } from 'recoil';
import { DataTableGrid } from '../common/DataTableGrid';
import { ColumnConversionType, DataTableInfoModel } from '../models';
import { dataTablesChangeState } from '../state/dataTablesState';
import { webClientState } from '../state/webClientState';
import { ColumnInfo } from './ColumnInfo';
import './DataTable.scss';

export const enum Operation{
    None = 0,
    ConvertColumns
}

export interface DataTableProps {
    id: string;
    openDataTable: (id: string, name: string) => void;
}

export const DataTable = ({id, openDataTable}: DataTableProps) => {
    const webClient = useRecoilValue(webClientState);
    const [dataTableInfo, setDataTableInfo] = useState<DataTableInfoModel>();
    const [operation, setOperation] = useState(Operation.None);
    const [operationName, setOperationName] = useState("");
    const setDataTablesChangeState = useSetRecoilState(dataTablesChangeState);
    const columnConversionOptions = useRef(new Map<number, ColumnConversionType>);
    const [preview, setPreview] = useState<any[][]>([]);

    useEffect(() => {
        webClient.getDataTableInfo(id).then(setDataTableInfo);
    }, [webClient]);
    const startOperation = useCallback((operation: Operation, name: string) => {
        setOperation(operation);
        setOperationName(name);
    }, [setOperation]);
    const completeOperation = useCallback(() => {
        if(operation === Operation.ConvertColumns) {
            const table = columnConversionOptions.current;
            const columnIndices: Array<number> = [];
            const columnConversions: Array<ColumnConversionType> = [];
            for(const kv of table.entries()) {
                columnIndices.push(kv[0]);
                columnConversions.push(kv[1]);
            }
            webClient.convertDataTable(id, {
                columnConversions,
                columnIndices
            }).then(id => {
                setDataTablesChangeState(x => x+1);
                openDataTable(id.id, id.name);
            });
        }
    }, [operation]);

    if(!dataTableInfo)
        return <Spinner/>;

    return <div className="data-table">
        <div className="header">
            {operation === Operation.None ? <Navbar>
                <Navbar.Group align={Alignment.LEFT}>
                <Popover2 content={<Menu>
                    <MenuItem 
                        text="Convert Columns" 
                        onClick={() => startOperation(Operation.ConvertColumns, "Convert Columns")}
                    />
                </Menu>} position={Position.BOTTOM_LEFT}>
                    <Button icon="flows" text="Modify" />
                </Popover2>
                </Navbar.Group>
                <Navbar.Group align={Alignment.RIGHT}>
                    <Button icon="cross" large={false} onClick={() => {
                        // setIsOpen(false);
                    }} />
                </Navbar.Group>
            </Navbar> : <Navbar>
                <Navbar.Group align={Alignment.LEFT}>
                    <strong>{operationName}</strong>
                    <ButtonGroup style={{marginLeft:'1em'}}>
                        <Button onClick={(() => setOperation(Operation.None))}>Cancel</Button>
                        <Button intent='primary' onClick={completeOperation}>Okay</Button>
                    </ButtonGroup>
                </Navbar.Group>
            </Navbar>}
        </div>
        <div className="body">
            <div className="lhs">{dataTableInfo.columns.map((c, i) => 
                <ColumnInfo 
                    key={i} 
                    column={c} 
                    index={i} 
                    operation={operation}
                    onChangeColumnType={(index, type) => columnConversionOptions.current.set(index, type)}
                    preview={preview.map(x => x[i])}
                />
            )}
            </div>
            <div className="rhs">
                <DataTableGrid 
                    tableId={id} 
                    columns={dataTableInfo.columns} 
                    rowCount={dataTableInfo.rowCount} 
                    onDataPreview={setPreview} 
                />
            </div>
        </div>
    </div>;
}