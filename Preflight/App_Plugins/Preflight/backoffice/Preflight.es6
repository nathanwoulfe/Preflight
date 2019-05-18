/*! preflight - v0.1.0-build46 - 2019-05-18
 * Copyright (c) 2019 Nathan Woulfe;
 * Licensed MIT
 */

(() => {

    angular.module('preflight.components', []);
    angular.module('preflight.services', []);

    angular.module('preflight', [
        'preflight.components',
        'preflight.services'
    ]);

    angular.module('umbraco').requires.push('preflight');

})();
(() => {

    function ctrl($scope, $timeout, editorState, preflightService, preflightHub) {

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
                $scope.model.badge = {};
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
                        .then(resp => {
                            // swallowed
                        });
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

    angular.module('preflight').controller('preflight.controller', ['$scope', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();
(() => {

    function notificationController(notificationsService) {

        this.saveCancelled = notificationsService.current[0].args.saveCancelled == 1;

        this.switch = n => {
            const tabScope = angular
                .element(document.querySelector('[data-element="sub-view-preflight"]')).scope();

            if (tabScope) {
                tabScope.$parent.clickNavigationItem(tabScope.item);
            }

            this.discard(n);
        };

        this.discard = n => {
            notificationsService.remove(n);
        };

    }

    // register controller 
    angular.module('preflight').controller('preflight.notification.controller', ['notificationsService', notificationController]);
})();
(() => {

    function ctrl(notificationsService, preflightService) {

        preflightService.getSettings()
            .then(resp => {
                this.settings = resp.data.settings;
                this.tabs = resp.data.tabs; 

                this.settings.forEach(v => {
                    if (v.view.indexOf('slider') !== -1) {
                        v.config = {
                            handle: 'round',
                            initVal1: v.alias === 'longWordSyllables' ? 5 : 65,
                            maxVal: v.alias === 'longWordSyllables' ? 10 : 100,
                            minVal: 0,
                            orientation: 'horizontal',
                            step: 1,
                            tooltip: 'always',
                            tooltipPosition: 'bottom',
                        };
                    } else if (v.view.indexOf('multipletextbox') !== -1) {

                        v.value = v.value.split(',').map(val => {
                            return { value: val };
                        }).sort((a, b) => a < b);

                        v.config = {
                            min: 0,
                            max: 0
                        };
                    }
                });
            });


        /**
         * 
         */
        this.saveSettings = () => {

            //const min = parseInt(getSettingByAlias('readabilityTargetMin').value);
            //const max = parseInt(getSettingByAlias('readabilityTargetMax').value);
            const min = 1;
            const max = 100;

            if (min < max) {

                if (min + 10 > max) {
                    notificationsService.warning('WARNING', 'Readability range is narrow');
                }

                // need to transform multitextbox values without digest
                // so must be a new object, not a reference
                const settingsToSave = JSON.parse(JSON.stringify(this.settings));

                settingsToSave.forEach(v => {
                    if (v.view.indexOf('multipletextbox') !== -1) {
                        v.value = v.value.map(o => o.value).join(',');
                    }
                });

                preflightService.saveSettings({ settings: settingsToSave, tabs: this.tabs })
                    .then(resp => {
                        resp.data ?
                            notificationsService.success('SUCCESS', 'Settings updated') :
                            notificationsService.error('ERROR', 'Unable to save settings');
                    });
            }
            else {
                notificationsService.error('ERROR',
                    'Unable to save settings - readability minimum cannot be greater than readability maximum');
            }
        };

    }

    angular.module('preflight').controller('preflight.settings.controller', ['notificationsService', 'preflightService', ctrl]);

})();
(() => {

    const template = `
        <div class="card {{ ::$ctrl.cardClass }}">
            <span class="card-score {{ ::$ctrl.cardScoreClass }}" ng-bind="::$ctrl.score"></span>
            <span class="card-title">
                {{ ::$ctrl.title }}<br />
                {{ ::$ctrl.subtitle }}
            </span>
        </div>`;

    function controller(localizationService) {
        this.cardClass = 'pass';
        this.cardScoreClass = 'pass-color';

        const init = () => {

            if (this.failed) {
                this.cardClass = 'fail';
                this.cardScoreClass = 'fail-color';
            }

            if (this.title[0] === '@') {
                localizationService.localize(this.title, this.tokens)
                    .then(localizedTitle => {
                        this.title = localizedTitle;
                    });
            }

            if (this.subtitle[0] === '@') {
                localizationService.localize(this.subtitle, this.tokens)
                    .then(localizedSubtitle => {
                        this.subtitle = localizedSubtitle;
                    });
            }
        };

        this.$onInit = () => {
            init();
        };
    };

    const component = {
        transclude: true,
        bindings: {
            title: '@?',
            subtitle: '@?',
            failed: '<',
            score: '<',
            tokens: '<'
        },
        template: template,
        controller: controller
    };

    controller.$inject = ['localizationService'];

    angular.module('preflight.components').component('preflightCard', component);

})();
(() => {

    /**
     * Directive used to render the heading for a plugin in the results view
     * Send a stringified array as the tokens attribute to replace %0%, %1% .. %n% in the localized string
     */

    const template = `
           <h5 ng-bind="::$ctrl.heading"></h5>
           <span ng-if="$ctrl.pass" ng-bind="::$ctrl.passText"></span>`;

    function controller(localizationService) {

        const init = () => {
            if (this.passText[0] === '@') {
                localizationService.localize(this.passText, this.tokens)
                    .then(localizedPassText => {
                        this.passText = localizedPassText;
                    });
            }

            if (this.heading[0] === '@') {
                localizationService.localize(this.heading, this.tokens)
                    .then(localizedHeading => {
                        this.heading = localizedHeading;
                    });
            }
        }

        this.$onInit = () => {
            init();
        }
    };

    const component = {
        transclude: true,
        bindings: {
            tokens: '<',
            passText: '@?',
            heading: '@?',
            pass: '<'
        },
        template: template,
        controller: controller
    };

    controller.$inject = ['localizationService'];

    angular.module('preflight.components').component('preflightResultIntro', component);

})();
(() => {

    const template = `
        <div class="state-icon {{ ::$ctrl.className }}">
            <i class="icon icon-{{ ::$ctrl.icon }}"></i>
        </div>`;

    function controller() {
        this.icon = 'power';
        this.className = 'disabled';

        this.$onInit = () => {
            if (!this.disabled) {
                this.icon = this.failed ? 'delete' : 'check';
                this.className = this.failed ? 'fail' : 'pass';
            }
        };
    };

    const component = {
        transclude: true,
        bindings: {
            failed: '<',
            disabled: '<'
        },
        template: template,
        controller: controller
    };

    angular.module('preflight.components').component('preflightStateIcon', component);

})();
(() => {

    const postSaveUrl = '/umbracoapi/content/postsave';
    let warningNotification = {
        key: 'preflight_notice'
    };

    function interceptor(notificationsService, $q, $injector) {
        return {
            request: request => {
                if (request.url.toLowerCase().indexOf(postSaveUrl) !== -1) {
                    $injector.invoke(['preflightService', s => {
                        s.getSettingValue('runPreflightOnSave')
                            .then(resp => {
                                if (resp.value === '1') {
                                    warningNotification.view = `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/views/warning.notification.html`;
                                    notificationsService.add(warningNotification);
                                }
                            });
                    }]);
                }

                return request || $q.when(request);
            },
            response: response => {
                try {
                    if (response.config.url.toLowerCase().indexOf(postSaveUrl) !== -1) {

                        const index = notificationsService.current.map(c => c.key === 'preflight_notice')
                            .indexOf(true);

                        if (index !== -1) {
                            notificationsService.remove(index);
                        }

                        if (response.data.notifications) {
                            
                            const notification = response.data.notifications.filter(f => f.header === Umbraco.Sys.ServerVariables.Preflight.ContentFailedChecks)[0];

                            if (notification) {
                                response.data.notifications = [];

                                notificationsService.add({
                                    view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/views/failed.notification.html`,
                                    args: { saveCancelled: notification.message.indexOf('_true') !== -1 }
                                });
                            }
                        }
                    }
                }
                catch (err) {
                    console.log(err.message);
                }

                return response || $q.when(response);
            }
        };
    }

    angular.module('preflight').factory('preflight.save.interceptor', ['notificationsService', '$q', '$injector', interceptor]);

    angular.module('preflight')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('preflight.save.interceptor');
        });

})();
(() => {

    const component = {
        transclude: true,
        bindings: {
            results: '<'
        },
        template: `
            <table class="linkhealth-result-table">
                <thead>
                    <tr><th>Link text</th> <th>Link target</th> <th>Link status</th></tr>
                </thead>
                <tr ng-repeat="link in $ctrl.results"><td ng-bind="link.text"></td><td ng-bind="link.href"></td><td ng-bind="link.status"></td></tr>
            </table>`
    };


    angular.module('preflight.components').component('linkhealthResult', component);

})();
(() => {

    function overlay($scope) {
        // there's nothing here
    }

    angular.module('umbraco').controller('readability.overlay.controller', ['$scope', overlay]);

    function ctrl($scope, editorService) {
        /**
        * Displays an overlay explaining what the readability test actually does
         * @param {any} e click event
        */

        this.help = e => {
            e.preventDefault();
            const helpOverlay = {
                view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/plugins/readability.overlay.html`,
                title: 'Readability',
                description: 'Why should I care?',
                text: $scope.model.description,
                close: () => {
                    editorService.close();
                }
            };

            editorService.open(helpOverlay);
        };
    }

    angular.module('umbraco').controller('readability.plugin.controller', ['$scope', 'editorService', ctrl]);
})();


(() => {

    function preflightHub($rootScope, $q, assetsService) {

        const scripts = [
            '/umbraco/lib/signalr/jquery.signalr.js',
            '/umbraco/backoffice/signalr/hubs'
        ];

        function initHub(callback) {
            if ($.connection === undefined) {
                const promises = [];
                scripts.forEach(script => {
                    promises.push(assetsService.loadJs(script));
                });

                $q.all(promises)
                    .then(() => {
                        hubSetup(callback);
                    });
            } else {
                hubSetup(callback);
            }
        }

        function hubSetup(callback) {

            const proxy = $.connection.preflightHub;

            const hub = {
                start: () => {
                    $.connection.hub.start();
                },
                on: (eventName, callback) => {
                    proxy.on(eventName, 
                        result => {
                            $rootScope.$apply(() => {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                },
                invoke: (methodName, callback) => {
                    proxy.invoke(methodName)
                        .done(result => {
                            $rootScope.$apply(() => {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                }
            };

            return callback(hub);
        }

        return {
            initHub: initHub
        };
    }

    angular.module('preflight.services').factory('preflightHub', ['$rootScope', '$q', 'assetsService', preflightHub]);

})();
(() => {

    function preflightService($http, umbRequestHelper) {

        const urlBase = Umbraco.Sys.ServerVariables.Preflight.ApiPath;

        const helpText = `
            <p>If your content is too difficult for your visitors to read, you're all going to have a bad time.</p>
            <p>The readability test runs your content through the Flesch reading ease algorithm to determine text complexity.</p>
            <h5>The algorithm</h5>
            <p><code>RE = 206.835 - (1.015 x ASL) - (84.6 x ASW)</code></p>
            <p>Where <code>RE</code> is Readability Ease, <code>ASL</code> is Average Sentence Length, and <code>ASW</code> is Average Syllables per Word</p>
            <p>The result is a number between 0 and 100, where a higher score means better readability, with a score between 60 and 69 largely considered acceptable.</p>
            <h5>Readability test results</h5>
            <p>As well as the Flesch score, the readability test returns sentence length; average syllables per word; and long or complex words;</p>`;

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke'
            );

        const service = {
            check: id => request('GET', `${urlBase}check/${id}`),

            checkDirty: data => request('POST', `${urlBase}checkdirty/`, data),

            getSettings: () => request('GET', `${urlBase}getSettings`),

            getSettingValue: alias => request('GET', `${urlBase}getSettingValue/${alias}`),

            saveSettings: settings => request('POST', `${urlBase}saveSettings`, settings),

            getHelpText: () => helpText
        };

        return service;
    }

    angular.module('preflight.services').service('preflightService', ['$http', 'umbRequestHelper', preflightService]);

})();