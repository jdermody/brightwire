import { Alignment, Button, Navbar } from '@blueprintjs/core';
import React, { useState } from 'react';
import { useEffect } from 'react';
import './FullScreenPopup.scss';

export interface FullScreenPopupProps {
    title: string;
    className?: string;
    onClose: () => void;
    onOpen?: () => void;
    hideScrollbars?: boolean;
    headerContent?: JSX.Element;
}

export const FullScreenPopup = ({title, onClose, onOpen, children, headerContent, ...props}: React.PropsWithChildren<FullScreenPopupProps>) => {
    const [isOpen, setIsOpen] = useState(true);
    const className = 'full-screen-popup ' 
        + (isOpen ? 'scale-in-center' : 'scale-out-center') 
        + (props.className ? (' ' + props.className) : '')
        + (props.hideScrollbars ? ' no-overflow' : '')
    ;

    useEffect(() => onOpen?.(), []);

    return <div 
        className={className} 
        onAnimationEnd={e => {
            if(e.animationName === 'scale-out-center') {
                setIsOpen(true);
                onClose();
            }
        }}
        onKeyDown={e => {
            if(e.key === 'Escape')
                setIsOpen(false);   
        }}
        tabIndex={-1}
        ref={e => e?.focus()}
    >
        <Navbar>
            <Navbar.Group align={Alignment.LEFT}>
                <Navbar.Heading>{title}</Navbar.Heading>
                {headerContent}
            </Navbar.Group>
            <Navbar.Group align={Alignment.RIGHT}>
                <Button icon="cross" large={true} onClick={() => {
                    setIsOpen(false);
                }} />
            </Navbar.Group>
        </Navbar>
        <div className="body">{children}</div>
    </div>;
}