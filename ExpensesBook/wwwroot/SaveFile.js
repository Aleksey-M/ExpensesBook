function fileSaveAs(filename, fileContent) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function uploadFileJS(element) {

    let file = element.files[0];
    let reader = new FileReader();

    reader.addEventListener("loadend", function () {
        let tmpField = document.getElementById('tempField');
        tmpField.value = reader.result;
        tmpField.dispatchEvent(new Event('change'));
    });

    reader.readAsText(file);
}