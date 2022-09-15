import { Button, Callout, ControlGroup, FormGroup, HTMLSelect, InputGroup, Radio, RadioGroup, Switch } from '@blueprintjs/core';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import DataGrid from 'react-data-grid';
import { useRecoilValue, useSetRecoilState } from 'recoil';
import { useCloseCurrentPanel } from '../hooks/closeCurrentPanel';
import { DataTablePreviewModel } from '../models';
import { AutoSizeContainer } from '../panels/AutoSizeContainer';
import { dataTablesState } from '../state/dataTablesState';
import { webClientState } from '../state/webClientState';
import { PreviewHeader } from './PreviewHeader';
import './TextFilePreview.scss';

export interface TextFilePreviewProps {
    file         : File;
    previewLines : string[];
    allLines     : string[];
}

export const TextFilePreview = ({file, previewLines, allLines}: TextFilePreviewProps) => {
    const [hasHeader, setHasHeader] = useState(false);
    const [delimeter, setDelimiter] = useState(',');
    const [customDelimeter, setCustomDelimeter] = useState('');
    const [targetColumn, setTargetColumn] = useState<number>(-1);
    const webClient = useRecoilValue(webClientState);
    const [preview, setPreview] = useState<DataTablePreviewModel>();
    const [nameOverride, setNameOverride] = useState(new Map<number, string>());
    const setDataTables = useSetRecoilState(dataTablesState);
    const onClose = useCloseCurrentPanel();

    const getDelimiter = useCallback(() => delimeter.length > 0
        ? delimeter
        : customDelimeter
    , [delimeter, customDelimeter]);

    useEffect(() => {
        webClient.getCSVPreview({
            hasHeader, 
            delimiter: getDelimiter(), 
            lines: previewLines
        }).then(setPreview);
    }, [delimeter, customDelimeter, hasHeader]);

    const columns = useMemo(() => preview?.columns?.map((c, i) => 
        ({ 
            key: `c${i}`, 
            name: c.name, 
            headerCellClass: (i === targetColumn) ? 'table-header target' : 'table-header',
            headerRenderer: (x:any) => {
                const {column} = x;
                const name = nameOverride.get(column.idx) ?? column.name;
                return <PreviewHeader
                    index={column.idx}
                    defaultName={name}
                    onChangeName={name => setNameOverride(x => new Map(x.set(column.idx, name)))}
                />
            }
        })
    ), [preview, nameOverride, targetColumn]);
    const targetColumns = useMemo(() => [
        {label: 'None', value: -1},
        ...(preview?.columns?.map((c, i) => ({label: c.name, value: i})) ?? [])
    ], [preview]);
    const rows = useMemo(() => preview?.previewRows?.map(r => {
        let ret:any = {}, i = 0;
        for(const p of r)
            ret[`c${i++}`] = p;
        return ret;
    }), [preview]);

    let info = `Import from ${file.name}`;
    if(allLines.length > previewLines.length)
        info += ` (previewing ${previewLines.length.toLocaleString()} of ${allLines.length.toLocaleString()} total lines in file)`;

    const onSubmit = useCallback(() => {
        webClient.createCSV({
            columnNames: preview!.columns!.map((x,i) => nameOverride.get(i) ?? x.name).join('|'),
            delimiter: getDelimiter(),
            fileName: file.name,
            hasHeader,
            lines: allLines,
            targetIndex: targetColumn >= 0 ? targetColumn : undefined
        }).then(r => setDataTables(x => [r, ...x]));
        onClose();
    }, [preview, nameOverride, file, hasHeader, allLines, targetColumn]);

    return <div className='text-file-preview'>
        <header>
            <Callout icon="panel-table" intent='primary'>{info}</Callout>
        </header>
        <header>
            <div className="input">
                <label style={{gridRow: 1, gridColumn:1}}>Delimiter</label>
                <ControlGroup className="delimiter">
                    <RadioGroup onChange={e => setDelimiter(e.currentTarget.value)} inline={true} selectedValue={delimeter}>
                        <Radio label="Comma" value=","/>
                        <Radio label="Tab" value="\t"/>
                        <Radio label="Space" value=" "/>
                        <Radio label="Custom" value=""/>
                    </RadioGroup>
                    <InputGroup 
                        id="delimiter"
                        type="text" 
                        maxLength={1} 
                        value={customDelimeter}
                        placeholder="Custom Delimiter"
                        disabled={delimeter !== ''}
                        onChange={e => setCustomDelimeter(e.currentTarget.value)}
                    />
                </ControlGroup>

                <label style={{gridRow: 2, gridColumn:1}}>Target Column</label>
                <HTMLSelect 
                    className='target-column'
                    fill={false}
                    options={targetColumns}
                    value={targetColumn}
                    onChange={e => setTargetColumn(parseInt(e.currentTarget.value, 10))}
                />

                <label style={{gridRow: 3, gridColumn:1}}>Has Header</label>
                <Switch className='has-header' checked={hasHeader} onChange={e => setHasHeader(e.currentTarget.checked)} />

                <div className="button-panel">
                    <Button large={true} onClick={onClose}>Cancel</Button>
                    <Button large={true} intent='primary' onClick={onSubmit}>Import</Button>
                </div>
            </div>
        </header>
        {(rows && columns) 
            ? <AutoSizeContainer>{(width, height) => <DataGrid
                style={{height: height + 'px', width: width + 'px'}}
                columns={columns} 
                rows={rows}
            />}</AutoSizeContainer>
            : undefined
        }
    </div>;
};