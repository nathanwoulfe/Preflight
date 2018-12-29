(() => {

    function ctrl(editorState, preflightService) {

        this.loaded = false;
        this.settings = {};

        const currentNodeId = editorState.current.id;

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
        this.failedTestsString = () => {
            const failedTests = [];
            if (this.failedReadability) {
                failedTests.push('fails the readability test');
            }

            if (this.brokenLinks && this.brokenLinks.length) {
                failedTests.push('contains broken or invalid/unsafe links');
            }

            if (this.blacklist) {
                failedTests.push('contains blacklisted words');
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
            preflightService.check(currentNodeId)
                .then(resp => {
                    if (resp.status === 200) {
                        bindResults(resp.data);
                    }
                });
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
            #fe6561 ${this.settings.readabilityTargetMin - 15}%, 
            #f9b945 ${this.settings.readabilityTargetMin}%, 
            #35c786,
            #f9b945 ${this.settings.readabilityTargetMax}%, 
            #fe6561 ${this.settings.readabilityTargetMax + 15}%)`;
        
        /**
         * 
         */
        preflightService.getSettings()
            .then(resp => {
                resp.data.forEach(v => {
                    this.settings[v.alias] = v.value;
                });
            });
    }

    angular.module('umbraco').controller('preflight.dialog.controller', ['editorState', 'preflightService', ctrl]);

})();