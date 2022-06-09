import { Button } from '@blueprintjs/core';
import React, { useCallback, useRef, useState } from 'react';
import { createBalancedTreeFromLeaves, getLeaves, Mosaic, MosaicBranch, MosaicDirection, MosaicNode, MosaicWindow } from 'react-mosaic-component';
import { importFromFile } from './Commands';
import { TextFilePreview, TextFilePreviewProps } from './TextFilePreview';
import '../../style/Project.scss';

// workaround from https://github.com/nomcopter/react-mosaic/issues/184 
declare module 'react-mosaic-component' {
    //@ts-ignore
    export interface MosaicWindowProps<T extends MosaicKey> {
        children: React.ReactNode;
    }
}

type ViewType = 'file-list' | 'import';

function renderView(id: ViewType, path: MosaicBranch[]) {
    if(id === 'file-list') {
        return <MosaicWindow<ViewType> 
            path={path} 
            title='Files'
        >
            <h1>Files</h1>
        </MosaicWindow>;
    }
    else if(id === 'import') {
        return <MosaicWindow<ViewType> 
            path={path} 
            title='Preview'
        >
            <h1>Import</h1>
        </MosaicWindow>;
    }
    return <div>unknown: {id}</div>;
}

export const Project = () => {
    const form = useRef<HTMLFormElement>(null);
    const fileInput = useRef<HTMLInputElement>(null);
    const mainContainer = useRef<HTMLDivElement>(null);
    const [windowState, setWindowState] = useState<MosaicNode<ViewType>>('file-list');
    const [textFilePreviews, setTextFilePreviews] = useState<TextFilePreviewProps[]>([]);
    const showWindow = useCallback((id: ViewType) => {
        const existingLeaves = getLeaves(windowState);
        if(existingLeaves.find(n => n === id))
            return;

        let direction: MosaicDirection = 'column';
        const rect = mainContainer?.current?.getBoundingClientRect();
        if(rect) {
            const {width, height} = rect;
            if(width > height)
                direction = 'row';
        }
        const newTree = createBalancedTreeFromLeaves([...existingLeaves, id], direction);
        if(newTree)
            setWindowState(newTree);
    }, []);

    return <div className="project" ref={mainContainer}>
        <form ref={form}>
            <input type="file" ref={fileInput} onChange={e => importFromFile(e.currentTarget, form.current!, (props) => {
                setTextFilePreviews(curr => [props, ...curr]);
                showWindow('import');
            })} multiple />
            <Button icon='document-open' onClick={() => fileInput.current!.click()}>Import...</Button>
        </form>

        <Mosaic<ViewType>
            value={windowState}
            onChange={state => setWindowState(state!)}
            renderTile={renderView}
        />
    </div>;
};