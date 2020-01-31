(() => {

    function ctrl(notificationsService, preflightService) {

        const getCheckboxListModel = o =>
            o.split(',')
                .map((val, i) => {
                    return {
                        value: val,
                        key: val,
                        sortOrder: i
                    };
                })
                .sort((a, b) => a < b);

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
                            items: getCheckboxListModel(v.prevalues)
                        };
                    }
                });
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
                        resp.data ?
                            notificationsService.success('SUCCESS', 'Settings updated') :
                            notificationsService.error('ERROR', 'Unable to save settings');

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

    angular.module('preflight').controller('preflight.settings.controller', ['notificationsService', 'preflightService', ctrl]);

})();