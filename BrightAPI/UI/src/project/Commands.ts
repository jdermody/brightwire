import { TextFilePreviewProps } from "./TextFilePreview";

export function importFromFile(
    fileInput: HTMLInputElement, 
    form: HTMLFormElement,
    onNewFile: (data: TextFilePreviewProps) => void
) {
    if(fileInput.files) {
        for(const file of fileInput.files) {
            console.log(file.type);
            if(file.type === 'text/csv' || file.type === "text/plain") {
                var reader = new FileReader();
                reader.onload = function() {
                    if(typeof(reader.result) === 'string') {
                        const allLines = reader.result.split('\n');
                        const previewLines = allLines.slice(0, 1000);
                        onNewFile({
                            file, 
                            previewLines, 
                            allLines
                        });
                    }
                };
                reader.readAsText(file);
            }
        }
    }
    form.reset();
}