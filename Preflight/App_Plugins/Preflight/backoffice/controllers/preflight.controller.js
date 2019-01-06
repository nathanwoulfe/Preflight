(() => {

    function ctrl($scope, $rootScope, editorState, preflightService) {

        this.loaded = false;
        this.settings = {};

        /**
         * 
         */
        const checkProperties = () => {
            this.properties.forEach(p => {
                this.blacklistFailed = this.blacklistFailed || p.readability.blacklist.length > 0;
                this.longWordsFailed = this.longWordsFailed || p.readability.longWords.length > 0;

                this.brokenLinksFailed = this.brokenLinksFailed || p.links.length > 0;
                this.safeBrowsingFailed = this.safeBrowsingFailed || p.safeBrowsing.length > 0;

                this.readabilityFailed = this.readabilityFailed ||
                    (p.readability.score < this.settings.readabilityTargetMin ||
                        p.readability.score > this.settings.readabilityTargetMax);

            });
        };

        /**
         * 
         * @param {any} data
         */
        const bindResults = data => {

            this.properties = data.properties;
            if (this.properties.length) {
                checkProperties();
            }

            this.checkLinks = data.checkLinks;
            this.checkReadability = data.checkReadability;
            this.checkSafeBrowsing = data.checkSafeBrowsing;

            this.failed = data.failed;

            this.loaded = true;
        };


        /**
         *
         */
        this.failedTestsString = () => {
            const failedTests = [];

            if (this.readabilityFailed) {
                failedTests.push('fails the readability test');
            }

            if (this.brokenLinksFailed || this.safeBrowsingFailed) {
                failedTests.push('contains broken or unsafe links');
            }

            if (this.blacklistFailed) {
                failedTests.push('contains blacklisted words');
            }

            if (this.longWordsFailed) {
                failedTests.push('contains long words');
            }

            if (failedTests.length === 1) {
                return failedTests;
            }

            if (failedTests.length === 2) {
                return failedTests.join(' and ');
            }

            return failedTests.join(', ').replace(/, ([^,]*)$/, ' and $1');
        };


        /**
         * 
         */
        this.preflight = () => {
            this.loaded = false;
            preflightService.check(editorState.current.id)
                .then(resp => {
                    if (resp.status === 200) {
                        bindResults(resp.data);
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
                        bindResults($rootScope.preflightResult);
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
        preflightService.getSettings()
            .then(resp => {
                resp.data.forEach(v => {
                    this.settings[v.alias] = v.value;
                });

                this.preflight();
            });
    }

    angular.module('umbraco').controller('preflight.controller', ['$scope', '$rootScope', 'editorState', 'preflightService', ctrl]);

})();