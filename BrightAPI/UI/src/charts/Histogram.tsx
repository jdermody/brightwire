import * as React from 'react';
import { useCallback } from 'react';
import { simplify } from '../common/FormatUtils';
import { getColour } from './Colours';

export interface HistogramProps {
    canvasWidth?: number;
    canvasHeight?: number;
    frequency: {name: string, value: string}[];
}

export const Histogram = ({frequency, canvasWidth, canvasHeight}: HistogramProps) => {
    const canvas = useCallback((el: HTMLCanvasElement) => {
        if (el && frequency && frequency.length) {
            const ctx = el.getContext('2d');
            const parent = el.parentElement;
            if (parent) {
                el.width = canvasWidth ?? parent.offsetWidth;
                el.height = canvasHeight ?? parent.offsetWidth;
            }

            let max = Number.MIN_VALUE;
            let isRanged = frequency[0].name.startsWith('FrequencyRange');
            const values = new Array<number>();
            const labels = new Array<string>();
            for(const item of frequency) {
                const pos = item.name.indexOf(':');
                labels.push(simplify(item.name.substr(pos+1)));

                const val = parseFloat(item.value);
                if(val > max)
                    max = val;
                values.push(val);
            }

            const {width, height} = el;
            const step = (width - values.length) / values.length;
            let xOffset = 0;
            if (ctx) {
                ctx.font = 'bold 12px Avenir Next LT Pro';
                ctx.clearRect(0, 0, width, height);

                if(isRanged)
                    ctx.fillStyle = getColour(0, true);
                for(let i = 0, len = values.length; i < len; i++) {
                    const val = values[i];
                    const y = val / max * height;
                    if(!isRanged)
                        ctx.fillStyle = getColour(i, true);
                    ctx.fillRect(xOffset, height - y, step, y);

                    ctx.save();
                    ctx.translate(xOffset, 10);
                    ctx.rotate(-Math.PI/2);
                    ctx.textAlign = "right";
                    ctx.fillStyle = "#000";
                    ctx.fillText(labels[i], 0, step / 2 + 4);
                    ctx.restore();

                    xOffset += step+1;
                }
            }
        }
    }, [frequency]);

    return <div className="histogram">
        <canvas ref={canvas}/>
    </div>;
}