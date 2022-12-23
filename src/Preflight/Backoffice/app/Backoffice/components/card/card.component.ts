class Card {

  static template = `
    <div class="card {{ ::$ctrl.cardClass }}">
        <span class="card-score {{ ::$ctrl.cardScoreClass }}" ng-bind="::$ctrl.score"></span>
        <span class="card-title">
            {{ ::$ctrl.title }}<br />
            {{ ::$ctrl.subtitle }}
        </span>
    </div>`;

  cardClass: string = 'pass';
  cardScoreClass: string = 'pass-color';
  failed!: boolean;
  title!: string;
  tokens!: Array<string>;
  subtitle!: string;

  constructor(private localizationService) { }

  $onInit = () => {
    if (this.failed) {
      this.cardClass = 'fail';
      this.cardScoreClass = 'fail-color';
    }

    if (this.title[0] === '@') {
      this.localizationService.localize(this.title, this.tokens)
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
  name: 'preflightCard',
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
