class Card {

    template = `
        <div class="card {{ ::$ctrl.cardClass }}">
            <span class="card-score {{ ::$ctrl.cardScoreClass }}" ng-bind="::$ctrl.score"></span>
            <span class="card-title">
                {{ ::$ctrl.title }}<br />
                {{ ::$ctrl.subtitle }}
            </span>
        </div>`;

    cardClass = 'pass';
    cardScoreClass = 'pass-color';

    localizationService;

    constructor(localizationService) {
        this.localizationService = localizationService;
    }

    $onInit() {

        if (this.failed) {
            this.cardClass = 'fail';
            this.cardScoreClass = 'fail-color';
        }

        if (this.title[0] === '@') {
            localizationService.localize(this.title, this.tokens)
                .then(localizedTitle => this.title = localizedTitle);
        }

        if (this.subtitle[0] === '@') {
            this.localizationService.localize(this.subtitle, this.tokens)
                .then(localizedSubtitle => this.subtitle = localizedSubtitle);
        }
    };
}

export const CardComponent = {
    transclude: true,
    name: 'card',
    bindings: {
        title: '@?',
        subtitle: '@?',
        failed: '<',
        score: '<',
        tokens: '<'
    },
    template: Card.template,
    controller: Card
};
