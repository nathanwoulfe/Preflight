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