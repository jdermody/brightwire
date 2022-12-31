import { Alignment, Button, ButtonGroup, Callout, FormGroup, HTMLTable, InputGroup, Menu, MenuDivider, MenuItem, Navbar, NumericInput, Popover, Position, Spinner } from '@blueprintjs/core';
import { Popover2 } from '@blueprintjs/popover2';
import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useRecoilValue, useSetRecoilState } from 'recoil';
import { DataTableGrid } from '../common/DataTableGrid';
import { Splitter } from '../common/Splitter';
import { useCloseCurrentPanel } from '../hooks/closeCurrentPanel';
import { ColumnConversionType, DataTableInfoModel, NamedItemModel, NewColumnFromExistingColumnsModel, NormalizationType, RangeModel } from '../models';
import { dataTablesChangeState } from '../state/dataTablesState';
import { webClientState } from '../state/webClientState';
import { ColumnInfo } from './ColumnInfo';
import './DataTable.scss';
import { ReinterpretColumns, ReinterpretColumnsProps } from './ReinterpretColumns';
import { SelectRows, SelectRowsProps } from './SelectRows';

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

function getOperationUI(
    operation            : Operation, 
    dataTableInfo        : DataTableInfoModel, 
    preview              : string[][], 
    onChangeRowGroups    : SelectRowsProps["onChange"],
    onChangeColumnGroups : ReinterpretColumnsProps["onChange"]
) {
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
    }else if(operation === Operation.ReinterpretColumns) {
        return <ReinterpretColumns dataTable={dataTableInfo} preview={preview} onChange={onChangeColumnGroups} />;
    }else if(operation === Operation.CopyRows) {
        return <SelectRows dataTable={dataTableInfo} onChange={x => {
            onChangeRowGroups(x);
        }} />;
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
    const columnNormalizationOptions = useRef(new Map<number, NormalizationType>);
    const columnSelection = useRef(new Map<number, boolean>);
    const [preview, setPreview] = useState<any[][]>([]);
    const [splitPercentage, setSplitPercentage] = useState(80);
    const [bagCount, setBagCount] = useState<number>(100);
    const [canCompleteOperation, setCanCompleteOperation] = useState(true);
    const [rowGroups, setRowGroups] = useState<RangeModel[]>([]);
    const [columnGroups, setColumnGroups] = useState<NewColumnFromExistingColumnsModel[]>([]);
    const onClose = useCloseCurrentPanel();

    useEffect(() => {
        webClient.getDataTableInfo(id).then(setDataTableInfo);
    }, [webClient]);

    const columnPreviews = useMemo(() => {
        const ret: string[][] = [];
        if(dataTableInfo && preview.length) {
            for(let i = 0; i < dataTableInfo.columns.length; i++) {
                ret.push(preview.map(x => x[i]));
            }
        }
        return ret;
    }, [preview, dataTableInfo]);
    
    const startOperation = (operation: Operation, name: string) => {
        setOperation(operation);
        setOperationName(name);
        setCanCompleteOperation(operation !== Operation.ConvertColumns && operation !== Operation.NormalizeColumns);
    };

    const addTable = (newTable: NamedItemModel) => {
        setDataTablesChangeState(x => x+1);
        openDataTable(newTable.id, newTable.name);
    };

    const completeOperation = () => {
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
            }).then(addTable);
        }else if(operation === Operation.NormalizeColumns) {
            const table = columnNormalizationOptions.current;
            const columnIndices: Array<number> = [];
            const columns: Array<NormalizationType> = [];
            for(const kv of table.entries()) {
                columnIndices.push(kv[0]);
                columns.push(kv[1]);
            }
            webClient.normalizeDataTable(id, {
                columns,
                columnIndices
            }).then(addTable);
        }else if(operation === Operation.VectoriseColumns || operation === Operation.CopyColumns) {
            const table = columnSelection.current;
            const columnIndices: Array<number> = [];
            for(let i = 0; i < (dataTableInfo?.columns ?? []).length; i++) {
                if(table.get(i))
                    columnIndices.push(i);
            }
            if(columnIndices.length) {
                if(operation === Operation.VectoriseColumns) {
                    webClient.vectoriseDataTable(id, {
                        columnIndices,
                        oneHotEncodeToMultipleColumns: true
                    }).then(addTable);
                }else {
                    webClient.copyDataTableColumns(id, {
                        columnIndices
                    }).then(addTable);
                }
            }
        }else if(operation === Operation.Split) {
            webClient.splitDataTable(id, {
                trainingPercentage: splitPercentage
            }).then((tables: NamedItemModel[]) => {
                setDataTablesChangeState(x => x+1);
                openDataTable(tables[0].id, tables[0].name);
                openDataTable(tables[1].id, tables[1].name);
            });
        }else if(operation === Operation.Bag) {
            webClient.bagDataTable(id, {
                rowCount: bagCount
            }).then(addTable);
        }else if(operation === Operation.Shuffle) {
            webClient.shuffleDataTable(id).then(addTable);
        }else if(operation === Operation.CopyRows) {
            webClient.copyDataTableRows(id, {rowRanges: rowGroups}).then(addTable);
        }else if(operation === Operation.ReinterpretColumns) {
            webClient.reinterpretDataTable(id, {columns: columnGroups}).then(addTable);
        }
        setOperation(Operation.None);
    };

    const renameColumn = (index: number, newName: string) => {
        webClient.renameDataTableColumns(id, {
            columnIndices: [index],
            columnsNames: [newName]
        }).then(setDataTableInfo);;
    };

    const setTargetColumn = (index: number) => {
        webClient.setDataTableTargetColumn(id, {
            targetColumn: index
        }).then(setDataTableInfo);
    };

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
                        text="Vectorise Columns" 
                        onClick={() => startOperation(Operation.VectoriseColumns, "Vectorise Columns")}
                    />
                    <MenuItem 
                        text="Reinterpret Columns" 
                        onClick={() => startOperation(Operation.ReinterpretColumns, "Reinterpret Columns")}
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
                    <div className="info">{dataTableInfo.rowCount.toLocaleString()} row{dataTableInfo.rowCount > 1 ? 's' : ''}</div>
                    <Button icon="cross" large={false} onClick={onClose} />
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
                                onValueChange ={e => {
                                    console.log(e);
                                    setSplitPercentage(e);
                                }}
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
                            min={1}
                            onValueChange={e => setBagCount(e)}
                        />
                    </FormGroup>
                        : undefined
                    }
                    <ButtonGroup style={{marginLeft:'1em'}}>
                        <Button onClick={(() => setOperation(Operation.None))}>Cancel</Button>
                        <Button intent='primary' onClick={completeOperation}/* disabled={!canCompleteOperation}*/>Okay</Button>
                    </ButtonGroup>
                </Navbar.Group>
            </Navbar>}
        </div>
        <Splitter
            first={operation === Operation.Bag || operation === Operation.CopyRows || operation === Operation.ReinterpretColumns || operation === Operation.Shuffle || operation === Operation.Split
                ? getOperationUI(operation, dataTableInfo, columnPreviews, setRowGroups, setColumnGroups)
                : dataTableInfo.columns.map((c, i) => 
                <ColumnInfo 
                    key={i} 
                    column={c} 
                    index={i} 
                    operation={operation}
                    onChangeColumnType={(index, type) => {
                        columnConversionOptions.current.set(index, type);
                        setCanCompleteOperation(Array.from(columnConversionOptions.current.values()).some(x => x !== ColumnConversionType.Unchanged));
                    }}
                    onChangeColumnNormalization={(index, type) => {
                        columnNormalizationOptions.current.set(index, type);
                        setCanCompleteOperation(Array.from(columnNormalizationOptions.current.values()).some(x => x !== NormalizationType.None));
                    }}
                    onChangeColumnSelection={(index, isSelected) => {
                        columnSelection.current.set(index, isSelected);
                        setCanCompleteOperation(Array.from(columnSelection.current.values()).some(x => x === true));
                    }}
                    onRenameColumn={renameColumn}
                    onSetTargetColumn={setTargetColumn}
                    preview={columnPreviews[i]}
                />
            )}
            second={<DataTableGrid 
                tableId={id} 
                columns={dataTableInfo.columns} 
                rowCount={dataTableInfo.rowCount} 
                onDataPreview={setPreview} 
            />}
        />
    </div>;
}