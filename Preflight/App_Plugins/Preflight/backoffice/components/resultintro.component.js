(() => {

    /**
     * Directive used to render the heading for a plugin in the results view
     * Send a stringified array as the tokens attribute to replace %0%, %1% .. %n% in the localized string
     */

    const template = `
           <h5 ng-bind="::$ctrl.heading"></h5>
           <span ng-if="$ctrl.pass" ng-bind="::$ctrl.passText"></span>`;

    function controller(localizationService) {

        const init = () => {
            if (this.passText[0] === '@') {
                localizationService.localize(this.passText, this.tokens)
                    .then(localizedPassText => {
                        this.passText = localizedPassText;
                    });
            }

            if (this.heading[0] === '@') {
                localizationService.localize(this.heading, this.tokens)
                    .then(localizedHeading => {
                        this.heading = localizedHeading;
                    });
            }
        }

        this.$onInit = () => {
            init();
        }
    };

    const component = {
        transclude: true,
        bindings: {
            tokens: '<',
            passText: '@?',
            heading: '@?',
            pass: '<'
        },
        template: template,
        controller: controller
    };

    controller.$inject = ['localizationService'];

    angular.module('preflight.components').component('preflightResultIntro', component);

})();