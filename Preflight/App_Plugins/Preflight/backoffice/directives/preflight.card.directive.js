(() => {

    const template = `
        <div class="card {{ cardClass }}">
            <span class="card-score {{ cardScoreClass }}" ng-bind="score"></span>
            <span class="card-title">
                {{ title }}<br />
                {{ subtitle }}
            </span>
        </div>`;

    function preflightCard(locale) {
        const dir = {
            restrict: 'E',
            scope: {
                title: '@?',
                subtitle: '@?',
                failed: '=',
                score: '=',
                tokens: '='
            },
            template: template,
            link: scope => {

                if (scope.title[0] === '@') {
                    locale.localize(scope.title, scope.tokens)
                        .then(localizedTitle => {
                            scope.title = localizedTitle;
                        });
                }

                if (scope.subtitle[0] === '@') {
                    locale.localize(scope.subtitle, scope.tokens)
                        .then(localizedSubtitle => {
                            scope.subtitle = localizedSubtitle;
                        });
                }

                scope.$watch('failed',
                    () => {
                        if (scope.failed === true) {
                            scope.cardClass = 'fail';
                            scope.cardScoreClass = 'fail-color';
                        } else if (scope.failed === false) {
                            scope.cardClass = 'pass';
                            scope.cardScoreClass = 'pass-color';
                        }
                    });
            }
        };

        return dir;
    }

    angular.module('preflight.directives').directive('preflightCard', ['localizationService', preflightCard]);

})();