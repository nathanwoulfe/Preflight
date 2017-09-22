(function () {
  'use strict';

  function ctrl(notificationsService, preflightService) {    

    var vm = this;

    function getSettingByAlias(alias) {
      return vm.settings.filter(function (v) {
        return v.alias === alias;
      })[0];
    }

    function saveSettings() {

      var min = getSettingByAlias('readabilityTargetMin').value;
      var max = getSettingByAlias('readabilityTargetMax').value;

      vm.settings.forEach(function (v) {
        if (v.view === 'multipletextbox') {
          v.value = v.value.map(function (o) {
            return o.value
          }).join(',');
        }
      });

      if (min < max) {

        if (min + 10 > max) {
          notificationsService.warning('WARNING', 'Readability range is narrow');
        }

        preflightService.saveSettings(vm.settings)
          .then(function (resp) {
            resp.data ? notificationsService.success('SUCCESS', 'Settings updated') : notificationsService.error('ERROR', 'Unable to save settings');
            getSettings();
          });
      }
      else {
        notificationsService.error('ERROR', 'Unable to save settings - readability minimum cannot be greater than readability maximum');
      }
    }

    function getSettings() {
      preflightService.getSettings()
        .then(function (resp) {
          vm.settings = resp.data;

          vm.settings.forEach(function (v) {
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

              v.value = v.value.split(',').map(function (val) {
                return { value: val }
              }).sort(function (a, b) { return a < b });

              v.config = {
                min: 0,
                max: 0
              }
            }
          });
        });
    }

    angular.extend(vm, {
      saveSettings: saveSettings,
      content: preflightService.getHelpText(),
      tabs: [{
        id: 101,
        label: "Settings",
        alias: "tab101",
        active: true
      }, {
        id: 102,
        label: "Black/white lists",
        alias: "tab102",
        active: false
      }]
    });

    getSettings();
  }

  angular.module('umbraco').controller('preflight.dashboard.controller', ['notificationsService', 'preflightService', ctrl]);

}());