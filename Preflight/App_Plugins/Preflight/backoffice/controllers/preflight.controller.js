(() => {

    function ctrl($scope, $rootScope, editorState, preflightService) {

        this.loaded = false;

        const setBadgeCount = () => {
            $scope.model.badge = {
                count: this.results.failedCount,
                type: 'alert'
            };
        };

        /**
         *  
         * @param {any} data
         */
        //const bindResults = () => {

        //    if (this.results.properties.length) {
        //        this.results.properties.forEach(p => {
        //            this.blacklistFailed = this.blacklistFailed || p.readability.blacklist.length > 0;
        //            this.longWordsFailed = this.longWordsFailed || p.readability.longWords.length > 0;

        //            this.brokenLinksFailed = this.brokenLinksFailed || p.links.length > 0;
        //            this.safeBrowsingFailed = this.safeBrowsingFailed || p.safeBrowsing.length > 0;

        //            this.readabilityFailed = this.readabilityFailed || p.failedReadability;
        //        });
        //    }

        //    this.loaded = true;
        //};


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
                        //bindResults();
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
                        //bindResults();
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