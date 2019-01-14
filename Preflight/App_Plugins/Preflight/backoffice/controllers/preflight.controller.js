(() => {

    function ctrl($scope, $rootScope, $timeout, editorState, preflightService, preflightHub) {

        this.loaded = false;
        const dirtyHashes = [];
        const validPropTypes = ['Umbraco.TinyMCEv3', 'Umbraco.Grid', 'Imulus.Archetype'];

        const getHash = s => s.split('').reduce((a, b) => {
                a = ((a << 5) - a) + b.charCodeAt(0);
                return a & a;
            }, 0);

        const setBadgeCount = () => {
            if (this.results && this.results.failedCount) {
                $scope.model.badge = {
                    count: this.results.failedCount,
                    type: 'alert'
                };
            }
        };

        const rebindResult = data => {
            let newProp = true;
            this.results.properties.forEach(prop => {
                if (prop.label === data.label) {
                    angular.extend(prop, data);
                    prop.loading = false;
                    newProp = false;
                } 
            });

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
                            editor: elmScope.property.editor,
                        });

                        dirtyHashes.push(hash);
                        hasDirty = true;
                    }

                    console.log(dirtyHashes);
                }
            });

            // response will be true/false, signalr handles updating ui
            if (hasDirty) {
                $timeout(() => {
                    dirtyProps.forEach(prop => {
                        const existing = this.results.properties.filter(p => p.name === prop.name)[0];
                        if (existing) {
                            existing.loading = true;
                            existing.open = false;
                        } else {
                            // generate new placeholder for pending results
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
                            console.log(resp);
                        });
                });
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

        const init = () => {
            if ($rootScope.preflightResult) {
                this.results = $rootScope.preflightResult;
                delete $rootScope.preflightResult;
            } else {
                checkDirty();
            }

            setBadgeCount();
            this.loaded = true;
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

        preflightHub.initHub(hub => {
            hub.on('preflightTest',
                e => {
                    rebindResult(e);
                    setBadgeCount();
                    this.loaded = true;
                });

            hub.start();
        });

        preflightService.getSettings()
            .then(resp => {
                console.log(resp);
            });

        // check all properties on load
        this.preflight();
    }

    angular.module('umbraco').controller('preflight.controller', ['$scope', '$rootScope', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();