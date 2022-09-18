import React, { useEffect, useRef, useState } from 'react';

export interface AutoSizeContainerProps {
    children: (width: number, height: number) => JSX.Element;
    className?: string;
}

export const AutoSizeContainer = ({children, className}: AutoSizeContainerProps) => {
    const container = useRef<HTMLDivElement>(null);
    const [dimensions, setDimensions] = useState<{width: number, height: number}>({width: 0, height: 0});
    
    useEffect(() => {
        if(container.current) {
            const observer = new ResizeObserver(() => {
                if(container.current) {
                    const {offsetWidth, offsetHeight} = container.current;
                    setDimensions({width: offsetWidth, height: offsetHeight});
                }
            });
            observer.observe(container.current);
            return () => {
                observer.disconnect();
            };
        }
    }, [container.current]);
    
    return <div ref={container} className={className}>
        {children(dimensions.width, dimensions.height)}
    </div>;
};