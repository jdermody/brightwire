import * as React from 'react';

export interface LegendItem {
    name: string;
    colour: string;
}

export interface LegendProps {
    labels: LegendItem[];
}

export function Legend(props: LegendProps) {
    return <ul className="legend">
        {props.labels.map((l, i) => <li key={i}>
            <span className="colour" style={{backgroundColor: l.colour}}></span>
            <span className="name">{l.name}</span>
        </li>)}
    </ul>;
}