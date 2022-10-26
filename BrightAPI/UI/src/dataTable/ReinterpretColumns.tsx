import { Button, EditableText } from '@blueprintjs/core';
import React, { useCallback, useMemo, useState } from 'react';
import { ReactSortable } from 'react-sortablejs';
import Sortable from 'sortablejs';
import { BrightDataType, DataTableColumnModel, DataTableInfoModel } from '../models';
import {v4} from 'uuid';
import './ReinterpretColumns.scss';
import { Popover2 } from '@blueprintjs/popover2';
import { ColumnInfo } from './ColumnInfo';
import { Operation } from './DataTable';

export interface ReinterpretColumnsProps {
    dataTable: DataTableInfoModel
}

interface Column extends DataTableColumnModel{
    id: string;
}

interface Group {
    id: string;
    name: string;
    type: BrightDataType;
    columns: Column[];
}

const LightColumnInfo = ({column}: {column:Column}) => {
    return <li className="column">
        {column.name}
        <Popover2 content={<ColumnInfo column={column} index={0} operation={Operation.None} />}>
            <Button icon="info-sign"/>
        </Popover2>
    </li>;
};

export const ReinterpretColumns = ({dataTable}: ReinterpretColumnsProps) => {
    const [columns, setColumns] = useState<Column[]>(() => dataTable.columns.map((x, i) => ({
        id: `c${i}`,
        ...x
    })));
    const [groups, setGroups] = useState<Group[]>([]);
    const addGroup = useCallback((type: BrightDataType, name: string) => {
        setGroups(x => [...x, {id: v4(), type, name, columns: []}]);
    }, [setGroups]);
    const addColumnToGroup = useCallback((group: Group, columns: Column[]) => {
        setGroups(x => x.map(y => y.id === group.id ? {...group, columns} : y));
    }, [setGroups]);
    const updateGroupName = useCallback((group: Group, name: string) => {
        setGroups(x => x.map(y => y.id === group.id ? {...group, name} : y));
    }, [setGroups]);

    return <div className="reinterpret-columns">
        <div className="header">
            <Button onClick={() => addGroup(BrightDataType.Vector, 'Vector')}>Add Vector</Button>
        </div>
        <div className="body">
            <div className="columns">
                <h2>Columns</h2>
                <ReactSortable list={columns} setList={setColumns} tag='ul' group='columns'>
                    {columns.map((item) => <LightColumnInfo key={item.id} column={item}/>)}
                </ReactSortable>
            </div>
            {groups.map(x =>
                <div className="group" key={x.id}>
                    <EditableText value={x.name} onChange={updateGroupName.bind(null, x)}/>
                    <ReactSortable list={x.columns} setList={addColumnToGroup.bind(null, x)} tag='ul' group='columns'>
                        {x.columns.map((item) => <LightColumnInfo key={item.id} column={item}/>)}
                    </ReactSortable>
                </div>
            )}
        </div>
    </div>;
};