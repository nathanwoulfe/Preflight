/*! preflight - v1.3.5-build23 - 2020-07-30
 * Copyright (c) 2020 Nathan Woulfe;
 * Licensed MIT
 */

(() => {

    angular.module('preflight.components', []);
    angular.module('preflight.services', []);

    angular.module('preflight', [
        'preflight.components',
        'preflight.services' 
    ]); 

    //        .config(['$provide', $provide => {
    //    $provide.decorator("$rootScope", function ($delegate) {
    //        var Scope = $delegate.constructor;
    //        var origBroadcast = Scope.prototype.$broadcast; 
    //        var origEmit = Scope.prototype.$emit;

    //        Scope.prototype.$broadcast = function () {
    //            console.log("$broadcast was called on $scope " + Scope.$id + " with arguments:", arguments);
    //            return origBroadcast.apply(this, arguments);
    //        };
    //        Scope.prototype.$emit = function () {
    //            console.log("$emit was called on $scope " + Scope.$id + " with arguments:", arguments);
    //            return origEmit.apply(this, arguments);
    //        };
    //        return $delegate;
    //    });
    //}]);

    angular.module('umbraco').requires.push('preflight');

})();
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
        this.percentageDone = 20;
        this.progressStep = 0;

        $scope.model.badge = {
            type: 'info'
        };


        /**
         * 
         * @param {any} arr
         */
        const joinList = arr => {
            let outStr;
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
        const getHash = s => s ? s.split('').reduce((a, b) => {
            a = (a << 5) - a + b.charCodeAt(0);
            return a & a;
        }, 0) : 1;


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
                p.disabled = p.failedCount === -1;
            }

            this.showSuccessMessage = !this.results.failed && !this.noTests;
            this.done = true;
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
            let totalTestsRun = 0;
            let existingProp = this.results.properties.find(x => x.label === data.label);

            if (existingProp) {
                existingProp = Object.assign(existingProp, data);
                existingProp.loading = false;
                newProp = false;
            }            

            // a new property will have a temporary placeholder - remove it
            // _temp ensures grid with multiple editors only removes the correct temp entry
            if (newProp && !data.remove && data.failedCount !== -1) {
                const tempIndex = this.results.properties.findIndex(p => p.name === `${data.name}_temp`);
                if (tempIndex !== -1) {
                    this.results.properties.splice(tempIndex, 1);
                }
                this.results.properties.push(data);
            }

            this.results.properties = this.results.properties.filter(x => x.remove === false);
            this.results.properties = this.results.properties.filter(x => x.failedCount > -1);

            this.results.failedCount = this.results.properties.reduce((prev, cur) => {
                totalTestsRun += cur.totalTests;
                return prev + cur.failedCount;
            }, 0);

            this.results.failed = this.results.failedCount > 0;            
            this.propsBeingCheckedStr = joinList(propsBeingChecked.splice(propsBeingChecked.indexOf(data.name), 1));
            this.percentageFailed = (totalTestsRun - this.results.failedCount) / totalTestsRun * 100;
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
                    this.done = false;

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
        const initSignarlR = () => {

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
                        checkDirty(); // builds initial hash array, but won't run anything
                        preflightService.check(editorState.current.id);
                    });
                });
            });
        };

        /**
         * Stores a reference collection of tracked properties
         */
        const activeVariant = editorState.current.variants.find(x => x.active);
        let propertiesToTrack = [];

        if (activeVariant) {
            activeVariant.tabs.forEach(x => {
                propertiesToTrack = propertiesToTrack.concat(x.properties.map(x => {
                    if (validPropTypes.includes(x.editor)) {
                        return {
                            editor: x.editor,
                            alias: x.alias,
                            label: x.label
                        };
                    }
                })).filter(x => x);
            });

            // array will have length, as app is only sent on types with testable properties
            if (propertiesToTrack.length) {
                initSignarlR();
            }
        }
    }

    angular.module('preflight').controller('preflight.controller', ['$scope', '$rootScope', '$element', '$timeout', 'editorState', 'preflightService', 'preflightHub', ctrl]);

})();
(() => {

    function notificationController($rootScope, notificationsService, editorState) {

        this.saveCancelled = +notificationsService.current[0].args.saveCancelled === 1;

        this.switch = n => {
            $rootScope.$emit('showPreflight', { nodeId: editorState.current.id });
            this.discard(n);
        };

        this.discard = n => {
            notificationsService.remove(n);
        };

    }

    // register controller 
    angular.module('preflight').controller('preflight.notification.controller', ['$rootScope', 'notificationsService', 'editorState', notificationController]);
})();
(() => {

    function ctrl($scope, notificationsService, preflightService) {

        const watchTestableProperties = () => {
            let propertiesToModify = this.settings.filter(x => x.alias.indexOf('PropertiesToTest') !== -1 && x.alias !== 'propertiesToTest');
            $scope.$watch(() => this.settings.find(x => x.alias === 'propertiesToTest').value, newVal => {
                if (newVal) {
                    for (let prop of propertiesToModify) {
                        // use the prop alias to find the checkbox set
                        for (let checkbox of document.querySelectorAll(`umb-checkbox[name*="${prop.alias}"]`)) {
                            checkbox.querySelector('.umb-form-check').classList[newVal.indexOf(checkbox.getAttribute('value')) === -1 ? 'add' : 'remove']('pf-disabled'); 
                        }
                    }
                }
            }, true);
        };

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

                        v.validation = {};
                    } else if (v.view.indexOf('checkboxlist') !== -1) {

                        v.value = v.value.split(',');

                        v.config = {
                            items: v.prevalues
                        };
                    }
                });

                watchTestableProperties();

            });


        /**
         * 
         */
        this.saveSettings = () => {

            const min = parseInt(this.settings.filter(x => x.alias === 'readabilityTargetMinimum')[0].value);
            const max = parseInt(this.settings.filter(x => x.alias === 'readabilityTargetMaximum')[0].value); 

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
                    } else if (v.view.indexOf('checkboxlist') !== -1) {
                        v.value = v.value.join(',');
                    }
                });

                preflightService.saveSettings(settingsToSave, this.tabs)
                    .then(resp => {
                        resp.data
                            ? notificationsService.success('SUCCESS', 'Settings updated')
                            : notificationsService.error('ERROR', 'Unable to save settings');

                        // reset dashboard form state
                        var formScope = angular.element(document.querySelector('[name="dashboardForm"]')).scope();
                        formScope.dashboardForm.$setPristine();
                    });
            }
            else {
                notificationsService.error('ERROR',
                    'Unable to save settings - readability minimum cannot be greater than readability maximum');
            }
        };
    }

    angular.module('preflight').controller('preflight.settings.controller', ['$scope', 'notificationsService', 'preflightService', ctrl]);

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
    }

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
    function ProgressCircleDirective() {

        function link(scope, element) {
            function onInit() {

                // making sure we get the right numbers
                let percent = Math.round(scope.percentage);
                percent = percent > 100 ? 100 : percent || 0;

                // calculating the circle's highlight
                const r = element.find('.umb-progress-circle__highlight').attr('r');
                const pathLength = r * Math.PI * 2 * percent / 100;

                // Full circle length
                scope.dashArray = `${pathLength},100`;

                // set font size
                scope.percentageSize = scope.size * 0.3 + 'px';

                // use rounded percentage
                scope.label = `${percent}%`;
            }

            scope.$watch('percentage', onInit);
        }

        let  directive = {
            restrict: 'E',
            replace: true,
            template: `
                <div class="umb-progress-circle preflight-progress" ng-style="{'width': size, 'height': size }"> {{ percent }}
                    <svg class="umb-progress-circle__view-box" viewBox="0 0 33.83098862 33.83098862"> 
                        <circle class="umb-progress-circle__highlight--{{ background }}" cx="16.91549431" cy="16.91549431" r="15.91549431" fill="none" stroke-width=".5"></circle>
                        <circle class="umb-progress-circle__highlight umb-progress-circle__highlight--{{ foreground }}" 
                            cx="16.91549431" cy="16.91549431" r="15.91549431" stroke-linecap="round" fill="none" stroke-width="2" stroke-dasharray="{{ dashArray }}"></circle> 
                    </svg> 
                    <div ng-style="{'font-size': percentageSize}" class="umb-progress-circle__percentage">
                        {{ label }}
                        <small>pass rate</small>                
                    </div>
                </div>`,
            scope: {
                size: '@?',
                percentage: '=',
                done: '@',
                foreground: '@',
                background: '@'
            },
            link: link
        };

        return directive;
    }

    angular.module('preflight.components').directive('progressCircle', ProgressCircleDirective);
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
        };

        this.$onInit = () => {
            init();
        };
    }

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
    }

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

    function interceptor(notificationsService, $q, $injector) {

        const checkGroup = userGroupOptInOut => {
            // use the stored value to get the corresponding key from the setting's prevalues (which is value,key paring of all groups)
            var enabledGroups = userGroupOptInOut.prevalues.filter(x => userGroupOptInOut.value.includes(x.value)).map(x => x.key);

            $injector.invoke(['authResource', authResource => {
                authResource.getCurrentUser()
                    .then(currentUser => {
                        if (enabledGroups.some(x => currentUser.userGroups.includes(x))) {
                            notificationsService.add({
                                key: 'preflight_notice',
                                view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/views/warning.notification.html`
                            });
                        }
                    });
            }]);
        };

        return {
            request: request => {
                if (request.url.toLowerCase().indexOf(postSaveUrl) !== -1) {
                    $injector.invoke(['preflightService', s => {
                        s.getSettings()
                            .then(resp => {                   
                                const settings = resp.data.settings;
                                const runOnSave = settings.find(x => x.alias === 'runPreflightOnSave'); 
                                if (runOnSave && runOnSave.value === '1') {
                                    const userGroupOptInOut = settings.find(x => x.alias === 'userGroupOptInOut');
                                    checkGroup(userGroupOptInOut);
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

    angular.module('preflight')
        .factory('preflight.save.interceptor', ['notificationsService', '$q', '$injector', interceptor])
        .config($httpProvider => $httpProvider.interceptors.push('preflight.save.interceptor'));

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
                size: 'small',
                text: $scope.model.description,
                close: () => editorService.close()                
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
                scripts.forEach(script =>
                    promises.push(assetsService.loadJs(script)));

                $q.all(promises)
                    .then(() => hubSetup(callback));
                    
            } else {
                hubSetup(callback);
            }
        }

        function hubSetup(callback) {

            const proxy = $.connection.preflightHub;

            const hub = {
                start: callback => {
                    $.connection.hub.start();
                    if (callback) {
                        callback();
                    }
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

            saveSettings: (settings, tabs) => request('POST', `${urlBase}saveSettings`, {
                settings: settings, 
                tabs: tabs
            }),

            getHelpText: () => helpText
        };

        return service;
    }

    angular.module('preflight.services').service('preflightService', ['$http', 'umbRequestHelper', preflightService]);

})();