(() => {

    /**
     * Directive used to render the heading for a plugin in the results view
     * Send a stringified array as the tokens attribute to replace %0%, %1% .. %n% in the localized string
     */

    const template = `
           <h5 ng-bind="heading"></h5>
           <span ng-if="pass" ng-bind="passText"></span>`;
     
    function preflightResultIntro(locale) {
        const dir = {
            restrict: 'E',
            scope: {
                tokens: '=',
                passText: '@?',
                heading: '@?',
                pass: '='
            },
            template: template,
            link: scope => {

                if (scope.passText[0] === '@') {
                    locale.localize(scope.passText, scope.tokens)
                        .then(localizedPassText => {
                            scope.passText = localizedPassText;
                        });
                }

                if (scope.heading[0] === '@') {
                    locale.localize(scope.heading, scope.tokens)
                        .then(localizedHeading => {
                            scope.heading = localizedHeading;
                        });
                }
            }
        };

        return dir;
    }

    angular.module('preflight.directives').directive('preflightResultIntro', ['localizationService', preflightResultIntro]);

})();