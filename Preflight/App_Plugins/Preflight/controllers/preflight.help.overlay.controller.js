(function () {
  'use strict';

  function ctrl(preflightService) {
    var vm = this;
    vm.text = preflightService.getHelpText();
  }

  angular.module('umbraco').controller('preflight.help.overlay.controller', ['preflightService', ctrl]);

}());