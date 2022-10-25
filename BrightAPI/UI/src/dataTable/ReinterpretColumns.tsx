import React, { useCallback, useMemo, useState } from 'react';
import { ReactSortable } from 'react-sortablejs';
import Sortable from 'sortablejs';
import { data } from 'vis-network';
import { DataTableInfoModel } from '../models';
import './ReinterpretColumns.scss';

export interface ReinterpretColumnsProps {
    dataTable: DataTableInfoModel
}

export const ReinterpretColumns = ({dataTable}: ReinterpretColumnsProps) => {
    const [columns, setColumns] = useState(() => dataTable.columns.map((x, i) => ({
        id: `c${i}`,
        ...x
    })));
    // const createDraggable = useCallback((el: HTMLUListElement) => {
    //     if(el) {
    //         var sortable = Sortable.create(el, {
    //             selectedClass: 'selected',
    //             group: dataTable.id,
    //             ghostClass: 'ghost',
    //             chosenClass: 'chosen',
    //             animation: 150,
    //             fallbackOnBody: true,
    //             swapThreshold: 0.65,
    //         });
    //         return () => {
    //             sortable.destroy();
    //         }
    //     }
    // }, []);

    // return <div className="reinterpret-columns">
    //     <ul ref={createDraggable}>{dataTable.columns.map((x, i) => 
    //         <li key={i}>
    //             {x.name}
    //             <ul ref={createDraggable}></ul>
    //         </li>
    //     )}</ul>
    // </div>;

    return <div className="reinterpret-columns">
        <ReactSortable list={columns} setList={setColumns}>
            {columns.map((item) => (
                <li key={item.id}>
                    {item.name}
                </li>
            ))}
        </ReactSortable>
    </div>;
};