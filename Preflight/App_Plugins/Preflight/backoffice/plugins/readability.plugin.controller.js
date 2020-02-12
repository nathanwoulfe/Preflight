(() => {

    function overlay($scope) {
        // there's nothing here
    }

    angular.module('umbraco').controller('readability.overlay.controller', ['$scope', overlay]);

    function ctrl($scope, editorService) {
        /**
        * Displays an overlay explaining what the readability test actually does
         * @param {any} e click event
        */

        this.help = e => {
            e.preventDefault();
            const helpOverlay = {
                view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability.overlay.html`,
                title: 'Readability',
                description: 'Why should I care?',
                size: 'small',
                text: $scope.model.description,
                close: () => editorService.close()                
            };

            editorService.open(helpOverlay);
        };
    }

    angular.module('umbraco').controller('readability.plugin.controller', ['$scope', 'editorService', ctrl]);
})();

