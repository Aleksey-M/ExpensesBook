'use strict'

export function connect(dbName, dbVersion, objectStoreInfos, dotnetCallbacks) {

    const request = indexedDB.open(dbName, dbVersion);

    // return indexedDb instance to dotnet
    request.onsuccess = (event) => {
        const idb = event.target.result;
        const dotnetRef = DotNet.createJSObjectReference(idb);

        dotnetCallbacks.invokeMethodAsync('Completed', dotnetRef);
    };

    request.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message)

        dotnetCallbacks.invokeMethodAsync('Error', message);
    }

    // create object store(s)
    request.onupgradeneeded = (event) => {
        const db = event.target.result;

        if (Array.isArray(objectStoreInfos)) {
            objectStoreInfos.forEach((v) =>
                db.createObjectStore(v.name, { keyPath: v.keyPath }));            
        }
    }
}


export function addItem(idbDotnetRef, dotnetCallbacks, objectStoreName, item) {

    const transaction = idbDotnetRef.transaction([objectStoreName], "readwrite");

    // waiting for transaction complete
    transaction.oncomplete = (event) => {
        dotnetCallbacks.invokeMethodAsync('Completed');
    };

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // invoke adding
    transaction
        .objectStore(objectStoreName)
        .add(item);
}


export function getItem(idbDotnetRef, dotnetCallbacks, objectStoreName, id) {

    const transaction = idbDotnetRef.transaction([objectStoreName]);

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // invoke and return item on success (readonly mode)
    transaction
        .objectStore(objectStoreName)
        .get(id)
        .onsuccess = (event) => {
            dotnetCallbacks.invokeMethodAsync('Completed', event.target.result);
        };
}


export function deleteItem(idbDotnetRef, dotnetCallbacks, objectStoreName, id) {

    const transaction = idbDotnetRef.transaction([objectStoreName], "readwrite");

    // waiting for transaction complete
    transaction.oncomplete = (event) => {
        dotnetCallbacks.invokeMethodAsync('Completed');
    };

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // invoke deleting
    transaction
        .objectStore(objectStoreName)
        .delete(id);
}


export function getCount(idbDotnetRef, dotnetCallbacks, objectStoreName) {

    const transaction = idbDotnetRef.transaction([objectStoreName]);

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // invoke and return count on success (readonly mode)
    transaction
        .objectStore(objectStoreName)
        .count()
        .onsuccess = (event) => {
            dotnetCallbacks.invokeMethodAsync('Completed', event.target.result);
        };
}


export function clearItems(idbDotnetRef, dotnetCallbacks, objectStoreName) {

    const transaction = idbDotnetRef.transaction([objectStoreName], "readwrite");

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // waiting for transaction complete
    transaction.oncomplete = (event) => {
        dotnetCallbacks.invokeMethodAsync('Completed');
    };

    // invoke deleting of all items
    transaction
        .objectStore(objectStoreName)
        .clear();
}


export function getItemsList(idbDotnetRef, dotnetCallbacks, objectStoreName, criteries) {

    const transaction = idbDotnetRef.transaction([objectStoreName]);

    // filtered result
    const items = [];

    transaction.oncomplete = (event) => {
        dotnetCallbacks.invokeMethodAsync('Completed', items);
    };

    transaction.onerror = (event) => {
        const message = `ERROR! ${event.srcElement.error.name} : ${event.srcElement.error.message}`;
        console.log(message);
        dotnetCallbacks.invokeMethodAsync('Error', message);
    };

    // invoke and filter items on success (readonly mode)
    transaction
        .objectStore(objectStoreName)
        .openCursor()
        .onsuccess = (event) => {
            const cursor = event.target.result;

            if (cursor) {

                let item = cursor.value;
                let fit = true;

                if (Array.isArray(criteries) && criteries.length > 0) {

                    for (let i = 0; i < criteries.length; i++) {

                        switch (criteries[i].type) {
                            case 1:
                                fit = item[criteries[i].propertyJsName].toLowerCase().includes(criteries[i].value.toLowerCase());
                                break;

                            case 2:
                                fit = item[criteries[i].propertyJsName] === criteries[i].value;
                                break;

                            case 3:
                                fit = item[criteries[i].propertyJsName] >= criteries[i].value;
                                break;

                            case 4:
                                fit = item[criteries[i].propertyJsName] <= criteries[i].value;
                                break;

                            default: fit = false;
                        }

                        if (!fit) break;
                    }
                }

                if (fit) {
                    items.push(item);
                }                           

                //
                cursor.continue();
            }
        };
}