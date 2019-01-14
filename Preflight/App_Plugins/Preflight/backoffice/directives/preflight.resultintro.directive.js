(() => {

    const template = `
           <h5 ng-bind="heading"></h5>
           <span ng-if="!active">This test is currently disabled.</span>
           <span ng-if="active && pass" ng-bind="passText"></span>`;

    function preflightResultIntro() {
        const dir = {
            restrict: 'E',
            scope: {
                active: '=',
                passText: '@?',
                heading: '@?',
                pass: '='
            },
            template: template
        };

        return dir;
    }

    angular.module('preflight.directives').directive('preflightResultIntro', preflightResultIntro);

})();