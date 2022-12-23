(() => {

    const postSaveUrl = '/umbracoapi/content/postsave';
    let presaveText;

    function interceptor(notificationsService, overlayService, editorState, $rootScope, $q, $injector) {

        const checkGroup = (userGroupOptInOut, culture) => {
            var value = userGroupOptInOut.value[culture];
            var enabledGroups = userGroupOptInOut.prevalues.filter(x => value.includes(x.value)).map(x => x.key);

            let localizationService;
            $injector.invoke(['localizationService', service => localizationService = service]);

            $injector.invoke(['authResource', authResource => {
                const promises = [
                    authResource.getCurrentUser(),
                    localizationService.localize('preflight_presaveText'),
                ];

                $q.all(promises)
                    .then(resp => {
                        const currentUser = resp[0];
                        presaveText = resp[1];

                        if (enabledGroups.some(x => currentUser.userGroups.includes(x))) {
                            notificationsService.info('Preflight', presaveText);
                        }
                    });
            }]);
        };

        return {
            request: request => {
                if (request.url.toLowerCase().includes(postSaveUrl)) {
                    $injector.invoke(['preflightService', s => {
                        s.getSettings()
                            .then(resp => {
                                const settings = resp.data.settings;

                                // settings values are a dictionary keyed by culture
                                const variantsToSave = request.data.value.variants.filter(v => v.save);
                                const preflightVars = Umbraco.Sys.ServerVariables.Preflight;

                                variantsToSave.forEach(variant => {
                                    const culture = variant.language ? variant.language.culture : preflightVars.DefaultCulture;
                                    const runOnSave = settings.find(x => x.guid === preflightVars.SettingsGuid.BindSaveHandler);

                                    if (runOnSave && runOnSave.value[culture] === '1') {
                                        const userGroupOptInOut = settings.find(x => x.guid === preflightVars.SettingsGuid.UserGroupOptIn);
                                        checkGroup(userGroupOptInOut, culture);
                                    }
                                });
                            });
                    }]);
                }

                return request || $q.when(request);
            },
            response: response => {
                try {
                    if (response.config.url.toLowerCase().includes(postSaveUrl)) {

                        const index = notificationsService.current.findIndex(x => x.message === presaveText);
                        if (index > -1) {
                            setTimeout(() => notificationsService.remove(index), 1500);
                        }

                        if (response.data.notifications) {

                            const notification = response.data.notifications.filter(f => f.header === Umbraco.Sys.ServerVariables.Preflight.ContentFailedChecks)[0];

                            if (notification) {
                                response.data.notifications = [];

                                overlayService.open({
                                    view: `${Umbraco.Sys.ServerVariables.Preflight.PluginPath}/views/failed.notification.html`,
                                    submitButtonLabelKey: 'preflight_review',
                                    hideHeader: true,
                                    saveCancelled: notification.message.includes('_True'),
                                    submit: () => {
                                        $rootScope.$emit('showPreflight', { nodeId: editorState.current.id });
                                        overlayService.close();
                                    },
                                    close: () => overlayService.close()
                                });
                            }
                        }
                    }
                }
                catch (err: any) {
                    console.log(err.message);
                }

                return response || $q.when(response);
            }
        };
    }

    angular.module('preflight')
        .factory('preflight.save.interceptor', ['notificationsService', 'overlayService', 'editorState', '$rootScope', '$q', '$injector', interceptor])
        .config($httpProvider => $httpProvider.interceptors.push('preflight.save.interceptor'));

})();
