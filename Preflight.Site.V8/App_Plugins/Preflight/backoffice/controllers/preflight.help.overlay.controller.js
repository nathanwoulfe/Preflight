(() => {

  function ctrl(preflightService) {
    this.text = preflightService.getHelpText();
  }

  angular.module('umbraco').controller('preflight.help.overlay.controller', ['preflightService', ctrl]);

})();