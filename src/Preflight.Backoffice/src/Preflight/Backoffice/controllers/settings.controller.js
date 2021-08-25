import { constants } from '../constants';

export class SettingsController {

    static controllerName = 'preflight.settings.controller';

    $scope;
    $q;
    languageResource;
    notificationsService;
    preflightService;

    languages = [];
    tabs = [];
    settings = [];
    currentLanguage;

    languageChangeWatcher;
    testablePropertiesWatcher;

    constructor($scope, $q, notificationsService, languageResource, preflightService) {
        this.$scope = $scope;
        this.$q = $q;
        this.notificationsService = notificationsService;
        this.preflightService = preflightService;
        this.languageResource = languageResource;

        this.languageChangeWatcher = $scope.$watch(() => this.currentLanguage, (newLang, oldLang) => {
            // update settings to only include the current variant
            if (newLang && newLang !== oldLang) {
                this.settings.forEach(s => {
                    const syncSetting = this.settingsSyncModel.find(x => x.alias === s.alias);

                    // manage old language by updating the sync settings model,
                    // ensuring the value is an object
                    if (oldLang) {
                        if (!syncSetting.value) {
                            syncSetting.value = {};
                        }
                        syncSetting.value[oldLang] = s.value;
                    }

                    // get the value for the new language and update the settings model
                    if (syncSetting.value && syncSetting.value[newLang]) {
                        s.value = syncSetting.value[newLang];
                    } else {
                        // set the value to a sensible default - array if type is checkboxlist
                        s.value = s.view.includes(constants.checkboxlist) || s.view.includes(constants.multipletextbox) ? [] : null;
                    }
                });
            }
        });
    }

    watchTestableProperties = () => {
        this.testablePropertiesWatcher = this.$scope.$watch(() => this.settings.find(x => x.alias === 'propertiesToTest').value, newVal => {
            if (newVal) {
                let propertiesToModify = this.settings.filter(x => x.alias.includes('PropertiesToTest') && x.alias !== 'propertiesToTest');

                for (let prop of propertiesToModify) {
                    // use the prop alias to find the checkbox set
                    for (let checkbox of document.querySelectorAll(`umb-checkbox[name*="${prop.alias}"]`)) {
                        checkbox.querySelector('.umb-form-check').classList[newVal.indexOf(checkbox.getAttribute('value')) === -1 ? 'add' : 'remove']('pf-disabled');
                    }
                }
            }
        }, true);
    };

    $onDestroy() {
        this.languageChangeWatcher();
        this.testablePropertiesWatcher();
    }

    $onInit() {
        const promises = [
            this.preflightService.getSettings(),
            this.languageResource.getAll(),
        ];

        this.$q.all(promises)
            .then(resp => {
                this.settingsSyncModel = resp[0].data.settings;
                this.settings = JSON.parse(JSON.stringify(this.settingsSyncModel));

                this.tabs = resp[0].data.tabs;

                this.languages = resp[1];
                const currentLanguage = this.languages.find(x => x.isDefault).culture;

                this.settingsSyncModel.forEach(v => {
                    if (v.view.includes(constants.multipletextbox) && v.value) {
                        for (let [key, value] of Object.entries(v.value)) {
                            v.value[key] = value.split(',').map(val => ({ value: val })).sort((a, b) => a < b);
                        }
                    } else if (v.view.includes(constants.checkboxlist) && v.value) {
                        for (let [key, value] of Object.entries(v.value)) {
                            v.value[key] = value.split(',');
                        }
                    }
                })

                this.settings.forEach(v => {
                    const syncSetting = this.settingsSyncModel.find(x => x.alias === v.alias);
                    v.value = syncSetting.value ? syncSetting.value[currentLanguage] : null;

                    if (v.view.includes('slider')) {
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
                    } else if (v.view.includes(constants.multipletextbox)) {
                        v.config = {
                            min: 0,
                            max: 0
                        };
                        v.validation = {};
                    } else if (v.view.includes(constants.checkboxlist)) {
                        v.config = {
                            items: v.prevalues
                        };
                    }
                });

                this.currentLanguage = currentLanguage;
                this.watchTestableProperties();
            });
    }

    /**
     * 
     */
    saveSettings = () => {

        // ensure the current language is correctly mapped to the sync model
        this.settings.forEach(s => {
            const syncSetting = this.settingsSyncModel.find(x => x.alias === s.alias);

            if (!syncSetting.value) {
                syncSetting.value = {};
            }

            syncSetting.value[this.currentLanguage] = s.value ? s.value :
                s.view.includes(constants.checkboxlist) || s.view.includes(constants.multipletextbox) ? [] : null;            
        });

        // ensure readability is valid
        let validRange = true;
        this.languages.forEach(l => {
            const culture = l.culture;

            const min = parseInt(this.settingsSyncModel.find(x => x.alias === 'readabilityTargetMinimum').value[culture]);
            const max = parseInt(this.settingsSyncModel.find(x => x.alias === 'readabilityTargetMaximum').value[culture]);

            if (min > max) {
                this.notificationsService.error('ERROR',
                    `Unable to save settings - readability minimum cannot be greater than readability maximum (${l.name})`);
                validRange = false;
            } else if (min + 10 > max) {
                this.notificationsService.warning('WARNING', `Readability range is narrow (${l.name})`);
            }
        });

        if (validRange) {
            // need to transform multitextbox values without digest
            // so must be a new object, not a reference
            const settingsToSave = JSON.parse(JSON.stringify(this.settingsSyncModel));

            settingsToSave.forEach(v => {
                if (v.view.includes(constants.multipletextbox)) {
                    for (let [key, value] of Object.entries(v.value)) {
                        v.value[key] = value.map(o => o.value).join(',');
                    }
                } else if (v.view.includes(constants.checkboxlist)) {
                    for (let [key, value] of Object.entries(v.value)) {
                        v.value[key] = value.join(',');
                    }
                }
            });

            this.preflightService.saveSettings(settingsToSave, this.tabs)
                .then(resp => {
                    resp.data
                        ? this.notificationsService.success('SUCCESS', 'Settings updated')
                        : this.notificationsService.error('ERROR', 'Unable to save settings');

                    // reset dashboard form state
                    this.$scope.preflightSettingsForm.$setPristine();
                });
        }
    }
}