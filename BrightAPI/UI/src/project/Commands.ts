import { TextFilePreviewProps } from "./TextFilePreview";

export function importFromFile(
    fileInput: HTMLInputElement, 
    form: HTMLFormElement,
    onNewFile: (data: TextFilePreviewProps) => void
) {
    if(fileInput.files) {
        for(const file of fileInput.files) {
            if(file.type === 'text/csv') {
                var reader = new FileReader();
                reader.onload = function() {
                    if(typeof(reader.result) === 'string') {
                        const lines = reader.result.split('\n').slice(0, 1000);
                        onNewFile({file, lines});
                    }
                };
                reader.readAsText(file);
            }
        }
    }
    form.reset();
}