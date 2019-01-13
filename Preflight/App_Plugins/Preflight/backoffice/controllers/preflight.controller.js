(() => {

    function ctrl($scope, $rootScope, editorState, preflightService) {

        this.loaded = false;

        const setBadgeCount = () => {
            if (this.results.failedCount) {
                $scope.model.badge = {
                    count: this.results.failedCount,
                    type: 'alert'
                };
            }
        };

        /**
         * 
         */
        this.preflight = () => {
            this.loaded = false;
            preflightService.check(editorState.current.id)
                .then(resp => {
                    if (resp.status === 200) {
                        this.results = resp.data;
                        setBadgeCount();
                        this.loaded = true;
                    }
                });
        };

        /**
         * Watch the visibility of the app, then check rootscope for any data to display
         */
        $scope.$watch(
            () => angular.element(document.getElementById('preflight-app')).is(':visible'),
            (newVal, oldVal) => {
                if (newVal && newVal !== oldVal) {
                    if ($rootScope.preflightResult) {
                        this.results = $rootScope.preflightResult;
                        setBadgeCount();
                        this.loaded = true;
                    }
                }
            }
        );

        /**
         * 
         */
        this.readabilityHelp = () => {
            this.overlay = {
                view: '../app_plugins/preflight/backoffice/views/help.overlay.html',
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

        /**
         * 
         */

        this.preflight();
    }

    angular.module('umbraco').controller('preflight.controller', ['$scope', '$rootScope', 'editorState', 'preflightService', ctrl]);

})();