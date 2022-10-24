import { Alignment, Button, ButtonGroup, Callout, FormGroup, HTMLTable, InputGroup, Menu, MenuDivider, MenuItem, Navbar, NumericInput, Popover, Position, Spinner } from '@blueprintjs/core';
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
    ConvertColumns,
    NormalizeColumns,
    ReinterpretColumns,
    VectoriseColumns,
    CopyColumns,
    CopyRows,
    Shuffle,
    Bag,
    Split
}

export interface DataTableProps {
    id: string;
    openDataTable: (id: string, name: string) => void;
}

function getOperationUI(operation: Operation) {
    if(operation === Operation.Split) {
        return <Callout icon="info-sign" intent="primary" title="Split Table">
            <p>This will split randomly split the current table into two parts - a training table and a test table.</p>
            <p>The training percentage determines the relative size of the training table.</p>
        </Callout>;
    }else if(operation === Operation.Bag) {
        return <Callout icon="info-sign" intent="primary" title="Bag Table">
            <p>This will randomly select rows from this table into a new table.</p>
            <p>The bag count is the number of rows to select.</p>
        </Callout>;
    }else if(operation === Operation.Shuffle) {
        return <Callout icon="info-sign" intent="primary" title="Shuffle Table">
            <p>This will randomly shuffle the rows from this table into a new table.</p>
            <p>The rows (and row count) will remain the same, only the order within the table will be changed.</p>
        </Callout>;
    }
    return <div>TODO</div>;
}

export const DataTable = ({id, openDataTable}: DataTableProps) => {
    const webClient = useRecoilValue(webClientState);
    const [dataTableInfo, setDataTableInfo] = useState<DataTableInfoModel>();
    const [operation, setOperation] = useState(Operation.None);
    const [operationName, setOperationName] = useState("");
    const setDataTablesChangeState = useSetRecoilState(dataTablesChangeState);
    const columnConversionOptions = useRef(new Map<number, ColumnConversionType>);
    const [preview, setPreview] = useState<any[][]>([]);
    const [splitPercentage, setSplitPercentage] = useState(80);
    const [bagCount, setBagCount] = useState<number>(100);

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
                    <MenuItem 
                        text="Normalize Columns" 
                        onClick={() => startOperation(Operation.NormalizeColumns, "Normalize Columns")}
                    />
                    <MenuItem 
                        text="Reinterpret Columns" 
                        onClick={() => startOperation(Operation.ReinterpretColumns, "Reinterpret Columns")}
                    />
                    <MenuItem 
                        text="Vectorise Columns" 
                        onClick={() => startOperation(Operation.VectoriseColumns, "Vectorise Columns")}
                    />
                    <MenuItem 
                        text="Copy Columns" 
                        onClick={() => startOperation(Operation.CopyColumns, "Copy Columns")}
                    />
                    <MenuItem 
                        text="Copy Rows" 
                        onClick={() => startOperation(Operation.CopyRows, "Copy Rows")}
                    />
                    <MenuItem 
                        text="Shuffle" 
                        onClick={() => startOperation(Operation.Shuffle, "Shuffle")}
                    />
                    <MenuItem 
                        text="Bag" 
                        onClick={() => startOperation(Operation.Bag, "Bag")}
                    />
                    <MenuItem 
                        text="Split" 
                        onClick={() => startOperation(Operation.Split, "Split")}
                    />
                </Menu>} position={Position.BOTTOM_LEFT}>
                    <Button icon="flows" text="Transform" />
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
                    {operation === Operation.Split
                        ? <FormGroup label="Training Percentage" labelFor="traning-percentage" inline={true}>
                            <NumericInput 
                                id="traning-percentage" 
                                autoFocus={true} 
                                type="number" 
                                min={1} 
                                max={99}
                                value={splitPercentage} 
                                onChange={e => setSplitPercentage(e.currentTarget.valueAsNumber)}
                            />
                        </FormGroup>
                        : undefined
                    }
                    {operation === Operation.Bag
                        ? <FormGroup label="Bag Count" labelFor="bag-count" inline={true}>
                        <NumericInput 
                            id="bag-count" 
                            autoFocus={true} 
                            type="number" 
                            value={bagCount} 
                            onChange={e => setBagCount(e.currentTarget.valueAsNumber)}
                        />
                    </FormGroup>
                        : undefined
                    }
                    <ButtonGroup style={{marginLeft:'1em'}}>
                        <Button onClick={(() => setOperation(Operation.None))}>Cancel</Button>
                        <Button intent='primary' onClick={completeOperation}>Okay</Button>
                    </ButtonGroup>
                </Navbar.Group>
            </Navbar>}
        </div>
        <div className="body">
            <div className="lhs">
                {operation === Operation.Bag || operation === Operation.CopyRows || operation === Operation.ReinterpretColumns || operation === Operation.Shuffle || operation === Operation.Split
                    ? getOperationUI(operation)
                    : dataTableInfo.columns.map((c, i) => 
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