(() => {

    function ctrl($scope, $rootScope, $timeout, editorState, preflightService, preflightHub) {

        const dirtyHashes = [];
        const validPropTypes = ['Umbraco.TinyMCEv3', 'Umbraco.Grid', 'Imulus.Archetype'];


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
            if (this.results && this.results.failedCount) {
                $scope.model.badge = {
                    count: this.results.failedCount,
                    type: 'alert'
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
        };


        /**
         * Finds dirty content properties, checks the type and builds a collection of simple models for posting to the preflight checkdirty endpoint
         * Also generates and stores a hash of the property value for comparison on subsequent calls, to prevent re-fetching unchanged data
         */
        const checkDirty = () => {
            const propForms = document.querySelectorAll('[data-element="editor-container"] [name="propertyForm"]');
            const dirtyProps = [];
            let hasDirty = false;

            propForms.forEach(f => {
                const elmScope = angular.element(f).scope();
                if (elmScope.propertyForm.$dirty && validPropTypes.indexOf(elmScope.property.editor) !== -1) {
                    const valAsString = JSON.stringify(elmScope.property.value); // treat json editors as strings
                    const hash = getHash(valAsString);

                    if (dirtyHashes.indexOf(hash) === -1) {

                        dirtyProps.push({
                            name: elmScope.property.label,
                            value: valAsString,
                            editor: elmScope.property.editor
                        });

                        dirtyHashes.push(hash);
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
                            existing.loading = true;
                            existing.open = false;
                        } else {
                            // generate new placeholder for pending results - this is removed when the response is returned
                            this.results.properties.push({
                                label: prop.name,
                                loading: true,
                                open: false,
                                failed: false,
                                failedCount: 0,
                                name: `${prop.name}_temp`
                            });
                        }
                    });

                    preflightService.checkDirty(dirtyProps)
                        .then(resp => {
                            // swallowed
                        });
                });
            }
        };


        /**
         * Determine what data to display - content from $rootScope if it exists, otherwise updates the existing properties
         */
        const init = () => {
            if ($rootScope.preflightResult) {
                this.results = $rootScope.preflightResult;
                delete $rootScope.preflightResult;
            } else {
                checkDirty();
            }

            setBadgeCount();
        };


        /**
         * Watch the visibility of the app, then check rootscope for any data to display
         */
        $scope.$watch(
            () => angular.element(document.getElementById('preflight-app')).is(':visible'),
            (newVal, oldVal) => {
                if (newVal && newVal !== oldVal) {
                    init();
                } 
            }
        );


        /**
         * Initiates the signalr hub for returning test results
         */
        preflightHub.initHub(hub => {
            hub.on('preflightTest',
                e => {
                    rebindResult(e);
                    setBadgeCount();
                    this.loaded = true;
                });

            hub.start();
        });


        /**
         * Check all properties when the controller loads. Won't re-run when changing between apps
         */
        preflightService.check(editorState.current.id)
            .then(resp => {
                if (resp.status === 200) {
                    this.results = resp.data;
                    setBadgeCount();
                }
            });
    }

    angular.module('preflight').controller('preflight.controller', ['$scope', '$rootScope', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();