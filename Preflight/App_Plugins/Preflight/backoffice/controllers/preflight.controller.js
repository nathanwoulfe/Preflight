(() => {

    function ctrl($scope, $rootScope, $element, $timeout, editorState, preflightService, preflightHub) {

        const dirtyHashes = {};
        const validPropTypes = Umbraco.Sys.ServerVariables.Preflight.PropertyTypesToCheck;
        let propsBeingChecked = [];
        let dirtyProps = [];

        this.results = {
            properties: []
        };

        this.noTests = false;

        $scope.model.badge = {
            type: 'info'
        };


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


        /**
         * Convert a string to a hash for storage and comparison.
         * @param {string} s - the string to hashify
         * @returns {int} the generated hash
         */
        const getHash = s => s.split('').reduce((a, b) => {
            a = (a << 5) - a + b.charCodeAt(0);
            return a & a;
        }, 0);


        /**
         * Get property by alias from the current variant
         * @param {any} alias
         */
        const getProperty = alias => {
            for (let tab of editorState.current.variants.find(x => x.active).tabs) {
                for (let prop of tab.properties) {
                    if (prop.alias === alias) {
                        return prop;
                    }
                }
            }
        };


        /**
         * 
         * @param {any} editor
         */
        const onComplete = () => {
            // it's possible no tests ran, in which case results won't exist
            this.noTests = this.results.properties.every(x => !x.plugins.length);
            if (this.noTests) {
                $scope.model.badge = undefined;
            }

            for (let p of this.results.properties) {
                if (p.failedCount === -1) {
                    p.disabled = true;
                }
            }
        };

        /**
         * Is the editor param Umbraco.Grid or Umbraco.NestedContent?
         * @param {any} editor
         */
        const isJsonProperty = editor => editor === 'Umbraco.Grid' || editor === 'Umbraco.NestedContent';


        /**
         * Updates the badge in the header with the number of failed tests
         */
        const setBadgeCount = pending => {
            if (pending) {
                $scope.model.badge = {
                    type: 'warning'
                };
                return;
            }

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

            dirtyProps = []; 
            let hasDirty = false;

            for (let prop of propertiesToTrack) {
                let currentValue = getProperty(prop.alias).value;
                currentValue = isJsonProperty(prop.editor) ? JSON.stringify(currentValue) : currentValue;

                const hash = getHash(currentValue);

                if (dirtyHashes[prop.label] && dirtyHashes[prop.label] !== hash) {

                    dirtyProps.push({
                        name: prop.label,
                        value: currentValue,
                        editor: prop.editor
                    });

                    dirtyHashes[prop.label] = hash;
                    hasDirty = true;
                } else if (!dirtyHashes[prop.label]) {
                    dirtyHashes[prop.label] = hash;
                }

            }

            // if dirty properties exist, create a simple model for each and send the lot off for checking
            // response comes via the signalr hub so is not handled here
            if (hasDirty) {
                $timeout(() => {

                    dirtyProps.forEach(prop => {
                        for (let existing of this.results.properties.filter(p => p.name === prop.name)) {

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
                        }

                        propsBeingChecked.push(prop.name);
                    });

                    this.propsBeingCheckedStr = joinList(propsBeingChecked);

                    const payload = {
                        properties: dirtyProps,
                        nodeId: editorState.current.id
                    };

                    setBadgeCount(true);
                    preflightService.checkDirty(payload);
                });
            }
        };


        /*
         * 
         */
        $rootScope.$on('app.tabChange', (e, data) => {
            if (data.alias === 'preflight') {
                // collapse open nc controls, timeouts prevent $apply errors
                for (let openNc of document.querySelectorAll('.umb-nested-content__item--active .umb-nested-content__header-bar')) {
                    $timeout(() => openNc.click());
                }

                $timeout(() => {
                    checkDirty();
                    setBadgeCount();
                });
            }
        });


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
        const init = () => {

            //preflightService.getSettings()
            //    .then(resp => {
            //        const groups = resp.data.settings.find(x => x.label === 'User group opt in/out');
            //        if (groups) {
            //            userService
            //        }
            //    });

            preflightHub.initHub(hub => {

                hub.on('preflightTest',
                    e => {
                        rebindResult(e);
                        setBadgeCount();
                    });

                hub.on('preflightComplete',
                    () => onComplete()
                );

                hub.start(e => {
                    /**
                     * Check all properties when the controller loads. Won't re-run when changing between apps
                     * but needs to happen after the hub loads
                     */
                    $timeout(() => {
                        setBadgeCount(true);
                        preflightService.check(editorState.current.id);
                    }, 3000);
                });
            });
        };

        /**
         * Check the current editor has properties managed by preflight - hide the app if not
         * Stores a reference collection of tracked properties
         */
        const activeVariant = editorState.current.variants.find(x => x.active);
        let propertiesToTrack = [];

        if (activeVariant) {
            activeVariant.tabs.forEach(x => {
                propertiesToTrack = propertiesToTrack.concat(x.properties.map(x => {
                    if (validPropTypes.indexOf(x.editor)) {
                        return {
                            editor: x.editor,
                            alias: x.alias,
                            label: x.label
                        };
                    }
                }));
            });

            if (propertiesToTrack.length) {
                init();
            } else {
                const appLink = $element.closest('form').find('[data-element="sub-view-preflight"]');

                if (appLink) {
                    const appLinkListItem = appLink.closest('li')[0];
                    if (appLinkListItem) {
                        appLinkListItem.classList.add('ng-hide');
                    }
                }
            }
        }
    }

    angular.module('preflight').controller('preflight.controller', ['$scope', '$rootScope', '$element', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();