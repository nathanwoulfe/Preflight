(() => {

    function linkhealthResult() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                results: '='
            },
            template: `
                <table class="linkhealth-result-table">
                    <thead>
                        <tr><th>Link text</th> <th>Link target</th> <th>Link status</th></tr>
                    </thead>
                    <tr ng-repeat="link in results"><td ng-bind="link.text"></td><td ng-bind="link.href"></td><td ng-bind="link.status"></td></tr>
                </table>`
        };
    }

    angular.module('preflight.directives').directive('linkhealthResult', linkhealthResult);

})();