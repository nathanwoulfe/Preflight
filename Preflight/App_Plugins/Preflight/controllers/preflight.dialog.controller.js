(function () {
  'use strict';

  function ctrl($scope, editorState, appState, preflightService) {

    var vm = this,
      currentNodeId = $scope.dialogOptions.currentNode ? $scope.currentNode.id : -1;

    vm.loading = true;

    /**
     * 
     */
    function preflight() {
      if (currentNodeId !== -1) {
        preflightService.check(currentNodeId)
          .then(function (resp) {
            if (resp.status === 200) {
              vm.properties = resp.data.properties;
              checkProperties();
              vm.loading = false;
            }
          });
      }
      else {
        vm.properties = $scope.dialogOptions.results.properties;
        checkProperties();
        vm.loading = false;
      }
    }

    /**
     * 
     */
    function checkProperties() {

      vm.failedReadability = vm.properties.filter(function (p) {
        return p.readability.score < vm.readabilityTargetMin || p.readability.score > vm.readabilityTargetMax;
      });

      vm.brokenLinks = vm.properties.filter(function (p) {
        return p.links.length;
      });

      vm.blacklist = vm.properties.filter(function (p) {
        return p.readability.blacklist.length;
      });
    }

    /**
     * 
     */
    function readabilityHelp() {
      vm.overlay = {
        view: '../app_plugins/preflight/views/help.overlay.html',
        show: true,
        title: 'Readability',
        subtitle: 'Why should I care?',
        text: 'Text, yo',
        close: function (model) {
          vm.overlay.show = false;
          vm.overlay = null;
        }
      };
    }

    /**
     * 
     */
    function resultGradient() {
      return 'linear-gradient(90deg, #fe6561 ' + (vm.readabilityTargetMin - 15)
        + '%, #f9b945 ' + vm.readabilityTargetMin
        + '%, #35c786, #f9b945 ' + vm.readabilityTargetMax
        + '%, #fe6561 ' + (vm.readabilityTargetMax + 15) + '%)';
    }

    // this is from navigation service, needs to happen here as the modal may not be added via the service, so must be closed manually
    function close() {
      appState.setGlobalState('navMode', 'default');
      appState.setMenuState('showMenu', false);
      appState.setMenuState('showMenuDialog', false);
      appState.setSectionState('showSearchResults', false);
      appState.setGlobalState('stickyNavigation', false);
      appState.setGlobalState('showTray', false);

      if (appState.getGlobalState('isTablet') === true) {
        appState.setGlobalState('showNavigation', false);
      }

    }

    /**
     * 
     */
    preflightService.getSettings()
      .then(function (resp) {
        var settings = resp.data;

        resp.data.forEach(function (v) {
          vm[v.alias] = v.value;
        });

        preflight();
      });

    angular.extend(vm, {
      resultGradient: resultGradient,
      readabilityHelp: readabilityHelp,
      close: close
    });
  }

  angular.module('umbraco').controller('preflight.dialog.controller', ['$scope', 'editorState', 'appState', 'preflightService', ctrl]);

}());