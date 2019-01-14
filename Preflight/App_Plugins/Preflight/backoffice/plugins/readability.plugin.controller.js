(() => {

    function overlay(preflightService) {
        this.text = preflightService.getHelpText();
    }

    angular.module('umbraco').controller('readability.overlay.controller', ['preflightService', overlay]);

    function ctrl() {
        /**
        * Displays an overlay explaining what the readability test actually does
         * @param {any} e click event
        */
        this.help = e => {
            e.preventDefault();
            this.overlay = {
                view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability.overlay.html`,
                show: true,
                title: 'Readability',
                subtitle: 'Why should I care?',
                text: 'Text, yo',
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };
    }

    angular.module('umbraco').controller('readability.plugin.controller', ctrl);
})();

