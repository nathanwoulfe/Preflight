(() => {

    angular.module('preflight.components', []);
    angular.module('preflight.services', []);

    angular.module('preflight', [
        'preflight.components',
        'preflight.services'
    ]);

    angular.module('umbraco').requires.push('preflight');

})();