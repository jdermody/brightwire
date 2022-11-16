import { Button } from '@blueprintjs/core';
import React, { useState } from 'react';
import { DataTableInfoModel, RangeModel } from '../models';

export interface SelectRowsProps {
    dataTable: DataTableInfoModel;
}

const RowGroup = ({range}: {range: RangeModel}) => {
    return <div>
        {range.firstInclusiveRowIndex} - {range.lastExclusiveRowIndex}
    </div>;
}

export const SelectRows = ({dataTable}: SelectRowsProps) => {
    const [rowGroups, setRowGroups] = useState<RangeModel[]>([{
        firstInclusiveRowIndex: 0,
        lastExclusiveRowIndex: dataTable.rowCount
    }]);

    return <div className="select-rows">
        <div className="header">
            <Button onClick={() => setRowGroups(x => [...x, {firstInclusiveRowIndex: 0, lastExclusiveRowIndex: 0}])}>Add Group</Button>
        </div>
        <div className="body">
            {rowGroups.map((x, i) => <RowGroup key={i} range={x}/>)}
        </div>
    </div>;
};