import { Button, Callout, EditableText } from '@blueprintjs/core';
import React, { useCallback, useMemo, useState } from 'react';
import { ReactSortable } from 'react-sortablejs';
import Sortable from 'sortablejs';
import { BrightDataType, DataTableColumnModel, DataTableInfoModel } from '../models';
import {v4} from 'uuid';
import './ReinterpretColumns.scss';
import { Popover2 } from '@blueprintjs/popover2';
import { ColumnInfo } from './ColumnInfo';
import { Operation } from './DataTable';
import { ColumnDataPreview } from './ColumnDataPreview';

export interface ReinterpretColumnsProps {
    dataTable: DataTableInfoModel;
    preview: string[][];
}

interface Column extends DataTableColumnModel{
    id: string;
    index: number;
}

interface Group {
    id: string;
    name: string;
    type: BrightDataType;
    columns: Column[];
}

const LightColumnInfo = ({column, preview}: {column:Column, preview: string[]}) => {
    return <li className="column">
        {column.name}
        {preview
            ? <Popover2 content={<ColumnDataPreview preview={preview} />}>
                <Button icon="th-list"/>
            </Popover2>
            :undefined
        }
        <Popover2 content={<ColumnInfo column={column} index={0} operation={Operation.None} />}>
            <Button icon="info-sign"/>
        </Popover2>
    </li>;
};

export const ReinterpretColumns = ({dataTable, preview}: ReinterpretColumnsProps) => {
    const [columns, setColumns] = useState<Column[]>(() => dataTable.columns.map((x, i) => ({
        id: `c${i}`,
        index: i,
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
    const deleteGroup = useCallback((group: Group) => {
        setGroups(x => x.filter(y => y.id !== group.id));
        setColumns(x => x.concat(group.columns));
    }, [setGroups]);

    return <div className="reinterpret-columns">
        <Callout icon="info-sign" intent='primary'>
            Create new groups and drag and drop existing table columns into groups to create new columns from multiple input columns.
        </Callout>
        <div className="header">
            <Button onClick={() => addGroup(BrightDataType.Vector, 'Vector')}>Add Vector</Button>
        </div>
        <div className="body">
            <div className="columns">
                <ReactSortable list={columns} setList={setColumns} tag='ul' group='columns'>
                    {columns.map((item, i) => <LightColumnInfo key={item.id} column={item} preview={preview[item.index]}/>)}
                </ReactSortable>
            </div>
            {groups.map(x =>
                <div className="group" key={x.id}>
                    <div className="header">
                        <h2>
                            <EditableText value={x.name} onChange={updateGroupName.bind(null, x)}/>
                        </h2>
                        <Button icon="delete" intent="danger" onClick={() => deleteGroup(x)}/>
                    </div>
                    <ReactSortable list={x.columns} setList={addColumnToGroup.bind(null, x)} tag='ul' group='columns'>
                        {x.columns.map((item) => <LightColumnInfo key={item.id} column={item} preview={preview[item.index]} />)}
                    </ReactSortable>
                </div>
            )}
        </div>
    </div>;
};