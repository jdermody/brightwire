import React from 'react';
import { useCallback } from 'react';

export interface ScatterPoint {
    x: number;
    y: number;
    colour: string;
    backgroundColor?: string;
}

interface InputDimensions {
    xMin: number;
    xRange: number;
    yMin: number;
    yRange: number;
}

export interface ScatterPlot2DProps extends InputDimensions {
    points: ScatterPoint[];
}

interface DisplayRect {
    width: number;
    height: number;
    paddingX: number;
    paddingY: number;
}

function drawPoint(point: ScatterPoint, colour: string, size: number, dims: InputDimensions, context: CanvasRenderingContext2D, display: DisplayRect) {
    context.beginPath();
    context.fillStyle = colour;
    const {width, height, paddingX, paddingY} = display;

    const px = paddingX + (point.x - dims.xMin) / dims.xRange * width;
    const py = paddingY + (point.y - dims.yMin) / dims.yRange * height;

    context.arc(px, height - py, size, 0, 2 * Math.PI, true);
    context.fill();
}

export const ScatterPlot2D = ({points, ...layout}: ScatterPlot2DProps) => {
    const canvas = useCallback((el: HTMLCanvasElement) => {
        if (el) {
            const parent = el.parentElement;
            if (parent) {
                el.width = parent.offsetWidth;
                el.height = parent.offsetWidth;
            }
            const {width, height} = el;

            const ctx = el.getContext('2d');
            if (ctx) {
                ctx.font = 'bold 12px Avenir Next LT Pro';
                ctx.clearRect(0, 0, width, height);
                const display: DisplayRect = {
                    height: height-6,
                    width: width - 6,
                    paddingX: 3,
                    paddingY: 3
                };

                for(const pt of points) {
                    if(pt.backgroundColor)
                        drawPoint(pt, pt.backgroundColor, 7, layout, ctx, display);  
                    drawPoint(pt, pt.colour, 3, layout, ctx, display);
                }
            }
        }
    }, [points]);
    return <canvas ref={canvas}/>;
};