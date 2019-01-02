(() => {

    function interceptor($rootScope) {
        return {
            response: response => {
                try {
                    if (response.config.url.toLowerCase().indexOf('/umbracoapi/content/postsave') !== -1) {
                        if (response.data.notifications) {

                            const notification = response.data.notifications.filter(f => f.header === Umbraco.Sys.ServerVariables.preflight.contentFailedChecks)[0];

                            if (notification) {
                                const preflightResponse = JSON.parse(notification.message);

                                notification.message = 'Check the Preflight content app for more details';

                                $rootScope.preflightResult = preflightResponse;
                            }
                        }
                    }
                }
                catch (err) {
                    console.log(err.message);
                }

                return response;
            }
        };
    }

    angular.module('umbraco').factory('preflight.save.interceptor', ['$rootScope', interceptor]);

    angular.module('umbraco')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('preflight.save.interceptor');
        });

})();