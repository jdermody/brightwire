import { Button, Classes, Dialog, Icon } from '@blueprintjs/core';
import React, { useCallback, useRef, useState } from 'react';
import { importFromFile } from './Commands';
import { TextFilePreview, TextFilePreviewProps } from './TextFilePreview';
import './Project.scss';
import Splitter, { SplitDirection } from '@devbookhq/splitter'

export const Project = () => {
    const form = useRef<HTMLFormElement>(null);
    const fileInput = useRef<HTMLInputElement>(null);
    const mainContainer = useRef<HTMLDivElement>(null);
    const [textFilePreviews, setTextFilePreviews] = useState<TextFilePreviewProps[]>([]);

    return <div className="project" ref={mainContainer}>
        <form ref={form}>
            <input type="file" ref={fileInput} onChange={e => importFromFile(e.currentTarget, form.current!, (props) => {
                setTextFilePreviews(curr => [props, ...curr]);
            })} multiple />
            <Button icon='document-open' onClick={() => fileInput.current!.click()}>Import...</Button>
        </form>

        <Dialog isOpen={textFilePreviews.length > 0}>
            <div className={Classes.DIALOG_HEADER}>
                <Icon icon='panel-table'/>
                <h3>{textFilePreviews[0].file.name}</h3>
            </div>
            <div className={Classes.DIALOG_BODY}>
                <TextFilePreview {...textFilePreviews[0]}/>
            </div>
            <div className={Classes.DIALOG_FOOTER}>
                <div className={Classes.DIALOG_FOOTER_ACTIONS}>
                    <Button onClick={() => setTextFilePreviews(textFilePreviews.slice(1))}>Cancel</Button>
                </div>
            </div>
        </Dialog>
    </div>;
};