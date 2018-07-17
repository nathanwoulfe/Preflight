(() => {

    function ctrl(notificationsService, preflightService) {

        this.content = preflightService.getHelpText();

        this.tabs = [
            {
                id: 101,
                label: 'Settings',
                alias: 'tab101',
                active: true
            }, {
                id: 102,
                label: 'Black/white lists',
                alias: 'tab102',
                active: false
            }, {
                id: 103,
                label: 'Autoreplace',
                alias: 'tab103',
                active: false
            }, {
                id: 104,
                label: 'Test types',
                alias: 'tab104',
                active: false
            }
        ];

        const getSettingByAlias = alias => this.settings.filter(v => v.alias === alias)[0];

        /**
         * 
         */
        const getSettings = () => {
            preflightService.getSettings()
                .then(resp => {
                    this.settings = resp.data;

                    this.settings.forEach(v => {
                        if (v.view === 'slider') {
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
                        } else if (v.view === 'multipletextbox') {

                            v.value = v.value.split(',').map(val => {
                                return { value: val }
                            }).sort((a, b) => a < b);

                            v.config = {
                                min: 0,
                                max: 0
                            }
                        }
                    });
                });
        };

        /**
         * 
         */
        this.saveSettings = () => {

            const min = getSettingByAlias('readabilityTargetMin').value;
            const max = getSettingByAlias('readabilityTargetMax').value;
             
            if (min < max) {

                if (min + 10 > max) {
                    notificationsService.warning('WARNING', 'Readability range is narrow');
                }

                // need to transform multitextbox values without digest
                // so must be a new object, not a reference
                const settingsToSave = JSON.parse(JSON.stringify(this.settings));

                settingsToSave.forEach(v => {
                    if (v.view === 'multipletextbox') {
                        v.value = v.value.map(o => o.value).join(',');
                    }
                });

                preflightService.saveSettings(settingsToSave)
                    .then(resp => {
                        resp.data
                            ? notificationsService.success('SUCCESS', 'Settings updated')
                            : notificationsService.error('ERROR', 'Unable to save settings');
                        getSettings();
                    });
            } else {
                notificationsService.error('ERROR',
                    'Unable to save settings - readability minimum cannot be greater than readability maximum');
            }
        };

        getSettings();
    }

    angular.module('umbraco').controller('preflight.dashboard.controller', ['notificationsService', 'preflightService', ctrl]);

})();