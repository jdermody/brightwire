import { LegendItem } from './Legend';

const COLOURS = [
    'rgba(31, 119, 180, 1)', 
    'rgba(255, 127, 14, 1)', 
    'rgba(44, 160, 44, 1)', 
    'rgba(214, 39, 40, 1)', 
    'rgba(148, 103, 189, 1)', 
    'rgba(140, 86, 75, 1)', 
    'rgba(227, 119, 194, 1)', 
    'rgba(127, 127, 127, 1)', 
    'rgba(188, 189, 34, 1)', 
    'rgba(23, 190, 207, 1)'
];

const COLOURS2 = [
    'rgba(141, 211, 199, 1)',
    'rgba(255, 255, 179, 1)',
    'rgba(190, 186, 218, 1)',
    'rgba(251, 128, 114, 1)',
    'rgba(128, 177, 211, 1)',
    'rgba(253, 180, 98, 1)',
    'rgba(179, 222, 105, 1)',
    'rgba(252, 205, 229, 1)',
    'rgba(217, 217, 217, 1)',
    'rgba(188, 128, 189, 1)',
    'rgba(204, 235, 197, 1)',
    'rgba(255, 237, 111, 1)'
];

export function getColour(index: number, transparent: boolean) {
    const ret = COLOURS[index % COLOURS.length];
    return transparent ? ret.replace(', 1)', ', 0.66') : ret;
}

export function getColour2(index: number, transparent: boolean) {
    const ret = COLOURS2[index % COLOURS2.length];
    return transparent ? ret.replace(', 1)', ', 0.8') : ret;
}

export class ColourTable {
    private readonly map = new Map<string, number>();

    getColourIndex(label: string) {
        const {map} = this;
        if(map.has(label))
            return map.get(label) || 0;
        
        const ret = map.size;
        map.set(label, ret);
        return ret;
    }

    get labels(): LegendItem[] {
        const {map} = this;
        return Array.from(map.entries())
            .map(kv => ({key: kv[0], value: kv[1]}))
            .sort((d1, d2) => d1.value - d2.value)
            .map(d => ({name:d.key, colour: getColour(this.getColourIndex(d.key), false)}))
        ;
    }
}