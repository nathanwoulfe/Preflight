(() => {

    function interceptor($rootScope, notificationsService) {
        return {
            response: response => {
                try {
                    if (response.config.url.toLowerCase().indexOf('/umbracoapi/content/postsave') !== -1) {
                        if (response.data.notifications) {

                            const notification = response.data.notifications.filter(f => f.header === Umbraco.Sys.ServerVariables.preflight.contentFailedChecks)[0];

                            if (notification) {
                                const preflightResponse = JSON.parse(notification.message);
                                $rootScope.preflightResult = preflightResponse;

                                response.data.notifications = [];

                                notificationsService.add({
                                    view: `${Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath}/preflight/backoffice/views/failed.notification.html`,
                                    args: { saveCancelled: preflightResponse.settings.cancelSaveOnFail }
                                });
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

    angular.module('umbraco').factory('preflight.save.interceptor', ['$rootScope', 'notificationsService', interceptor]);

    angular.module('umbraco')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('preflight.save.interceptor');
        });

})();