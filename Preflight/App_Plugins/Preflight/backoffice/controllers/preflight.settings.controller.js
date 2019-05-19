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