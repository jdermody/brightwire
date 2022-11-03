import { Button, Callout, Checkbox, Classes, HTMLSelect, HTMLTable, Icon, Overlay, RadioGroup, Switch } from '@blueprintjs/core';
import React, { useEffect, useMemo, useState } from 'react';
import { Histogram } from '../charts/Histogram';
import { formatName, simplify, getDataTypeName } from '../common/FormatUtils';
import { FullScreenPopup } from '../common/FullScreenPopup';
import { formatMetaData, getAllMetaData, getName, isNumeric } from '../common/MetaDataUtil';
import { PropertyList } from '../common/PropertyList';
import { BrightDataType, ColumnConversionType, DataTableColumnModel, NameValueModel, NormalizationType } from '../models';
import { AutoSizeContainer } from '../panels/AutoSizeContainer';
import { Operation } from './DataTable';
import './ColumnInfo.scss';
import { Popover2 } from '@blueprintjs/popover2';
import { ColumnDataPreview } from './ColumnDataPreview';

export interface ColumnInfoProps {
    operation: Operation;
    column: DataTableColumnModel;
    index: number;
    onChangeColumnType?: (columnIndex: number, type: ColumnConversionType) => void;
    onChangeColumnNormalization?: (columnIndex: number, normalization: NormalizationType) => void;
    preview?: string[];
}

export const invalidHistogramColumnTypes = new Set<BrightDataType>([
    BrightDataType.IndexList,
    BrightDataType.WeightedIndexList,
    BrightDataType.Vector,
    BrightDataType.Matrix,
    BrightDataType.Tensor3D,
    BrightDataType.Tensor4D,
    BrightDataType.Date
]);

const invalidColumns = new Set<string>([
    'Name',
    'Index',
    'Type',
    'HasBeenAnalysed',
    'IsNumeric',
    'IsTarget',
    'Cluster Index'
]);

function getHistogram(metaData: NameValueModel[], width?: number, height?: number,) {
    const numDistinct = metaData.filter(d => d.name === 'NumDistinct');
    let forceFrequency = false;
    if(numDistinct?.length && +numDistinct[0].value <= 10)
        forceFrequency = true;

    const frequency = formatMetaData(getAllMetaData('Frequency:', metaData));
    const frequencyRange = formatMetaData(getAllMetaData('FrequencyRange:', metaData));
    return <Histogram 
        canvasWidth={width}
        canvasHeight={height}
        frequency={(frequencyRange.length && !forceFrequency) ? frequencyRange : frequency}
    />;
}

function* getConversionOptions(dataType: BrightDataType) {
    yield {label: 'Unchanged', value: ColumnConversionType.Unchanged}

    if(dataType === BrightDataType.String) {
        yield {label: 'To Number', value: ColumnConversionType.ToNumeric};
        yield {label: 'To Date', value: ColumnConversionType.ToDate};
        yield {label: 'To Categorical Index', value: ColumnConversionType.ToCategoricalIndex};
        yield {label: 'To Boolean', value: ColumnConversionType.ToBoolean};
    }else {
        yield {label: 'To String', value: ColumnConversionType.ToString};
    }
}

function* getSecondaryConversionOptions(dataType: BrightDataType) {
    if(dataType === BrightDataType.String) {
        yield {label: 'To Float', value: ColumnConversionType.ToFloat};
        yield {label: 'To Double', value: ColumnConversionType.ToDouble};
        yield {label: 'To Int', value: ColumnConversionType.ToInt};
        yield {label: 'To Long', value: ColumnConversionType.ToLong};
        yield {label: 'To Byte', value: ColumnConversionType.ToByte};
        yield {label: 'To Short', value: ColumnConversionType.ToShort};
        yield {label: 'To Decimal', value: ColumnConversionType.ToDecimal};
    }
}

function* getNormalizationOptions(dataType: BrightDataType) {
    
    if(isNumeric(dataType)) {
        yield { label: 'None', value: NormalizationType.None};
        yield { label: 'Euclidean', value: NormalizationType.Euclidean};
        yield { label: 'Feature Scale', value: NormalizationType.FeatureScale};
        yield { label: 'Manhattan', value: NormalizationType.Manhattan};
        yield { label: 'Standard', value: NormalizationType.Standard};
    }else {
        yield { label: 'Not numeric column', value: NormalizationType.None};
    }
}

