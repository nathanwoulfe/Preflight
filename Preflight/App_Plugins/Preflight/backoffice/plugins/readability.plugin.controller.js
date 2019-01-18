(() => {

    function overlay($scope) {
        // there's nothing here
    }

    angular.module('umbraco').controller('readability.overlay.controller', ['$scope', overlay]);

    function ctrl($scope) {
        /**
        * Displays an overlay explaining what the readability test actually does
         * @param {any} e click event
        */

        console.log($scope.model);

        this.help = e => {
            e.preventDefault();
            this.overlay = {
                view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability.overlay.html`,
                show: true,
                title: 'Readability',
                subtitle: 'Why should I care?',
                text: $scope.model.description,
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };
    }

    angular.module('umbraco').controller('readability.plugin.controller', ['$scope', ctrl]);
})();

