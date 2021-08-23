class ResultIntro {
    /**
     * Directive used to render the heading for a plugin in the results view
     * Send a stringified array as the tokens attribute to replace %0%, %1% .. %n% in the localized string
     */

    static template = `
        <h5 ng-bind="::$ctrl.heading"></h5>
        <span ng-if="$ctrl.pass" ng-bind="::$ctrl.passText"></span>`;

    localizationService;

    constructor(localizationService) {
        this.localizationService = localizationService;
    }

    $onInit() {
        if (this.passText[0] === '@') {
            this.localizationService.localize(this.passText, this.tokens)
                .then(localizedPassText => this.passText = localizedPassText);
        }

        if (this.heading[0] === '@') {
            this.localizationService.localize(this.heading, this.tokens)
                .then(localizedHeading => this.heading = localizedHeading);
        }
    };
}

export const ResultIntroComponent = {
    transclude: true,
    name: 'resultIntro',
    bindings: {
        tokens: '<',
        passText: '@?',
        heading: '@?',
        pass: '<'
    },
    template: ResultIntro.template,
    controller: ResultIntro
};