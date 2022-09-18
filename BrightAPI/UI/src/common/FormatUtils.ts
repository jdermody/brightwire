import numeral from 'numeral';
import { BrightDataType } from '../models';

export function formatNumeric(val: number|string) {
    return numeral(val).format('0,0.[000]');
}

export function simplify(val: string) {
    const num = +val;
    const pos = val.indexOf('/');

    if(pos > 0) {
        const left = +val.substr(0, pos);
        const right = +val.substr(pos+1);
        if(!isNaN(left) && !isNaN(right)) {
            return `${formatNumeric(left)}  --->  ${formatNumeric(right)}`;
        }
    }else if(isNaN(num)) {
        return val;
    }
    return formatNumeric(num);
}

export function formatName(name: string) {
    let ret = '';
    for(const ch of name) {
        if(isUpperCase(ch) && ret.length > 0)
            ret += ' ';
        ret += ch;
    }
    return ret;
}

export function isLowerCase(str: string)
{
    return str == str.toLowerCase() && str != str.toUpperCase();
}

export function isUpperCase(str: string)
{
    return str == str.toUpperCase() && str != str.toLowerCase();
}

export function replaceUnderscores(text: string) {
    return text.replace(/_/g, ' ');
}

export function limitText(text: string, maxLength = 50) {
    let ret = '';
    for (let i = 0, len = text.length; i < len; i++) {
        const ch = text.charAt(i);
        if (ch === ' ' && ret.length > maxLength) {
            ret += '...';
            return ret;
        } else {
            ret += ch;
        }
    }
    return ret;
}

export function getDataTypeName(type: BrightDataType) {
    switch(type) {
        case BrightDataType.Boolean: return "Boolean";
        case BrightDataType.SByte: return "Signed Byte";
        case BrightDataType.Date: return "Date";
        case BrightDataType.Double: return "Double";
        case BrightDataType.Float: return "Single";
        case BrightDataType.IndexList: return "Index List";
        case BrightDataType.Int: return "Int";
        case BrightDataType.Long: return "Long";
        case BrightDataType.Matrix: return "Matrix";
        case BrightDataType.String: return "String";
        case BrightDataType.Tensor3D: return "3D Tensor";
        case BrightDataType.Tensor4D: return "4D Tensor";
        case BrightDataType.Vector: return "Vector";
        case BrightDataType.WeightedIndexList: return "Weighted Index List";
    }
    return "??";
}