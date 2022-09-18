import { BrightDataType, NameValueModel } from "../models";
import { simplify } from "./FormatUtils";

export const isClassificationTarget = (metaData: NameValueModel[]) => metaData.filter(d => d.name === 'IsTarget').length > 0;
export const isClusterIndex = (metaData: NameValueModel[]) => metaData.filter(d => d.name === 'Cluster Index').length > 0;
export const getName = (metaData: NameValueModel[], defaultName: string = '???') => metaData?.filter(d => d.name === 'Name')[0]?.value ?? defaultName;

export const getColumnIndex = (metaData: NameValueModel[]) => parseInt(metaData.filter(d => d.name === 'Index')[0].value, 10);

export const isNumeric = (dataType: BrightDataType) => {switch(dataType) {
    case BrightDataType.SByte:
    case BrightDataType.Decimal:
    case BrightDataType.Double:
    case BrightDataType.Float:
    case BrightDataType.Int:
    case BrightDataType.Long:
    case BrightDataType.Short:
        return true;

    default:
        return false;
}};

export function getExactMetaData(name: string, metaData: NameValueModel[]) {
    const ret = metaData.filter(d => d.name === name);
    return ret.length ? ret[0].value : undefined;
}

export function getAllMetaData(name: string, metaData: NameValueModel[]) {
    return metaData.filter(d => d.name.startsWith(name));
}

export function formatMetaData(metaData: NameValueModel[]) {
    return metaData.map(m => ({
        name: m.name,
        value: simplify(m.value)
    }))
}