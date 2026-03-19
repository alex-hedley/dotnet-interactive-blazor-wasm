window.monacoInterop = (function () {
    let _editor = null;

    return {
        initialize: function (containerId, code, language) {
            return new Promise(function (resolve, reject) {
                require.config({
                    paths: { vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs' }
                });
                require(['vs/editor/editor.main'], function () {
                    const container = document.getElementById(containerId);
                    if (!container) {
                        const msg = 'monacoInterop: container #' + containerId + ' not found';
                        console.error(msg);
                        reject(new Error(msg));
                        return;
                    }
                    const lang = language === 'fsharp' ? 'fsharp' : 'csharp';
                    _editor = monaco.editor.create(container, {
                        value: code,
                        language: lang,
                        theme: 'vs-dark',
                        automaticLayout: true,
                        minimap: { enabled: false },
                        fontSize: 14,
                        scrollBeyondLastLine: false,
                        lineNumbers: 'on',
                        roundedSelection: false,
                        padding: { top: 8, bottom: 8 }
                    });
                    resolve();
                });
            });
        },

        getValue: function () {
            return _editor ? _editor.getValue() : '';
        },

        setValue: function (code) {
            if (_editor) _editor.setValue(code);
        },

        setLanguage: function (language) {
            if (_editor) {
                const lang = language === 'fsharp' ? 'fsharp' : 'csharp';
                monaco.editor.setModelLanguage(_editor.getModel(), lang);
            }
        },

        dispose: function () {
            if (_editor) {
                _editor.dispose();
                _editor = null;
            }
        }
    };
})();
