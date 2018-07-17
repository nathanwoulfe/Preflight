(() => {

    function ctrl($scope, editorState, appState, preflightService) {

        this.loaded = false;

        const currentNodeId = $scope.dialogOptions.currentNode ? $scope.currentNode.id : -1;

        /**
         * 
         */
        const checkProperties = () => {

            if (this.checkReadability) {
                this.blacklist = this.properties.filter(p => p.readability.blacklist.length);

                this.failedReadability = this.properties.filter(p =>
                    p.readability.score < this.readabilityTargetMin || p.readability.score > this.readabilityTargetMax
                );
            }

            if (this.checkLinks) {
                this.brokenLinks = this.properties.filter(p => p.links.length);
            }
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
        const preflight = () => {
            if (currentNodeId !== -1) {
                preflightService.check(currentNodeId)
                    .then(resp => {
                        if (resp.status === 200) {
                            bindResults(resp.data);
                        }
                    });
            } else {
                bindResults($scope.dialogOptions.results);
            }
        };
        

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
        this.resultGradient = () => 
            `linear-gradient(90deg, 
            #fe6561 ${this.readabilityTargetMin - 15}%, 
            #f9b945 ${this.readabilityTargetMin}%, 
            #35c786,
            #f9b945 ${this.readabilityTargetMax}%, 
            #fe6561 ${this.readabilityTargetMax + 15}%)`;
        

        // this is from navigation service, needs to happen here as the modal may not be added via the service, so must be closed manually
        this.close = () => {
            appState.setGlobalState('navMode', 'default');
            appState.setMenuState('showMenu', false);
            appState.setMenuState('showMenuDialog', false);
            appState.setSectionState('showSearchResults', false);
            appState.setGlobalState('stickyNavigation', false);
            appState.setGlobalState('showTray', false);

            if (appState.getGlobalState('isTablet') === true) {
                appState.setGlobalState('showNavigation', false);
            }
        }

        /**
         * 
         */
        preflightService.getSettings()
            .then(resp => {
                resp.data.forEach(v => {
                    this[v.alias] = v.value;
                });

                preflight();
            });
    }

    angular.module('umbraco').controller('preflight.dialog.controller', ['$scope', 'editorState', 'appState', 'preflightService', ctrl]);

})();