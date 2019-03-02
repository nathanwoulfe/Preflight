(() => {

    const template = `
        <div class="card {{ ::$ctrl.cardClass }}">
            <span class="card-score {{ ::$ctrl.cardScoreClass }}" ng-bind="::$ctrl.score"></span>
            <span class="card-title">
                {{ ::$ctrl.title }}<br />
                {{ ::$ctrl.subtitle }}
            </span>
        </div>`;

    function controller(localizationService) {
        this.cardClass = 'pass';
        this.cardScoreClass = 'pass-color';

        const init = () => {

            if (this.failed) {
                this.cardClass = 'fail';
                this.cardScoreClass = 'fail-color';
            }

            if (this.title[0] === '@') {
                localizationService.localize(this.title, this.tokens)
                    .then(localizedTitle => {
                        this.title = localizedTitle;
                    });
            }

            if (this.subtitle[0] === '@') {
                localizationService.localize(this.subtitle, this.tokens)
                    .then(localizedSubtitle => {
                        this.subtitle = localizedSubtitle;
                    });
            }
        };

        this.$onInit = () => {
            init();
        };
    };

    const component = {
        transclude: true,
        bindings: {
            title: '@?',
            subtitle: '@?',
            failed: '<',
            score: '<',
            tokens: '<'
        },
        template: template,
        controller: controller
    };

    controller.$inject = ['localizationService'];

    angular.module('preflight.components').component('preflightCard', component);

})();