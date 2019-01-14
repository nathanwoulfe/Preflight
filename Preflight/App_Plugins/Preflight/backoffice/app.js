(() => {

    angular.module('preflight.directives', []);
    angular.module('preflight.services', []);

    angular.module('preflight', [
        'preflight.directives',
        'preflight.services'
    ]);

    angular.module('umbraco').requires.push('preflight');

})();