export const ColumnInfo = ({column, index, operation, onChangeColumnType, preview, onChangeColumnNormalization}: ColumnInfoProps) => {
    const {metadata, isTarget} = column;
    const [isExpanded, setIsExpanded] = useState(false);
    const [columnConversionOption, setColumnConversionOption] = useState(ColumnConversionType.Unchanged);
    const [isSelected, setIsSelected] = useState(false);
    const primary = Array.from(getConversionOptions(column.columnType));
    const reversedPrimary = [...primary].reverse();
    const secondary = Array.from(getSecondaryConversionOptions(column.columnType));
    const [normalizationType, setNormalizationType] = useState(NormalizationType.None);
    const categories = useMemo(() => metadata ? getAllMetaData("Category:", metadata).map(x => ({index: parseInt(x.name.substring(9), 10), name: x.value})) : [], [metadata]);

    useEffect(() => {
        onChangeColumnType?.(index, columnConversionOption);
    }, [columnConversionOption, index, onChangeColumnType]);

    useEffect(() => {
        onChangeColumnNormalization?.(index, normalizationType);
    }, [normalizationType, index, onChangeColumnNormalization]);

    return <div className={'column-info' + (isTarget ? ' target' : '') + ((isSelected && operation === Operation.VectoriseColumns) ? ' selected' : '')}>
        <div className="header">
            <span className={'type ' + column.columnType}>
                {getDataTypeName(column.columnType)}
                {isTarget ? <Icon icon="locate" iconSize={20} /> : null}
            </span>
            <span className="name">{column.name}</span>
            {((operation === Operation.VectoriseColumns && isNumeric(column.columnType)) || operation === Operation.CopyColumns)
                ? <Switch checked={isSelected} onChange={e => setIsSelected(e.currentTarget.checked)}/>    
                : undefined
            }
            {categories.length
                ? <Popover2 content={<HTMLTable>
                        <thead>
                            <tr>
                                <th colSpan={2}>Categories</th>
                            </tr>
                        </thead>
                        <tbody>{categories.map(x => 
                            <tr key={x.index}>
                                <td>{x.index}</td>
                                <td>{x.name}</td>
                            </tr>
                        )}</tbody>
                    </HTMLTable>
                }>
                    <Button icon="properties"/>
                </Popover2>
                : undefined
            }
            {preview
                ? <Popover2 content={<ColumnDataPreview preview={preview}/>}>
                    <Button icon="th-list"/>
                </Popover2>
                : undefined
            }
        </div>
        <div className="body">
            {operation != Operation.None || invalidHistogramColumnTypes.has(column.columnType)
                ? undefined
                : <div style={{cursor: 'pointer'}} tabIndex={0} onClick={() => setIsExpanded(true)}>{getHistogram(metadata ?? [])}</div>
            }
            {operation != Operation.None ? undefined : <PropertyList properties={[
                ...(metadata?.filter(m => !invalidColumns.has(m.name) && !m.name.startsWith('Frequency:') && !m.name.startsWith('FrequencyRange:') && !m.name.startsWith('FrequencyRange:') && !m.name.startsWith('Category:')).map(m => ({
                    name: formatName(m.name), 
                    value: simplify(m.value)
                })) ?? [])
            ]}/>}
            {operation === Operation.ConvertColumns
                ? <>
                    <RadioGroup
                        key="radio"
                        onChange={e => {
                            setColumnConversionOption(ColumnConversionType[e.currentTarget.value as ColumnConversionType]);
                        }}
                        selectedValue={columnConversionOption.toString()}
                        options={primary} 
                    />
                    {secondary.length ? <HTMLSelect 
                        options={secondary.concat(reversedPrimary)} 
                        value={columnConversionOption.toString()}
                        onChange={e => {
                            setColumnConversionOption(ColumnConversionType[e.currentTarget.value as ColumnConversionType]);
                        }}
                    /> : undefined}
                </>
                : undefined
            }
            {operation === Operation.NormalizeColumns
                ? <RadioGroup key="radio"
                    options={Array.from(getNormalizationOptions(column.columnType))}
                    selectedValue={normalizationType.toString()}
                    onChange={e => {
                        setNormalizationType(NormalizationType[e.currentTarget.value as NormalizationType]);
                    }}
                />
                : undefined
            }
            {operation === Operation.VectoriseColumns && !isNumeric(column.columnType)
                ? <Callout title="Not Numeric" icon="info-sign" intent='primary'>
                    <p>Only numeric columns can be vectorised</p>
                </Callout>
                : undefined
            }
        </div>
        {isExpanded ? <FullScreenPopup hideScrollbars={true} className='popup' title={column.name + ' Histogram'} onClose={() => setIsExpanded(false)}>
            <AutoSizeContainer className="histogram-container">{(width, height) => getHistogram(metadata ?? [], width, height - 5)}</AutoSizeContainer>
        </FullScreenPopup> : undefined}
    </div>;
}