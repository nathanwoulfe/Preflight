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