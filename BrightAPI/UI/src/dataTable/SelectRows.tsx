import { Button, ControlGroup, FormGroup, NumericInput } from '@blueprintjs/core';
import React, { useCallback, useEffect, useState } from 'react';
import { DataTableInfoModel, RangeModel } from '../models';
import './SelectRows.scss';

export interface SelectRowsProps {
    dataTable: DataTableInfoModel;
    onChange: (rowGroups: RangeModel[]) => void;
}

interface RowGroupProps {
    range: RangeModel;
    index: number;
    max: number;
    onChange: (index: number, model: RangeModel) => void;
    onDelete: (index: number) => void;
}

const RowGroup = ({range, index, max, onChange, onDelete}: RowGroupProps) => {
    const [from, setFrom] = useState(range.firstInclusiveRowIndex);
    const [to, setTo] = useState(range.lastExclusiveRowIndex);

    useEffect(() => {
        onChange(index, {firstInclusiveRowIndex: from, lastExclusiveRowIndex: to});
    }, [from, to]);

    return <ControlGroup>
        <FormGroup label="From" labelFor={`from${index}`} inline={true}>
            <NumericInput 
                id={`from${index}`}
                autoFocus={true} 
                type="number" 
                min={0} 
                max={max}
                value={from} 
                onValueChange={x => setFrom(x)}
            />
        </FormGroup>
        <FormGroup label="To" labelFor={`to${index}`} inline={true}>
            <NumericInput 
                id={`to${index}`}
                autoFocus={true} 
                type="number" 
                min={0} 
                max={max}
                value={to} 
                onValueChange={x => setTo(x)}
            />
        </FormGroup>
        <Button icon='delete' intent='danger' onClick={() => onDelete(index)}/>
    </ControlGroup>;
}

export const SelectRows = ({dataTable, onChange}: SelectRowsProps) => {
    const [rowGroups, setRowGroups] = useState<RangeModel[]>([{
        firstInclusiveRowIndex: 0,
        lastExclusiveRowIndex: dataTable.rowCount
    }]);
    const updateRow = useCallback((index: number, model: RangeModel) => {
        setRowGroups(x => x.map((y, i) => i === index ? model : y));
    }, [setRowGroups]);
    const deleteRow = useCallback((index: number) => {
        setRowGroups(x => x.filter((y, i) => i !== index));
    }, [setRowGroups]);
    useEffect(() => {
        onChange(rowGroups);
    }, [rowGroups]);

    return <div className="select-rows">
        <div className="body">
            {rowGroups.map((x, i) => <RowGroup key={i} range={x} index={i} max={dataTable.rowCount} onChange={updateRow} onDelete={deleteRow} />)}
        </div>
        <div className="footer">
            <Button onClick={() => setRowGroups(x => [...x, {firstInclusiveRowIndex: 0, lastExclusiveRowIndex: 0}])}>Add Row Group</Button>
        </div>
    </div>;
};