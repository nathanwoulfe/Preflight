(() => {

    function ctrl($scope, $rootScope, $element, $timeout, editorState, preflightService, preflightHub) {

        const dirtyHashes = {};
        const propsBeingChecked = [];
        const validPropTypes = Umbraco.Sys.ServerVariables.Preflight.PropertyTypesToCheck;

        // cache these for the trivial perf benefit
        const formSelector = '.umb-property ng-form[name="propertyForm"]';
        const appSelector = 'preflight-app';

        const joinList = arr => {
            let outStr = '';
            if (arr.length === 1) {
                outStr = arr[0];
            } else if (arr.length === 2) {
                outStr = arr.join(' and ');
            } else if (arr.length > 2) {
                outStr = arr.slice(0, -1).join(', ') + ', and ' + arr.slice(-1);
            }
            return outStr;
        };

        this.results = {
            properties: []
        };

        /**
         * Convert a string to a hash for storage and comparison.
         * @param {string} s - the string to hashify
         * @returns {int} the generated hash
         */
        const getHash = s => s.split('').reduce((a, b) => {
                a = ((a << 5) - a) + b.charCodeAt(0);
                return a & a;
            }, 0);


        /**
         * Updates the badge in the header with the number of failed tests
         */
        const setBadgeCount = () => {
            if (this.results && this.results.failedCount > 0) {
                $scope.model.badge = {
                    count: this.results.failedCount,
                    type: 'alert'
                };
            } else {
                $scope.model.badge = {
                    type: 'success icon-'
                };
            }
        };


        /**
         * Updates the property set with the new value, and removes any temporary property from that set
         * @param {object} data - a response model item returned via the signalr hub
         */
        const rebindResult = data => {
            let newProp = true;
            this.results.properties.forEach(prop => {
                if (prop.label === data.label) {
                    angular.extend(prop, data);
                    prop.loading = false;
                    newProp = false;
                } 
            });

            // a new property will have a temporary placeholder - remove it
            // _temp ensures grid with multiple editors only removes the correct temp entry
            if (newProp) {
                const tempIndex = this.results.properties.map(p => p.name === `${data.name}_temp`).indexOf(true);
                if (tempIndex !== -1) {
                    this.results.properties.splice(tempIndex, 1);
                }
                this.results.properties.push(data);
            }
            
            this.results.failedCount = this.results.properties.reduce((prev, cur) => prev + cur.failedCount, 0);
            this.results.failed = this.results.failedCount > 0;

            propsBeingChecked.splice(propsBeingChecked.indexOf(data.name), 1);
            this.propsBeingCheckedStr = joinList(propsBeingChecked);
        };


        /**
         * Finds dirty content properties, checks the type and builds a collection of simple models for posting to the preflight checkdirty endpoint
         * Also generates and stores a hash of the property value for comparison on subsequent calls, to prevent re-fetching unchanged data
         */
        const checkDirty = () => {
            const propForms = document.querySelectorAll(formSelector);
            const dirtyProps = [];

            this.propsBeingCheckedStr = '';

            let hasDirty = false;

            propForms.forEach(f => {
                const elmScope = angular.element(f).scope();
                if (elmScope.propertyForm.$dirty && validPropTypes.indexOf(elmScope.property.editor) !== -1) {
                    const valAsString = JSON.stringify(elmScope.property.value); // treat json editors as strings
                    const hash = getHash(valAsString);
                    const propSlug = elmScope.property.label;

                    if (dirtyHashes[propSlug] !== hash) {

                        dirtyProps.push({
                            name: elmScope.property.label,
                            value: elmScope.property.editor === 'Umbraco.TinyMCE' ? elmScope.property.value : valAsString,
                            editor: elmScope.property.editor
                        });

                        dirtyHashes[propSlug] = hash;
                        hasDirty = true;
                    }
                }
            });

            // if dirty properties exist, create a simple model for each and send the lot off for checking
            // response comes via the signalr hub so is not handled here
            if (hasDirty) {
                $timeout(() => {

                    dirtyProps.forEach(prop => {
                        const existing = this.results.properties.filter(p => p.name === prop.name)[0];
                        if (existing) {
                            existing.open = false;
                            existing.failedCount = -1;
                        } else {
                            // generate new placeholder for pending results - this is removed when the response is returned
                            this.results.properties.push({
                                label: prop.name,
                                open: false,
                                failed: false,
                                failedCount: -1,
                                name: `${prop.name}_temp`
                            }); 
                        }

                        propsBeingChecked.push(prop.name);
                    });

                    this.propsBeingCheckedStr = joinList(propsBeingChecked);

                    const payload = {
                        properties: dirtyProps,
                        nodeId: editorState.current.id
                    };

                    preflightService.checkDirty(payload)
                        .then(() => { /* swallowed */ });
                });
            }
        };

        /**
         * Watch the visibility of the app, then update any dirty props
         */
        $scope.$watch(
            () => angular.element(document.getElementById(appSelector)).is(':visible'),
            (newVal, oldVal) => {
                if (newVal && newVal !== oldVal) {
                    checkDirty();
                    setBadgeCount();
                } 
            }
        );

        /*
         * 
         */
        $rootScope.$on('showPreflight', (event, data) => {
            if (data.nodeId === $scope.content.id) {
                // needs to find app closest to current scope
                const appLink = $element.closest('form').find('[data-element="sub-view-preflight"]');

                if (appLink) {
                    appLink.click();
                }          
            }
        });

        /**
         * Initiates the signalr hub for returning test results
         */
        preflightHub.initHub(hub => {
            hub.on('preflightTest',
                e => {
                    rebindResult(e);
                    setBadgeCount();
                });

            hub.start();
        });


        /**
         * Check all properties when the controller loads. Won't re-run when changing between apps
         */
        preflightService.check(editorState.current.id)
            .then(resp => {});
    }

    angular.module('preflight').controller('preflight.controller', ['$scope', '$rootScope', '$element', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();