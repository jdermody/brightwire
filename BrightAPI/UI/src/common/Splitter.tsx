import { Slider } from '@blueprintjs/core';
import React, { useRef, useState } from 'react';
import './Splitter.scss';

export interface SplitterProps {
    first: JSX.Element | JSX.Element[];
    second: JSX.Element | JSX.Element[];
}

const MIN_SIZE = 50;

export const Splitter = ({first, second}: SplitterProps) => {
    const splitter = useRef<HTMLDivElement>(null);
    const divider = useRef<HTMLDivElement>(null);
    const [isDragging, setIsDragging] = useState(false);
    const [firstPosition, setFirstPosition] = useState<number>();

    return <div className="splitter" ref={splitter}>
        <div className="lhs" style={firstPosition ? {flexBasis: firstPosition + 'px'} : undefined}>
            {first}
        </div>
        <div 
            ref={divider}
            className={'divider' + (isDragging ? ' dragging' : '')}
            onPointerDown={e => {
                divider.current?.setPointerCapture(e.pointerId);
                setIsDragging(true);
            }}
            onPointerMove={e => {
                if(splitter.current && isDragging) {
                    var box = splitter.current.getBoundingClientRect();
                    var style = getComputedStyle(splitter.current);
                    if(style.flexDirection === 'row') {
                        var pos = Math.min(box.width - MIN_SIZE, Math.max(MIN_SIZE, e.clientX - box.left));
                        setFirstPosition(pos);
                    }else if(style.flexDirection === 'column') {
                        var pos = Math.min(box.height - MIN_SIZE, Math.max(MIN_SIZE, e.clientY - box.top));
                        setFirstPosition(pos);
                    }
                }
            }}
            onPointerUp={e => {
                divider.current?.releasePointerCapture(e.pointerId);
                setIsDragging(false);
            }}
        />
        <div className="rhs">
            {second}
        </div>
    </div>
};