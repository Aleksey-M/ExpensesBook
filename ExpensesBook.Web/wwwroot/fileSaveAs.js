'use strict'

export function fileSaveAs(filename, fileContent, binary = false) {
  const dataType = binary ? "data:text/plain;base64," : "data:text/plain;charset=utf-8,";
  const link = document.createElement('a');

  link.download = filename;
  link.href = dataType + encodeURIComponent(fileContent)
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}
